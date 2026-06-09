using SwarmBuild.Dominio.Enums;

namespace SwarmBuild.Dominio.Entidades;

/// <summary>
/// Evento critico registrado pelo enxame (offline, bateria baixa, realocacao, etc.).
/// Forma o historico de auditoria da missao.
/// </summary>
public sealed class Alerta
{
    public long Id { get; set; }
    public TipoAlerta Tipo { get; set; }
    public SeveridadeAlerta Severidade { get; set; }
    public string Mensagem { get; set; } = string.Empty;
    public long? RoboId { get; set; }
    public long? TarefaId { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime? ResolvidoEm { get; set; }
    public bool Resolvido { get; set; }

    public Alerta() { }

    public void AoCriar()
    {
        if (CriadoEm == default)
            CriadoEm = DateTime.Now;
    }

    public void Resolver()
    {
        Resolvido = true;
        ResolvidoEm = DateTime.Now;
    }
}
