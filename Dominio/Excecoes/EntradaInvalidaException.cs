namespace SwarmBuild.Dominio.Excecoes;

/// <summary>
/// Erro de validacao da camada de apresentacao (input do usuario invalido).
/// Capturado nos menus para mostrar mensagem sem encerrar o programa.
/// </summary>
public sealed class EntradaInvalidaException : SwarmBuildException
{
    public EntradaInvalidaException(string mensagem) : base(mensagem) { }
}
