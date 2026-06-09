using SwarmBuild.Aplicacao.Interfaces;
using SwarmBuild.Dominio.Entidades;
using SwarmBuild.Dominio.Enums;

namespace SwarmBuild.Infraestrutura.Persistencia;

public sealed class RepositorioTarefaJson : RepositorioBaseJson<Tarefa>, IRepositorioTarefa
{
    public RepositorioTarefaJson(string caminhoBase)
        : base(Path.Combine(caminhoBase, "tarefas.json")) { }

    protected override long ObterId(Tarefa entidade) => entidade.Id;
    protected override void DefinirId(Tarefa entidade, long id) => entidade.Id = id;
    protected override void AntesDeSalvarNova(Tarefa entidade) => entidade.AoCriar();

    public bool ExistePorCodigo(string codigo) =>
        Snapshot().Any(t => string.Equals(t.Codigo, codigo, StringComparison.OrdinalIgnoreCase));

    public Tarefa? BuscarPorCodigo(string codigo) =>
        Snapshot().FirstOrDefault(t => string.Equals(t.Codigo, codigo, StringComparison.OrdinalIgnoreCase));

    public IReadOnlyList<Tarefa> BuscarEmExecucaoPorRobo(long roboId) =>
        Snapshot()
            .Where(t => t.RoboAtribuidoId == roboId)
            .Where(t => t.Status == StatusTarefa.EM_EXECUCAO || t.Status == StatusTarefa.REALOCADA)
            .ToList();

    public void DesvincularRobo(long roboId)
    {
        var alterou = false;
        foreach (var t in Snapshot())
        {
            if (t.RoboAtribuidoId == roboId)
            {
                t.RoboAtribuidoId = null;
                alterou = true;
            }
        }
        if (alterou)
            PersistirSeAlterado();
    }
}
