using SwarmBuild.Aplicacao.Dtos;
using SwarmBuild.Dominio.Entidades;

namespace SwarmBuild.Aplicacao.Interfaces;

/// <summary>
/// Nucleo da inteligencia de enxame.
/// Responsavel por construir robos a partir de DTOs, escolher o melhor candidato
/// para uma tarefa e realocar tarefas quando o robo original falha.
/// </summary>
public interface IOrquestradorEnxame
{
    Robo ConstruirRoboAPartirDoDto(CriarRoboDto dto);

    Robo? EscolherMelhorRobo(Tarefa tarefa);

    bool RealocarTarefa(Tarefa tarefa);
}
