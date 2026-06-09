namespace SwarmBuild.Dominio.Excecoes;

public sealed class CodigoDuplicadoException : SwarmBuildException
{
    public string Codigo { get; }

    public CodigoDuplicadoException(string codigo)
        : base($"Codigo ja em uso: {codigo}")
    {
        Codigo = codigo;
    }
}
