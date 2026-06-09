using SwarmBuild.Aplicacao.Dtos;
using SwarmBuild.Aplicacao.Servicos;
using SwarmBuild.Dominio.Enums;
using SwarmBuild.Dominio.Excecoes;

namespace SwarmBuild.Apresentacao;

public sealed class MenuHeartbeats
{
    private readonly ServicoHeartbeat _servico;

    public MenuHeartbeats(ServicoHeartbeat servico)
    {
        _servico = servico;
    }

    public void Exibir()
    {
        while (true)
        {
            ConsoleUtils.EscreverTitulo("HEARTBEATS");
            Console.WriteLine("1) Registrar heartbeat (um robo)");
            Console.WriteLine("2) Listar historico do robo");
            Console.WriteLine("0) Voltar");
            var op = ConsoleUtils.LerInteiro("Escolha", 0, 2);

            try
            {
                switch (op)
                {
                    case 1: Registrar(); break;
                    case 2: Historico(); break;
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

    private void Registrar()
    {
        ConsoleUtils.EscreverSubtitulo("Registrar heartbeat");
        var roboId = ConsoleUtils.LerLong("id do robo");
        var bateria = ConsoleUtils.LerInteiro("bateria (0-100)", 0, 100);
        var status = ConsoleUtils.LerEnum<StatusRobo>("status reportado");
        var lat = ConsoleUtils.LerDoubleOpcional("latitude (opcional)");
        var lon = ConsoleUtils.LerDoubleOpcional("longitude (opcional)");
        var msg = ConsoleUtils.LerTexto("mensagem (opcional, enter para pular)", obrigatorio: false);

        var dto = new HeartbeatDto(bateria, status, lat, lon, string.IsNullOrEmpty(msg) ? null : msg);
        var hb = _servico.Registrar(roboId, dto);
        ConsoleUtils.EscreverSucesso(
            $"Heartbeat #{hb.Id} registrado em {hb.Timestamp:dd/MM HH:mm:ss} (bat={hb.Bateria}%)");
    }

    private void Historico()
    {
        var roboId = ConsoleUtils.LerLong("id do robo");
        var historico = _servico.HistoricoDoRobo(roboId);
        if (historico.Count == 0)
        {
            ConsoleUtils.EscreverInfo("Nenhum heartbeat registrado para este robo.");
            return;
        }
        ConsoleUtils.EscreverSubtitulo($"Historico do robo #{roboId}");
        foreach (var h in historico)
        {
            Console.WriteLine(
                $"  {h.Timestamp:dd/MM HH:mm:ss} | bat={h.Bateria,3}% | {h.StatusReportado,-12} | {h.Mensagem ?? string.Empty}");
        }
    }
}
