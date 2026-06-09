namespace SwarmBuild.Dominio.Excecoes;

public sealed class TarefaNaoEncontradaException : SwarmBuildException
{
    public TarefaNaoEncontradaException(long id) : base($"Tarefa nao encontrada: id={id}") { }
}
