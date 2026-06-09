using SwarmBuild.Dominio.Enums;
using SwarmBuild.Dominio.Structs;

namespace SwarmBuild.Dominio.Entidades;

/// <summary>
/// Trabalho a ser executado por um robo de um tipo especifico.
/// Possui ciclo de vida (PENDENTE -> EM_EXECUCAO -> CONCLUIDA/REALOCADA).
/// </summary>
public sealed class Tarefa
{
    public long Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public TipoRobo TipoRoboRequerido { get; set; }
    public PrioridadeTarefa Prioridade { get; set; } = PrioridadeTarefa.MEDIA;
    public StatusTarefa Status { get; set; } = StatusTarefa.PENDENTE;
    public long? RoboAtribuidoId { get; set; }
    public Coordenada? LocalExecucao { get; set; }
    public DateTime CriadaEm { get; set; }
    public DateTime? IniciadaEm { get; set; }
    public DateTime? ConcluidaEm { get; set; }
    public int VezesRealocada { get; set; }

    public Tarefa() { }

    public void AoCriar()
    {
        if (CriadaEm == default)
            CriadaEm = DateTime.Now;
    }

    public void AtribuirA(Robo robo)
    {
        ArgumentNullException.ThrowIfNull(robo);
        RoboAtribuidoId = robo.Id;
        Status = StatusTarefa.EM_EXECUCAO;
        IniciadaEm = DateTime.Now;
    }

    public void Desatribuir()
    {
        RoboAtribuidoId = null;
        Status = StatusTarefa.PENDENTE;
    }

    public void MarcarRealocada()
    {
        Status = StatusTarefa.REALOCADA;
        VezesRealocada++;
    }

    public void Concluir()
    {
        Status = StatusTarefa.CONCLUIDA;
        ConcluidaEm = DateTime.Now;
    }

    public bool EstaEmExecucao() =>
        Status == StatusTarefa.EM_EXECUCAO || Status == StatusTarefa.REALOCADA;
}
