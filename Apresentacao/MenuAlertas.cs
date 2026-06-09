using SwarmBuild.Aplicacao.Servicos;
using SwarmBuild.Dominio.Excecoes;

namespace SwarmBuild.Apresentacao;

public sealed class MenuAlertas
{
    private readonly ServicoAlerta _servico;

    public MenuAlertas(ServicoAlerta servico)
    {
        _servico = servico;
    }

    public void Exibir()
    {
        while (true)
        {
            ConsoleUtils.EscreverTitulo("ALERTAS DO ENXAME");
            Console.WriteLine("1) Listar todos");
            Console.WriteLine("2) Listar nao resolvidos");
            Console.WriteLine("3) Listar resolvidos");
            Console.WriteLine("4) Resolver alerta");
            Console.WriteLine("0) Voltar");
            var op = ConsoleUtils.LerInteiro("Escolha", 0, 4);

            try
            {
                switch (op)
                {
                    case 1: Listar(null); break;
                    case 2: Listar(false); break;
                    case 3: Listar(true); break;
                    case 4: Resolver(); break;
                    case 0: return;
                }
            }
            catch (SwarmBuildException ex)
            {
                ConsoleUtils.EscreverErro(ex.Message);
            }
            catch (Exception ex)
            {
                ConsoleUtils.EscreverErro($"Erro inesperado: {ex.Message}");
            }
            ConsoleUtils.Pausar();
        }
    }

    private void Listar(bool? resolvido)
    {
        var titulo = resolvido switch
        {
            null => "Todos os alertas",
            false => "Alertas nao resolvidos",
            true => "Alertas resolvidos"
        };
        ConsoleUtils.EscreverSubtitulo(titulo);
        var alertas = _servico.Listar(resolvido);
        if (alertas.Count == 0)
        {
            ConsoleUtils.EscreverInfo("Nenhum alerta encontrado.");
            return;
        }
        foreach (var a in alertas.OrderByDescending(x => x.CriadoEm))
        {
            var statusStr = a.Resolvido ? "RESOLVIDO" : "ABERTO";
            Console.WriteLine(
                $"  id={a.Id} | {a.CriadoEm:dd/MM HH:mm:ss} | {a.Severidade,-8} | {a.Tipo,-26} | {statusStr}");
            Console.WriteLine($"      {a.Mensagem}");
        }
    }

    private void Resolver()
    {
        var id = ConsoleUtils.LerLong("id do alerta");
        var resolvido = _servico.Resolver(id);
        ConsoleUtils.EscreverSucesso(
            $"Alerta #{resolvido.Id} resolvido em {resolvido.ResolvidoEm:dd/MM HH:mm:ss}");
    }
}
