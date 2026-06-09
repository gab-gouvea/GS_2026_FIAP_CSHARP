using SwarmBuild.Dominio.Entidades;

namespace SwarmBuild.Aplicacao.Interfaces;

public interface IRepositorioHeartbeat : IRepositorio<Heartbeat>
{
    IReadOnlyList<Heartbeat> BuscarPorRobo(long roboId);
    int DeletarPorRobo(long roboId);
}
