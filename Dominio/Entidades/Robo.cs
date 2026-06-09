using System.Text.Json.Serialization;
using SwarmBuild.Dominio.Enums;
using SwarmBuild.Dominio.Structs;

namespace SwarmBuild.Dominio.Entidades;

/// <summary>
/// Classe abstrata que representa um robo do enxame.
/// Modelo de heranca: subclasses concretas (Escavadeira, Transportador, Montador)
/// implementam o comportamento polimorfico via metodos abstratos Tipo e DescricaoCapacidade.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$tipoRobo")]
[JsonDerivedType(typeof(RoboEscavadeira), nameof(RoboEscavadeira))]
[JsonDerivedType(typeof(RoboTransportador), nameof(RoboTransportador))]
[JsonDerivedType(typeof(RoboMontador), nameof(RoboMontador))]
public abstract class Robo
{
    private const int BateriaInicial = 100;
    private const int BateriaMinimaParaOperar = 10;

    public long Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public StatusRobo Status { get; set; } = StatusRobo.DISPONIVEL;
    public int Bateria { get; set; } = BateriaInicial;
    public Coordenada? Coordenada { get; set; }
    public DateTime? UltimoHeartbeat { get; set; }
    public DateTime CriadoEm { get; set; }

    protected Robo() { }

    /// <summary>
    /// Hook chamado pelo repositorio antes de persistir um novo robo.
    /// Equivale ao @PrePersist do JPA original.
    /// </summary>
    protected internal virtual void AoCriar()
    {
        if (CriadoEm == default)
            CriadoEm = DateTime.Now;
    }

    public abstract TipoRobo Tipo { get; }

    public abstract string DescricaoCapacidade();

    public bool EstaDisponivel() =>
        Status == StatusRobo.DISPONIVEL && Bateria > BateriaMinimaParaOperar;

    public bool EhCompativelCom(TipoRobo tipoRequerido) => Tipo == tipoRequerido;

    public override string ToString() =>
        $"[{Codigo}] {Nome} ({Modelo}) - {Tipo} | {Status} | Bat {Bateria}%";
}
