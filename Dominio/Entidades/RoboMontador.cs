using SwarmBuild.Dominio.Enums;

namespace SwarmBuild.Dominio.Entidades;

public sealed class RoboMontador : Robo
{
    public double PrecisaoMontagemMm { get; set; }
    public int BracosManipuladores { get; set; }

    public override TipoRobo Tipo => TipoRobo.MONTADOR;

    public override string DescricaoCapacidade() =>
        $"Montador: {PrecisaoMontagemMm:F2} mm precisao / {BracosManipuladores} bracos";
}
