using SwarmBuild.Dominio.Enums;

namespace SwarmBuild.Aplicacao.Dtos;

public sealed record CriarRoboDto(
    string Codigo,
    string Nome,
    string Modelo,
    TipoRobo Tipo,
    double? Latitude = null,
    double? Longitude = null,
    double? CapacidadeCargaKg = null,
    double? ProfundidadeMaximaMetros = null,
    double? CapacidadeTransporteKg = null,
    double? VelocidadeMaximaKmh = null,
    double? PrecisaoMontagemMm = null,
    int? BracosManipuladores = null
);
