namespace SwarmBuild.Dominio.Excecoes;

public sealed class RoboNaoEncontradoException : SwarmBuildException
{
    public RoboNaoEncontradoException(long id) : base($"Robo nao encontrado: id={id}") { }
    public RoboNaoEncontradoException(string mensagem) : base(mensagem) { }
}
