using SwarmBuild.Dominio.Entidades;

namespace SwarmBuild.Aplicacao.Interfaces;

public interface IRepositorioAlerta : IRepositorio<Alerta>
{
    IReadOnlyList<Alerta> BuscarPorResolvido(bool resolvido);
    void DesvincularRobo(long roboId);
}
