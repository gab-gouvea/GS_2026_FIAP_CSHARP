using SwarmBuild.Dominio.Enums;

namespace SwarmBuild.Aplicacao.Dtos;

public sealed record HeartbeatDto(
    int Bateria,
    StatusRobo StatusReportado,
    double? Latitude = null,
    double? Longitude = null,
    string? Mensagem = null
);
