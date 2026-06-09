using Microsoft.Extensions.DependencyInjection;
using SwarmBuild.Aplicacao.Interfaces;
using SwarmBuild.Aplicacao.Servicos;
using SwarmBuild.Apresentacao;
using SwarmBuild.Infraestrutura.Persistencia;
using SwarmBuild.Infraestrutura.Tempo;

namespace SwarmBuild;

/// <summary>
/// Ponto de entrada da aplicacao.
/// Configura o container de Injecao de Dependencia (Microsoft.Extensions.DependencyInjection),
/// resolve o menu principal e inicia o loop interativo.
/// </summary>
public static class Program
{
    public static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        var caminhoBaseDados = ResolverCaminhoDados(args);
        Directory.CreateDirectory(caminhoBaseDados);

        var provider = ConstruirContainer(caminhoBaseDados);

        try
        {
            var menu = provider.GetRequiredService<MenuPrincipal>();
            menu.Executar();
        }
        catch (Exception ex)
        {
            ConsoleUtils.EscreverErro($"Falha fatal: {ex.Message}");
            Console.Error.WriteLine(ex);
            Environment.ExitCode = 1;
        }
        finally
        {
            (provider as IDisposable)?.Dispose();
        }
    }

    private static string ResolverCaminhoDados(string[] args)
    {
        // Permite passar --dados <pasta> para sobrescrever o caminho padrao.
        for (var i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == "--dados")
                return Path.GetFullPath(args[i + 1]);
        }
        return Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "dados");
    }

    private static ServiceProvider ConstruirContainer(string caminhoBaseDados)
    {
        var servicos = new ServiceCollection();

        // Infraestrutura: tempo e persistencia.
        servicos.AddSingleton<IRelogio, RelogioDoSistema>();
        servicos.AddSingleton<IRepositorioRobo>(_ => new RepositorioRoboJson(caminhoBaseDados));
        servicos.AddSingleton<IRepositorioTarefa>(_ => new RepositorioTarefaJson(caminhoBaseDados));
        servicos.AddSingleton<IRepositorioAlerta>(_ => new RepositorioAlertaJson(caminhoBaseDados));
        servicos.AddSingleton<IRepositorioHeartbeat>(_ => new RepositorioHeartbeatJson(caminhoBaseDados));

        // Aplicacao: orquestrador (interface -> impl) e servicos de dominio.
        servicos.AddSingleton<IOrquestradorEnxame, OrquestradorEnxame>();
        servicos.AddSingleton<ServicoRobo>();
        servicos.AddSingleton<ServicoTarefa>();
        servicos.AddSingleton<ServicoAlerta>();
        servicos.AddSingleton<ServicoHeartbeat>();
        servicos.AddSingleton<MonitorDeFalhasService>();

        // Apresentacao: menus.
        servicos.AddSingleton<MenuRobos>();
        servicos.AddSingleton<MenuTarefas>();
        servicos.AddSingleton<MenuHeartbeats>();
        servicos.AddSingleton<MenuAlertas>();
        servicos.AddSingleton<MenuSimulacao>();
        servicos.AddSingleton<MenuPrincipal>();

        var provider = servicos.BuildServiceProvider(validateScopes: true);

        // Conecta os logs do monitor ao console.
        var monitor = provider.GetRequiredService<MonitorDeFalhasService>();
        monitor.AoLogar += msg => ConsoleUtils.EscreverAviso($"[MONITOR] {msg}");
        monitor.AoDetectarFalha += (robo, qtdTarefas) => ConsoleUtils.EscreverErro(
            $"[MONITOR] Robo {robo.Codigo} marcado como FALHA. Realocando {qtdTarefas} tarefa(s).");

        return provider;
    }
}
