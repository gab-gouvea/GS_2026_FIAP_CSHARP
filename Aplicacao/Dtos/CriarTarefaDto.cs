using SwarmBuild.Dominio.Enums;

namespace SwarmBuild.Aplicacao.Dtos;

public sealed record CriarTarefaDto(
    string Codigo,
    string Descricao,
    TipoRobo TipoRoboRequerido,
    PrioridadeTarefa? Prioridade = null,
    double? Latitude = null,
    double? Longitude = null
);
