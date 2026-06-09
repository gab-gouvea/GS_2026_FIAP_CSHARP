using SwarmBuild.Aplicacao.Interfaces;
using SwarmBuild.Dominio.Entidades;

namespace SwarmBuild.Infraestrutura.Persistencia;

public sealed class RepositorioAlertaJson : RepositorioBaseJson<Alerta>, IRepositorioAlerta
{
    public RepositorioAlertaJson(string caminhoBase)
        : base(Path.Combine(caminhoBase, "alertas.json")) { }

    protected override long ObterId(Alerta entidade) => entidade.Id;
    protected override void DefinirId(Alerta entidade, long id) => entidade.Id = id;
    protected override void AntesDeSalvarNova(Alerta entidade) => entidade.AoCriar();

    public IReadOnlyList<Alerta> BuscarPorResolvido(bool resolvido) =>
        Snapshot()
            .Where(a => a.Resolvido == resolvido)
            .OrderByDescending(a => a.CriadoEm)
            .ToList();

    public void DesvincularRobo(long roboId)
    {
        var alterou = false;
        foreach (var a in Snapshot())
        {
            if (a.RoboId == roboId)
            {
                a.RoboId = null;
                alterou = true;
            }
        }
        if (alterou)
            PersistirSeAlterado();
    }
}
