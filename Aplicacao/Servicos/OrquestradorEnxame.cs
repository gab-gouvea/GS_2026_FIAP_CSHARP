using SwarmBuild.Aplicacao.Dtos;
using SwarmBuild.Aplicacao.Interfaces;
using SwarmBuild.Dominio.Entidades;
using SwarmBuild.Dominio.Enums;
using SwarmBuild.Dominio.Structs;

namespace SwarmBuild.Aplicacao.Servicos;

/// <summary>
/// Implementacao principal do orquestrador de enxame.
/// Classe partial - a outra metade (alertas) esta em OrquestradorEnxame.Alertas.cs.
/// Demonstra organizacao por responsabilidade dentro de uma mesma classe.
/// </summary>
public sealed partial class OrquestradorEnxame : IOrquestradorEnxame
{
    private readonly IRepositorioRobo _repoRobo;
    private readonly IRepositorioTarefa _repoTarefa;
    private readonly IRepositorioAlerta _repoAlerta;

    public OrquestradorEnxame(
        IRepositorioRobo repoRobo,
        IRepositorioTarefa repoTarefa,
        IRepositorioAlerta repoAlerta)
    {
        _repoRobo = repoRobo;
        _repoTarefa = repoTarefa;
        _repoAlerta = repoAlerta;
    }

    public Robo ConstruirRoboAPartirDoDto(CriarRoboDto dto)
    {
        Robo robo = dto.Tipo switch
        {
            TipoRobo.ESCAVADEIRA => new RoboEscavadeira
            {
                CapacidadeCargaKg = dto.CapacidadeCargaKg ?? 0,
                ProfundidadeMaximaMetros = dto.ProfundidadeMaximaMetros ?? 0
            },
            TipoRobo.TRANSPORTADOR => new RoboTransportador
            {
                CapacidadeTransporteKg = dto.CapacidadeTransporteKg ?? 0,
                VelocidadeMaximaKmh = dto.VelocidadeMaximaKmh ?? 0
            },
            TipoRobo.MONTADOR => new RoboMontador
            {
                PrecisaoMontagemMm = dto.PrecisaoMontagemMm ?? 0,
                BracosManipuladores = dto.BracosManipuladores ?? 0
            },
            _ => throw new ArgumentOutOfRangeException(nameof(dto), $"Tipo desconhecido: {dto.Tipo}")
        };

        robo.Codigo = dto.Codigo;
        robo.Nome = dto.Nome;
        robo.Modelo = dto.Modelo;
        if (dto.Latitude.HasValue && dto.Longitude.HasValue)
            robo.Coordenada = new Coordenada(dto.Latitude.Value, dto.Longitude.Value);

        return robo;
    }

    public Robo? EscolherMelhorRobo(Tarefa tarefa)
    {
        var candidatos = _repoRobo.BuscarDisponiveis()
            .Where(r => r.EhCompativelCom(tarefa.TipoRoboRequerido))
            .ToList();

        if (candidatos.Count == 0)
            return null;

        // Sem destino definido, escolhe o de maior bateria (melhor reserva energetica).
        if (!tarefa.LocalExecucao.HasValue)
            return candidatos.OrderByDescending(r => r.Bateria).First();

        var destino = tarefa.LocalExecucao.Value;
        return candidatos
            .OrderBy(r => r.Coordenada.HasValue
                ? r.Coordenada.Value.DistanciaEuclidiana(destino)
                : double.MaxValue)
            .First();
    }

    public bool RealocarTarefa(Tarefa tarefa)
    {
        var anteriorId = tarefa.RoboAtribuidoId;
        var anterior = anteriorId.HasValue ? _repoRobo.BuscarPorId(anteriorId.Value) : null;

        tarefa.Desatribuir();
        tarefa.MarcarRealocada();

        // Um robo saudavel que estava apenas ocupado com esta tarefa volta para DISPONIVEL.
        // Robos em FALHA ou MANUTENCAO mantem o status (fluxo do monitor de falhas).
        if (anterior is not null && anterior.Status == StatusRobo.EM_TAREFA)
        {
            anterior.Status = StatusRobo.DISPONIVEL;
            _repoRobo.Salvar(anterior);
        }

        var novo = EscolherMelhorRobo(tarefa);
        if (novo is not null)
        {
            novo.Status = StatusRobo.EM_TAREFA;
            _repoRobo.Salvar(novo);
            tarefa.AtribuirA(novo);
            _repoTarefa.Salvar(tarefa);
            RegistrarAlertaRealocacao(tarefa, anterior, novo);
            return true;
        }

        _repoTarefa.Salvar(tarefa);
        RegistrarAlertaSemRoboDisponivel(tarefa, anterior);
        return false;
    }
}
