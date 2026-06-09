using SwarmBuild.Dominio.Entidades;

namespace SwarmBuild.Aplicacao.Interfaces;

public interface IRepositorioRobo : IRepositorio<Robo>
{
    bool ExistePorCodigo(string codigo);
    Robo? BuscarPorCodigo(string codigo);
    IReadOnlyList<Robo> BuscarDisponiveis();
    IReadOnlyList<Robo> BuscarOffline(DateTime limite);
}
