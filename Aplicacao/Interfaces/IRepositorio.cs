namespace SwarmBuild.Aplicacao.Interfaces;

/// <summary>
/// Contrato generico para repositorios de leitura/escrita em memoria com persistencia em JSON.
/// </summary>
public interface IRepositorio<TEntidade> where TEntidade : class
{
    TEntidade Salvar(TEntidade entidade);
    TEntidade? BuscarPorId(long id);
    IReadOnlyList<TEntidade> ListarTodos();
    bool Remover(long id);
    void Limpar();
}
