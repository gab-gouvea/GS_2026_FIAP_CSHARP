namespace SwarmBuild.Apresentacao;

public sealed class MenuPrincipal
{
    private readonly MenuRobos _menuRobos;
    private readonly MenuTarefas _menuTarefas;
    private readonly MenuHeartbeats _menuHeartbeats;
    private readonly MenuAlertas _menuAlertas;
    private readonly MenuSimulacao _menuSimulacao;

    public MenuPrincipal(
        MenuRobos menuRobos,
        MenuTarefas menuTarefas,
        MenuHeartbeats menuHeartbeats,
        MenuAlertas menuAlertas,
        MenuSimulacao menuSimulacao)
    {
        _menuRobos = menuRobos;
        _menuTarefas = menuTarefas;
        _menuHeartbeats = menuHeartbeats;
        _menuAlertas = menuAlertas;
        _menuSimulacao = menuSimulacao;
    }

    public void Executar()
    {
        ExibirBanner();
        while (true)
        {
            Console.WriteLine();
            ConsoleUtils.EscreverTitulo("SWARMBUILD - MENU PRINCIPAL");
            Console.WriteLine("1) Robos");
            Console.WriteLine("2) Tarefas");
            Console.WriteLine("3) Heartbeats");
            Console.WriteLine("4) Alertas");
            Console.WriteLine("5) Simulacao (cenario lunar)");
            Console.WriteLine("0) Sair");
            var op = ConsoleUtils.LerInteiro("Escolha", 0, 5);

            switch (op)
            {
                case 1: _menuRobos.Exibir(); break;
                case 2: _menuTarefas.Exibir(); break;
                case 3: _menuHeartbeats.Exibir(); break;
                case 4: _menuAlertas.Exibir(); break;
                case 5: _menuSimulacao.Exibir(); break;
                case 0:
                    ConsoleUtils.EscreverInfo("Encerrando SwarmBuild. Ate a proxima missao.");
                    return;
            }
        }
    }

    private static void ExibirBanner()
    {
        Console.WriteLine();
        Console.WriteLine("####################################################");
        Console.WriteLine("#                                                  #");
        Console.WriteLine("#     SwarmBuild - Orquestracao de Enxame          #");
        Console.WriteLine("#     Construcao Robotica Autonoma                 #");
        Console.WriteLine("#     Global Solution 2026 - Space Connect         #");
        Console.WriteLine("#                                                  #");
        Console.WriteLine("####################################################");
    }
}
