using SwarmBuild.Dominio.Entidades;

namespace SwarmBuild.Aplicacao.Interfaces;

public interface IRepositorioTarefa : IRepositorio<Tarefa>
{
    bool ExistePorCodigo(string codigo);
    Tarefa? BuscarPorCodigo(string codigo);
    IReadOnlyList<Tarefa> BuscarEmExecucaoPorRobo(long roboId);
    void DesvincularRobo(long roboId);
}
