using SwarmBuild.Aplicacao.Interfaces;
using SwarmBuild.Dominio.Entidades;

namespace SwarmBuild.Infraestrutura.Persistencia;

public sealed class RepositorioHeartbeatJson : RepositorioBaseJson<Heartbeat>, IRepositorioHeartbeat
{
    public RepositorioHeartbeatJson(string caminhoBase)
        : base(Path.Combine(caminhoBase, "heartbeats.json")) { }

    protected override long ObterId(Heartbeat entidade) => entidade.Id;
    protected override void DefinirId(Heartbeat entidade, long id) => entidade.Id = id;
    protected override void AntesDeSalvarNova(Heartbeat entidade) => entidade.AoCriar();

    public IReadOnlyList<Heartbeat> BuscarPorRobo(long roboId) =>
        Snapshot()
            .Where(h => h.RoboId == roboId)
            .OrderByDescending(h => h.Timestamp)
            .ToList();

    public int DeletarPorRobo(long roboId)
    {
        var alvos = Snapshot().Where(h => h.RoboId == roboId).Select(h => h.Id).ToList();
        var count = 0;
        foreach (var id in alvos)
        {
            if (Remover(id))
                count++;
        }
        return count;
    }
}
