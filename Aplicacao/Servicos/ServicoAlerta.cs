using SwarmBuild.Aplicacao.Interfaces;
using SwarmBuild.Dominio.Entidades;
using SwarmBuild.Dominio.Excecoes;

namespace SwarmBuild.Aplicacao.Servicos;

public sealed class ServicoAlerta
{
    private readonly IRepositorioAlerta _repoAlerta;

    public ServicoAlerta(IRepositorioAlerta repoAlerta)
    {
        _repoAlerta = repoAlerta;
    }

    public IReadOnlyList<Alerta> Listar(bool? resolvido = null) =>
        resolvido is null
            ? _repoAlerta.ListarTodos()
            : _repoAlerta.BuscarPorResolvido(resolvido.Value);

    public Alerta Buscar(long id) =>
        _repoAlerta.BuscarPorId(id) ?? throw new AlertaNaoEncontradoException(id);

    public Alerta Resolver(long id)
    {
        var alerta = Buscar(id);
        alerta.Resolver();
        return _repoAlerta.Salvar(alerta);
    }
}
