using SwarmBuild.Dominio.Enums;

namespace SwarmBuild.Dominio.Entidades;

public sealed class RoboTransportador : Robo
{
    public double CapacidadeTransporteKg { get; set; }
    public double VelocidadeMaximaKmh { get; set; }

    public override TipoRobo Tipo => TipoRobo.TRANSPORTADOR;

    public override string DescricaoCapacidade() =>
        $"Transportador: {CapacidadeTransporteKg:F1} kg / {VelocidadeMaximaKmh:F1} km/h";
}
