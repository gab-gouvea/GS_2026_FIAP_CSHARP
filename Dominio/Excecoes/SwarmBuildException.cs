namespace SwarmBuild.Dominio.Excecoes;

/// <summary>
/// Classe base para todas as excecoes do dominio.
/// Permite que o handler central capture um tipo unico para erros esperados.
/// </summary>
public abstract class SwarmBuildException : Exception
{
    protected SwarmBuildException(string mensagem) : base(mensagem) { }
    protected SwarmBuildException(string mensagem, Exception causa) : base(mensagem, causa) { }
}
