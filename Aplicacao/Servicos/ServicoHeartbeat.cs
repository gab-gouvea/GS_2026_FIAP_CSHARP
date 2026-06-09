using SwarmBuild.Aplicacao.Dtos;
using SwarmBuild.Aplicacao.Interfaces;
using SwarmBuild.Dominio.Entidades;
using SwarmBuild.Dominio.Enums;
using SwarmBuild.Dominio.Structs;

namespace SwarmBuild.Aplicacao.Servicos;

public sealed class ServicoHeartbeat
{
    private const int BateriaAlertaPercentual = 20;
    private const int BateriaCriticaPercentual = 10;

    private readonly IRepositorioHeartbeat _repoHeartbeat;
    private readonly IRepositorioRobo _repoRobo;
    private readonly IRepositorioAlerta _repoAlerta;
    private readonly ServicoRobo _servicoRobo;
    private readonly IRelogio _relogio;

    public ServicoHeartbeat(
        IRepositorioHeartbeat repoHeartbeat,
        IRepositorioRobo repoRobo,
        IRepositorioAlerta repoAlerta,
        ServicoRobo servicoRobo,
        IRelogio relogio)
    {
        _repoHeartbeat = repoHeartbeat;
        _repoRobo = repoRobo;
        _repoAlerta = repoAlerta;
        _servicoRobo = servicoRobo;
        _relogio = relogio;
    }

    public Heartbeat Registrar(long roboId, HeartbeatDto dto)
    {
        var robo = _servicoRobo.Buscar(roboId);

        robo.Bateria = dto.Bateria;
        robo.UltimoHeartbeat = _relogio.Agora;
        if (dto.Latitude.HasValue && dto.Longitude.HasValue)
            robo.Coordenada = new Coordenada(dto.Latitude.Value, dto.Longitude.Value);

        // Recuperacao automatica: se o robo estava em FALHA e voltou a transmitir
        // reportando outro status, retorna para DISPONIVEL.
        if (robo.Status == StatusRobo.FALHA && dto.StatusReportado != StatusRobo.FALHA)
            robo.Status = StatusRobo.DISPONIVEL;

        _repoRobo.Salvar(robo);

        var heartbeat = new Heartbeat
        {
            RoboId = robo.Id,
            Bateria = dto.Bateria,
            StatusReportado = dto.StatusReportado,
            Mensagem = dto.Mensagem,
            Timestamp = _relogio.Agora
        };
        if (dto.Latitude.HasValue && dto.Longitude.HasValue)
            heartbeat.Coordenada = new Coordenada(dto.Latitude.Value, dto.Longitude.Value);

        var salvo = _repoHeartbeat.Salvar(heartbeat);

        if (dto.Bateria < BateriaAlertaPercentual)
        {
            var alerta = new Alerta
            {
                Tipo = TipoAlerta.BATERIA_BAIXA,
                Severidade = dto.Bateria < BateriaCriticaPercentual
                    ? SeveridadeAlerta.CRITICO
                    : SeveridadeAlerta.AVISO,
                Mensagem = $"Bateria do robo {robo.Codigo} em {dto.Bateria}%",
                RoboId = robo.Id
            };
            _repoAlerta.Salvar(alerta);
        }

        return salvo;
    }

    public IReadOnlyList<Heartbeat> HistoricoDoRobo(long roboId)
    {
        _servicoRobo.Buscar(roboId);
        return _repoHeartbeat.BuscarPorRobo(roboId);
    }
}
