using SwarmBuild.Dominio.Enums;

namespace SwarmBuild.Dominio.Entidades;

public sealed class RoboEscavadeira : Robo
{
    public double CapacidadeCargaKg { get; set; }
    public double ProfundidadeMaximaMetros { get; set; }

    public override TipoRobo Tipo => TipoRobo.ESCAVADEIRA;

    public override string DescricaoCapacidade() =>
        $"Escavadeira: {CapacidadeCargaKg:F1} kg / {ProfundidadeMaximaMetros:F1} m profundidade";
}
