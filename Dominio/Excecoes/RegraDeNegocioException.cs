namespace SwarmBuild.Dominio.Excecoes;

public sealed class RegraDeNegocioException : SwarmBuildException
{
    public RegraDeNegocioException(string mensagem) : base(mensagem) { }
}
