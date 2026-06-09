namespace SwarmBuild.Dominio.Excecoes;

public sealed class AlertaNaoEncontradoException : SwarmBuildException
{
    public AlertaNaoEncontradoException(long id) : base($"Alerta nao encontrado: id={id}") { }
}
