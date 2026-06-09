using SwarmBuild.Aplicacao.Dtos;
using SwarmBuild.Aplicacao.Interfaces;
using SwarmBuild.Dominio.Entidades;
using SwarmBuild.Dominio.Enums;
using SwarmBuild.Dominio.Excecoes;

namespace SwarmBuild.Aplicacao.Servicos;

public sealed class ServicoRobo
{
    private readonly IRepositorioRobo _repoRobo;
    private readonly IRepositorioTarefa _repoTarefa;
    private readonly IRepositorioAlerta _repoAlerta;
    private readonly IRepositorioHeartbeat _repoHeartbeat;
    private readonly IOrquestradorEnxame _orquestrador;

    public ServicoRobo(
        IRepositorioRobo repoRobo,
        IRepositorioTarefa repoTarefa,
        IRepositorioAlerta repoAlerta,
        IRepositorioHeartbeat repoHeartbeat,
        IOrquestradorEnxame orquestrador)
    {
        _repoRobo = repoRobo;
        _repoTarefa = repoTarefa;
        _repoAlerta = repoAlerta;
        _repoHeartbeat = repoHeartbeat;
        _orquestrador = orquestrador;
    }

    public Robo Criar(CriarRoboDto dto)
    {
        if (_repoRobo.ExistePorCodigo(dto.Codigo))
            throw new CodigoDuplicadoException(dto.Codigo);

        var robo = _orquestrador.ConstruirRoboAPartirDoDto(dto);
        return _repoRobo.Salvar(robo);
    }

    public IReadOnlyList<Robo> Listar() => _repoRobo.ListarTodos();

    public Robo Buscar(long id) =>
        _repoRobo.BuscarPorId(id) ?? throw new RoboNaoEncontradoException(id);

    public Robo BuscarPorCodigo(string codigo) =>
        _repoRobo.BuscarPorCodigo(codigo)
            ?? throw new RoboNaoEncontradoException($"Robo nao encontrado: codigo={codigo}");

    public Robo AtualizarStatus(long id, StatusRobo novoStatus)
    {
        var robo = Buscar(id);
        robo.Status = novoStatus;
        return _repoRobo.Salvar(robo);
    }

    public void Deletar(long id)
    {
        var robo = Buscar(id);
        if (robo.Status == StatusRobo.EM_TAREFA)
            throw new RegraDeNegocioException("Nao e possivel deletar um robo em tarefa");
        if (_repoTarefa.BuscarEmExecucaoPorRobo(id).Count > 0)
            throw new RegraDeNegocioException("Nao e possivel deletar um robo com tarefas ativas atribuidas");

        // Preserva o historico: tarefas concluidas e alertas mantem o registro,
        // apenas zerando a FK para o robo deletado. Heartbeats sao descartados.
        _repoTarefa.DesvincularRobo(id);
        _repoAlerta.DesvincularRobo(id);
        _repoHeartbeat.DeletarPorRobo(id);
        _repoRobo.Remover(id);
    }
}
