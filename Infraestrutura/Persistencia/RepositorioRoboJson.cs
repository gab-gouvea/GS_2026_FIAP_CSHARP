using SwarmBuild.Aplicacao.Interfaces;
using SwarmBuild.Dominio.Entidades;
using SwarmBuild.Dominio.Enums;

namespace SwarmBuild.Infraestrutura.Persistencia;

public sealed class RepositorioRoboJson : RepositorioBaseJson<Robo>, IRepositorioRobo
{
    public RepositorioRoboJson(string caminhoBase)
        : base(Path.Combine(caminhoBase, "robos.json")) { }

    protected override long ObterId(Robo entidade) => entidade.Id;
    protected override void DefinirId(Robo entidade, long id) => entidade.Id = id;
    protected override void AntesDeSalvarNova(Robo entidade) => entidade.AoCriar();

    public bool ExistePorCodigo(string codigo) =>
        Snapshot().Any(r => string.Equals(r.Codigo, codigo, StringComparison.OrdinalIgnoreCase));

    public Robo? BuscarPorCodigo(string codigo) =>
        Snapshot().FirstOrDefault(r => string.Equals(r.Codigo, codigo, StringComparison.OrdinalIgnoreCase));

    public IReadOnlyList<Robo> BuscarDisponiveis() =>
        Snapshot().Where(r => r.EstaDisponivel()).ToList();

    public IReadOnlyList<Robo> BuscarOffline(DateTime limite) =>
        Snapshot()
            .Where(r => r.Status == StatusRobo.EM_TAREFA || r.Status == StatusRobo.DISPONIVEL)
            .Where(r => r.UltimoHeartbeat.HasValue && r.UltimoHeartbeat.Value < limite)
            .ToList();
}
