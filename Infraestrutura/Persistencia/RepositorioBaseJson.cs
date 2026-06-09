using System.Text.Json;
using SwarmBuild.Aplicacao.Interfaces;

namespace SwarmBuild.Infraestrutura.Persistencia;

/// <summary>
/// Repositorio generico abstrato com persistencia em arquivo JSON.
/// As subclasses informam o caminho do arquivo e como ler/escrever o Id da entidade.
/// Mantem cache em memoria para evitar IO em cada operacao de leitura.
/// </summary>
public abstract class RepositorioBaseJson<TEntidade> : IRepositorio<TEntidade>
    where TEntidade : class
{
    private readonly string _caminhoArquivo;
    private readonly List<TEntidade> _cache;
    private long _proximoId = 1;
    private readonly object _lock = new();

    protected RepositorioBaseJson(string caminhoArquivo)
    {
        _caminhoArquivo = caminhoArquivo;
        _cache = CarregarDoDisco();
        _proximoId = _cache.Count == 0 ? 1 : _cache.Max(ObterId) + 1;
    }

    protected abstract long ObterId(TEntidade entidade);
    protected abstract void DefinirId(TEntidade entidade, long id);

    /// <summary>
    /// Hook opcional executado antes de persistir uma nova entidade (id == 0).
    /// Subclasses sobrescrevem para chamar AoCriar() das entidades.
    /// </summary>
    protected virtual void AntesDeSalvarNova(TEntidade entidade) { }

    public TEntidade Salvar(TEntidade entidade)
    {
        lock (_lock)
        {
            var id = ObterId(entidade);
            if (id == 0)
            {
                DefinirId(entidade, _proximoId++);
                AntesDeSalvarNova(entidade);
                _cache.Add(entidade);
            }
            else
            {
                var existente = _cache.FirstOrDefault(e => ObterId(e) == id);
                if (existente == null)
                {
                    _cache.Add(entidade);
                }
                else if (!ReferenceEquals(existente, entidade))
                {
                    _cache.Remove(existente);
                    _cache.Add(entidade);
                }
            }
            PersistirNoDisco();
            return entidade;
        }
    }

    public TEntidade? BuscarPorId(long id)
    {
        lock (_lock)
        {
            return _cache.FirstOrDefault(e => ObterId(e) == id);
        }
    }

    public IReadOnlyList<TEntidade> ListarTodos()
    {
        lock (_lock)
        {
            return _cache.ToList();
        }
    }

    public bool Remover(long id)
    {
        lock (_lock)
        {
            var existente = _cache.FirstOrDefault(e => ObterId(e) == id);
            if (existente == null)
                return false;
            _cache.Remove(existente);
            PersistirNoDisco();
            return true;
        }
    }

    public void Limpar()
    {
        lock (_lock)
        {
            _cache.Clear();
            _proximoId = 1;
            PersistirNoDisco();
        }
    }

    protected IEnumerable<TEntidade> Snapshot()
    {
        lock (_lock)
        {
            return _cache.ToList();
        }
    }

    protected void PersistirSeAlterado() => PersistirNoDisco();

    private List<TEntidade> CarregarDoDisco()
    {
        try
        {
            if (!File.Exists(_caminhoArquivo))
                return new List<TEntidade>();

            var json = File.ReadAllText(_caminhoArquivo);
            if (string.IsNullOrWhiteSpace(json))
                return new List<TEntidade>();

            var lista = JsonSerializer.Deserialize<List<TEntidade>>(json, ConfiguracaoJson.Opcoes);
            return lista ?? new List<TEntidade>();
        }
        catch (JsonException ex)
        {
            Console.Error.WriteLine($"[AVISO] Arquivo {_caminhoArquivo} corrompido - iniciando vazio. Detalhe: {ex.Message}");
            return new List<TEntidade>();
        }
        catch (IOException ex)
        {
            Console.Error.WriteLine($"[AVISO] Falha de IO ao ler {_caminhoArquivo}: {ex.Message}");
            return new List<TEntidade>();
        }
    }

    private void PersistirNoDisco()
    {
        try
        {
            var diretorio = Path.GetDirectoryName(_caminhoArquivo);
            if (!string.IsNullOrEmpty(diretorio) && !Directory.Exists(diretorio))
                Directory.CreateDirectory(diretorio);

            var json = JsonSerializer.Serialize(_cache, ConfiguracaoJson.Opcoes);
            File.WriteAllText(_caminhoArquivo, json);
        }
        catch (IOException ex)
        {
            Console.Error.WriteLine($"[ERRO] Falha ao gravar {_caminhoArquivo}: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.Error.WriteLine($"[ERRO] Sem permissao para gravar {_caminhoArquivo}: {ex.Message}");
        }
    }
}
