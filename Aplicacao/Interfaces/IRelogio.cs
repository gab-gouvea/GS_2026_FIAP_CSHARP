namespace SwarmBuild.Aplicacao.Interfaces;

/// <summary>
/// Abstracao do relogio do sistema. Permite injetar uma implementacao fake
/// em testes (ex.: avancar o tempo manualmente para validar timeouts).
/// </summary>
public interface IRelogio
{
    DateTime Agora { get; }
}
