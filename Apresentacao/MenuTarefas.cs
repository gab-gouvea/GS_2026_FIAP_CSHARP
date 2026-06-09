using SwarmBuild.Aplicacao.Dtos;
using SwarmBuild.Aplicacao.Servicos;
using SwarmBuild.Dominio.Enums;
using SwarmBuild.Dominio.Excecoes;

namespace SwarmBuild.Apresentacao;

public sealed class MenuTarefas
{
    private readonly ServicoTarefa _servico;

    public MenuTarefas(ServicoTarefa servico)
    {
        _servico = servico;
    }

    public void Exibir()
    {
        while (true)
        {
            ConsoleUtils.EscreverTitulo("TAREFAS DA MISSAO");
            Console.WriteLine("1) Listar tarefas");
            Console.WriteLine("2) Criar tarefa");
            Console.WriteLine("3) Atribuir ao melhor robo");
            Console.WriteLine("4) Atribuir a um robo especifico");
            Console.WriteLine("5) Concluir tarefa");
            Console.WriteLine("6) Forcar realocacao");
            Console.WriteLine("7) Deletar tarefa");
            Console.WriteLine("0) Voltar");
            var op = ConsoleUtils.LerInteiro("Escolha", 0, 7);

            try
            {
                switch (op)
                {
                    case 1: Listar(); break;
                    case 2: Criar(); break;
                    case 3: AtribuirMelhor(); break;
                    case 4: AtribuirEspecifico(); break;
                    case 5: Concluir(); break;
                    case 6: Realocar(); break;
                    case 7: Deletar(); break;
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

    private void Listar()
    {
        ConsoleUtils.EscreverSubtitulo("Lista de tarefas");
        var tarefas = _servico.Listar();
        if (tarefas.Count == 0)
        {
            ConsoleUtils.EscreverInfo("Nenhuma tarefa cadastrada.");
            return;
        }
        foreach (var t in tarefas)
        {
            var roboInfo = t.RoboAtribuidoId.HasValue ? $"robo#{t.RoboAtribuidoId}" : "(nao atribuida)";
            Console.WriteLine(
                $"  id={t.Id} [{t.Codigo}] {t.TipoRoboRequerido} | {t.Status} | prio={t.Prioridade}");
            Console.WriteLine(
                $"      {t.Descricao}");
            Console.WriteLine(
                $"      {roboInfo} | realocada={t.VezesRealocada}x | criada={t.CriadaEm:dd/MM HH:mm}");
        }
    }

    private void Criar()
    {
        ConsoleUtils.EscreverSubtitulo("Criar nova tarefa");
        var codigo = ConsoleUtils.LerTexto("codigo");
        var descricao = ConsoleUtils.LerTexto("descricao");
        var tipo = ConsoleUtils.LerEnum<TipoRobo>("tipo de robo requerido");
        var prio = ConsoleUtils.LerEnum<PrioridadeTarefa>("prioridade");
        var lat = ConsoleUtils.LerDoubleOpcional("latitude (opcional)");
        var lon = ConsoleUtils.LerDoubleOpcional("longitude (opcional)");

        var dto = new CriarTarefaDto(codigo, descricao, tipo, prio, lat, lon);
        var criada = _servico.Criar(dto);
        ConsoleUtils.EscreverSucesso($"Tarefa criada: id={criada.Id} codigo={criada.Codigo}");
    }

    private void AtribuirMelhor()
    {
        var id = ConsoleUtils.LerLong("id da tarefa");
        var atualizada = _servico.AtribuirMelhorRobo(id);
        ConsoleUtils.EscreverSucesso(
            $"Tarefa {atualizada.Codigo} atribuida ao robo #{atualizada.RoboAtribuidoId}");
    }

    private void AtribuirEspecifico()
    {
        var tarefaId = ConsoleUtils.LerLong("id da tarefa");
        var roboId = ConsoleUtils.LerLong("id do robo");
        var atualizada = _servico.AtribuirRoboEspecifico(tarefaId, roboId);
        ConsoleUtils.EscreverSucesso(
            $"Tarefa {atualizada.Codigo} atribuida ao robo #{atualizada.RoboAtribuidoId}");
    }

    private void Concluir()
    {
        var id = ConsoleUtils.LerLong("id da tarefa");
        var concluida = _servico.Concluir(id);
        ConsoleUtils.EscreverSucesso(
            $"Tarefa {concluida.Codigo} concluida em {concluida.ConcluidaEm:dd/MM/yyyy HH:mm:ss}");
    }

    private void Realocar()
    {
        var id = ConsoleUtils.LerLong("id da tarefa");
        var realocada = _servico.Realocar(id);
        var novo = realocada.RoboAtribuidoId.HasValue
            ? $"para robo #{realocada.RoboAtribuidoId}"
            : "SEM ROBO DISPONIVEL (alerta gerado)";
        ConsoleUtils.EscreverSucesso($"Tarefa {realocada.Codigo} {novo}");
    }

    private void Deletar()
    {
        var id = ConsoleUtils.LerLong("id da tarefa");
        if (!ConsoleUtils.LerSimNao($"Confirma exclusao da tarefa #{id}?"))
            return;
        _servico.Deletar(id);
        ConsoleUtils.EscreverSucesso($"Tarefa #{id} removida.");
    }
}
