using SwarmBuild.Aplicacao.Dtos;
using SwarmBuild.Aplicacao.Interfaces;
using SwarmBuild.Dominio.Entidades;
using SwarmBuild.Dominio.Enums;
using SwarmBuild.Dominio.Excecoes;
using SwarmBuild.Dominio.Structs;

namespace SwarmBuild.Aplicacao.Servicos;

public sealed class ServicoTarefa
{
    private readonly IRepositorioTarefa _repoTarefa;
    private readonly IRepositorioRobo _repoRobo;
    private readonly IOrquestradorEnxame _orquestrador;

    public ServicoTarefa(
        IRepositorioTarefa repoTarefa,
        IRepositorioRobo repoRobo,
        IOrquestradorEnxame orquestrador)
    {
        _repoTarefa = repoTarefa;
        _repoRobo = repoRobo;
        _orquestrador = orquestrador;
    }

    public Tarefa Criar(CriarTarefaDto dto)
    {
        if (_repoTarefa.ExistePorCodigo(dto.Codigo))
            throw new CodigoDuplicadoException(dto.Codigo);

        var tarefa = new Tarefa
        {
            Codigo = dto.Codigo,
            Descricao = dto.Descricao,
            TipoRoboRequerido = dto.TipoRoboRequerido,
            Prioridade = dto.Prioridade ?? PrioridadeTarefa.MEDIA
        };
        if (dto.Latitude.HasValue && dto.Longitude.HasValue)
            tarefa.LocalExecucao = new Coordenada(dto.Latitude.Value, dto.Longitude.Value);

        return _repoTarefa.Salvar(tarefa);
    }

    public IReadOnlyList<Tarefa> Listar() => _repoTarefa.ListarTodos();

    public Tarefa Buscar(long id) =>
        _repoTarefa.BuscarPorId(id) ?? throw new TarefaNaoEncontradaException(id);

    public Tarefa AtribuirMelhorRobo(long tarefaId)
    {
        var tarefa = Buscar(tarefaId);
        if (tarefa.Status == StatusTarefa.CONCLUIDA || tarefa.Status == StatusTarefa.CANCELADA)
            throw new RegraDeNegocioException("Tarefa ja finalizada");

        var robo = _orquestrador.EscolherMelhorRobo(tarefa)
            ?? throw new RegraDeNegocioException($"Nenhum robo {tarefa.TipoRoboRequerido} disponivel");

        robo.Status = StatusRobo.EM_TAREFA;
        _repoRobo.Salvar(robo);
        tarefa.AtribuirA(robo);
        return _repoTarefa.Salvar(tarefa);
    }

    public Tarefa AtribuirRoboEspecifico(long tarefaId, long roboId)
    {
        var tarefa = Buscar(tarefaId);
        var robo = _repoRobo.BuscarPorId(roboId)
            ?? throw new RegraDeNegocioException($"Robo nao encontrado: {roboId}");

        if (!robo.EhCompativelCom(tarefa.TipoRoboRequerido))
            throw new RegraDeNegocioException(
                $"Robo {robo.Codigo} nao e compativel com tarefa que exige {tarefa.TipoRoboRequerido}");
        if (!robo.EstaDisponivel())
            throw new RegraDeNegocioException($"Robo {robo.Codigo} nao esta disponivel");

        robo.Status = StatusRobo.EM_TAREFA;
        _repoRobo.Salvar(robo);
        tarefa.AtribuirA(robo);
        return _repoTarefa.Salvar(tarefa);
    }

    public Tarefa Concluir(long tarefaId)
    {
        var tarefa = Buscar(tarefaId);
        if (!tarefa.EstaEmExecucao())
            throw new RegraDeNegocioException("Tarefa nao esta em execucao");

        if (tarefa.RoboAtribuidoId.HasValue)
        {
            var robo = _repoRobo.BuscarPorId(tarefa.RoboAtribuidoId.Value);
            if (robo is not null)
            {
                robo.Status = StatusRobo.DISPONIVEL;
                _repoRobo.Salvar(robo);
            }
        }
        tarefa.Concluir();
        return _repoTarefa.Salvar(tarefa);
    }

    public Tarefa Realocar(long tarefaId)
    {
        var tarefa = Buscar(tarefaId);
        if (tarefa.Status != StatusTarefa.EM_EXECUCAO && tarefa.Status != StatusTarefa.REALOCADA)
            throw new RegraDeNegocioException("Tarefa precisa estar em execucao para ser realocada");

        _orquestrador.RealocarTarefa(tarefa);
        return Buscar(tarefaId);
    }

    public void Deletar(long id)
    {
        var tarefa = Buscar(id);
        if (tarefa.EstaEmExecucao())
            throw new RegraDeNegocioException("Nao e possivel deletar tarefa em execucao");
        _repoTarefa.Remover(id);
    }
}
