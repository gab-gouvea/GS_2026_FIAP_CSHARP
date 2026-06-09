using SwarmBuild.Aplicacao.Interfaces;
using SwarmBuild.Dominio.Entidades;
using SwarmBuild.Dominio.Enums;

namespace SwarmBuild.Aplicacao.Servicos;

/// <summary>
/// Servico que roda em background detectando robos offline e disparando realocacao.
/// Equivalente ao @Scheduled do Spring - usa System.Threading.Timer.
/// </summary>
public sealed class MonitorDeFalhasService : IDisposable
{
    public const int TimeoutSegundosPadrao = 60;
    public const int IntervaloVerificacaoMsPadrao = 10_000;

    private readonly IRepositorioRobo _repoRobo;
    private readonly IRepositorioTarefa _repoTarefa;
    private readonly IRepositorioAlerta _repoAlerta;
    private readonly IOrquestradorEnxame _orquestrador;
    private readonly IRelogio _relogio;

    private int _timeoutSegundos = TimeoutSegundosPadrao;
    private Timer? _timer;
    private bool _executandoAgora;
    private bool _ativo;

    public int TimeoutSegundos
    {
        get => _timeoutSegundos;
        set => _timeoutSegundos = value > 0 ? value : TimeoutSegundosPadrao;
    }

    public event Action<string>? AoLogar;
    public event Action<Robo, int>? AoDetectarFalha;

    public MonitorDeFalhasService(
        IRepositorioRobo repoRobo,
        IRepositorioTarefa repoTarefa,
        IRepositorioAlerta repoAlerta,
        IOrquestradorEnxame orquestrador,
        IRelogio relogio)
    {
        _repoRobo = repoRobo;
        _repoTarefa = repoTarefa;
        _repoAlerta = repoAlerta;
        _orquestrador = orquestrador;
        _relogio = relogio;
    }

    public void Iniciar(int intervaloMs = IntervaloVerificacaoMsPadrao)
    {
        if (_ativo) return;
        _ativo = true;
        _timer = new Timer(_ => DetectarRobosOffline(), null, intervaloMs, intervaloMs);
    }

    public void Parar()
    {
        _timer?.Dispose();
        _timer = null;
        _ativo = false;
    }

    public bool EstaAtivo => _ativo;

    /// <summary>
    /// Varre o repositorio em busca de robos sem heartbeat recente, marca como FALHA
    /// e realoca cada tarefa em execucao deles. Pode ser chamado manualmente para forcar
    /// a verificacao (usado no menu de simulacao).
    /// </summary>
    public int DetectarRobosOffline()
    {
        if (_executandoAgora)
            return 0;

        _executandoAgora = true;
        try
        {
            var limite = _relogio.Agora.AddSeconds(-_timeoutSegundos);
            var offline = _repoRobo.BuscarOffline(limite);

            if (offline.Count == 0)
                return 0;

            AoLogar?.Invoke($"Detectados {offline.Count} robo(s) offline. Marcando como FALHA.");

            foreach (var robo in offline)
            {
                try
                {
                    MarcarFalhaERealocarTarefas(robo);
                }
                catch (Exception ex)
                {
                    AoLogar?.Invoke($"[ERRO] Falha ao tratar robo {robo.Codigo}: {ex.Message}");
                }
            }
            return offline.Count;
        }
        finally
        {
            _executandoAgora = false;
        }
    }

    private void MarcarFalhaERealocarTarefas(Robo robo)
    {
        robo.Status = StatusRobo.FALHA;
        _repoRobo.Salvar(robo);

        var alerta = new Alerta
        {
            Tipo = TipoAlerta.ROBO_OFFLINE,
            Severidade = SeveridadeAlerta.CRITICO,
            Mensagem = $"Robo {robo.Codigo} nao envia heartbeat ha mais de {_timeoutSegundos}s. Marcado como FALHA.",
            RoboId = robo.Id
        };
        _repoAlerta.Salvar(alerta);

        var tarefas = _repoTarefa.BuscarEmExecucaoPorRobo(robo.Id);
        AoDetectarFalha?.Invoke(robo, tarefas.Count);

        foreach (var tarefa in tarefas)
        {
            var realocada = _orquestrador.RealocarTarefa(tarefa);
            AoLogar?.Invoke(
                $"Tarefa {tarefa.Codigo} {(realocada ? "realocada" : "sem robo disponivel")} apos falha de {robo.Codigo}");
        }
    }

    public void Dispose() => Parar();
}
