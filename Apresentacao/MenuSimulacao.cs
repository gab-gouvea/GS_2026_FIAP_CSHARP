using SwarmBuild.Aplicacao.Dtos;
using SwarmBuild.Aplicacao.Interfaces;
using SwarmBuild.Aplicacao.Servicos;
using SwarmBuild.Dominio.Enums;
using SwarmBuild.Dominio.Excecoes;

namespace SwarmBuild.Apresentacao;

/// <summary>
/// Menu de simulacao - cria um cenario realista de construcao na Lua e demonstra
/// o fluxo completo: cadastro, atribuicao, falha de hardware, realocacao automatica
/// e geracao de alertas. Pensado para gerar a evidencia de execucao do projeto.
/// </summary>
public sealed class MenuSimulacao
{
    private readonly ServicoRobo _servicoRobo;
    private readonly ServicoTarefa _servicoTarefa;
    private readonly ServicoHeartbeat _servicoHeartbeat;
    private readonly ServicoAlerta _servicoAlerta;
    private readonly MonitorDeFalhasService _monitor;
    private readonly IRepositorioRobo _repoRobo;

    public MenuSimulacao(
        ServicoRobo servicoRobo,
        ServicoTarefa servicoTarefa,
        ServicoHeartbeat servicoHeartbeat,
        ServicoAlerta servicoAlerta,
        MonitorDeFalhasService monitor,
        IRepositorioRobo repoRobo)
    {
        _servicoRobo = servicoRobo;
        _servicoTarefa = servicoTarefa;
        _servicoHeartbeat = servicoHeartbeat;
        _servicoAlerta = servicoAlerta;
        _monitor = monitor;
        _repoRobo = repoRobo;
    }

    public void Exibir()
    {
        while (true)
        {
            ConsoleUtils.EscreverTitulo("SIMULACAO - CENARIO LUNAR ARTEMIS");
            Console.WriteLine("1) Executar cenario completo de realocacao automatica");
            Console.WriteLine("2) Forcar verificacao do monitor de falhas");
            Console.WriteLine("3) Iniciar monitor em background");
            Console.WriteLine("4) Parar monitor em background");
            Console.WriteLine("5) Limpar todos os dados (reset)");
            Console.WriteLine("0) Voltar");
            var op = ConsoleUtils.LerInteiro("Escolha", 0, 5);

            try
            {
                switch (op)
                {
                    case 1: ExecutarCenarioCompleto(); break;
                    case 2: ForcarVerificacao(); break;
                    case 3: IniciarMonitor(); break;
                    case 4: PararMonitor(); break;
                    case 5: ResetarDados(); break;
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

    private void ExecutarCenarioCompleto()
    {
        ConsoleUtils.EscreverTitulo("CENARIO: 3 robos, 2 tarefas, 1 falha de hardware");

        ConsoleUtils.EscreverSubtitulo("Passo 1/6 - Cadastrando enxame");
        var escav1 = CriarSeNaoExistir(new CriarRoboDto(
            "ESC-001", "Hephaestus", "Excavator-V2", TipoRobo.ESCAVADEIRA,
            -30.0, -45.0, CapacidadeCargaKg: 250, ProfundidadeMaximaMetros: 4.0));
        var escav2 = CriarSeNaoExistir(new CriarRoboDto(
            "ESC-002", "Vulcan", "Excavator-V2", TipoRobo.ESCAVADEIRA,
            -30.5, -45.5, CapacidadeCargaKg: 250, ProfundidadeMaximaMetros: 4.0));
        var transp = CriarSeNaoExistir(new CriarRoboDto(
            "TRN-001", "Mercury", "Hauler-L1", TipoRobo.TRANSPORTADOR,
            -30.2, -45.2, CapacidadeTransporteKg: 800, VelocidadeMaximaKmh: 12));

        Console.WriteLine($"   {escav1}");
        Console.WriteLine($"   {escav2}");
        Console.WriteLine($"   {transp}");

        ConsoleUtils.EscreverSubtitulo("Passo 2/6 - Heartbeats iniciais (todos saudaveis)");
        _servicoHeartbeat.Registrar(escav1.Id, new HeartbeatDto(95, StatusRobo.DISPONIVEL, -30.0, -45.0));
        _servicoHeartbeat.Registrar(escav2.Id, new HeartbeatDto(88, StatusRobo.DISPONIVEL, -30.5, -45.5));
        _servicoHeartbeat.Registrar(transp.Id, new HeartbeatDto(72, StatusRobo.DISPONIVEL, -30.2, -45.2));
        ConsoleUtils.EscreverInfo("3 heartbeats registrados.");

        ConsoleUtils.EscreverSubtitulo("Passo 3/6 - Criando tarefas de construcao");
        var tarefaEscav = CriarTarefaSeNaoExistir(new CriarTarefaDto(
            "T-CRATERA-A", "Escavar cratera A para fundacao do habitat",
            TipoRobo.ESCAVADEIRA, PrioridadeTarefa.ALTA, -30.05, -45.05));
        var tarefaTransp = CriarTarefaSeNaoExistir(new CriarTarefaDto(
            "T-TRANSP-A", "Transportar regolito da cratera A para o silo",
            TipoRobo.TRANSPORTADOR, PrioridadeTarefa.MEDIA, -30.2, -45.2));
        Console.WriteLine($"   {tarefaEscav.Codigo} - {tarefaEscav.Descricao}");
        Console.WriteLine($"   {tarefaTransp.Codigo} - {tarefaTransp.Descricao}");

        ConsoleUtils.EscreverSubtitulo("Passo 4/6 - Atribuicao automatica (escolhe robo mais proximo)");
        var atrib1 = _servicoTarefa.AtribuirMelhorRobo(tarefaEscav.Id);
        var atrib2 = _servicoTarefa.AtribuirMelhorRobo(tarefaTransp.Id);
        var roboEscolhido = _repoRobo.BuscarPorId(atrib1.RoboAtribuidoId!.Value)!;
        Console.WriteLine($"   Tarefa {atrib1.Codigo} -> Robo {roboEscolhido.Codigo} (proximidade venceu)");
        var roboTransp = _repoRobo.BuscarPorId(atrib2.RoboAtribuidoId!.Value)!;
        Console.WriteLine($"   Tarefa {atrib2.Codigo} -> Robo {roboTransp.Codigo}");

        ConsoleUtils.EscreverSubtitulo("Passo 5/6 - SIMULANDO FALHA: robo para de transmitir");
        // "Volta no tempo" o ultimo heartbeat do robo escolhido para forcar o timeout.
        roboEscolhido.UltimoHeartbeat = DateTime.Now.AddMinutes(-5);
        _repoRobo.Salvar(roboEscolhido);
        ConsoleUtils.EscreverAviso(
            $"Robo {roboEscolhido.Codigo} esta sem comunicacao ha 5 minutos (timeout: {_monitor.TimeoutSegundos}s).");

        ConsoleUtils.EscreverSubtitulo("Passo 6/6 - Monitor de falhas detecta e realoca automaticamente");
        var detectados = _monitor.DetectarRobosOffline();
        Console.WriteLine($"   Robos offline detectados: {detectados}");

        ConsoleUtils.EscreverSubtitulo("Resultado pos-realocacao");
        var todosRobos = _servicoRobo.Listar();
        foreach (var r in todosRobos)
            Console.WriteLine($"   {r}");

        ConsoleUtils.EscreverSubtitulo("Alertas gerados pelo sistema (sem intervencao humana)");
        var alertas = _servicoAlerta.Listar().OrderByDescending(a => a.CriadoEm).Take(5);
        foreach (var a in alertas)
            Console.WriteLine($"   [{a.Severidade}] {a.Tipo}: {a.Mensagem}");

        ConsoleUtils.EscreverSubtitulo("Tarefas apos realocacao");
        foreach (var t in _servicoTarefa.Listar())
        {
            var roboId = t.RoboAtribuidoId?.ToString() ?? "(sem)";
            Console.WriteLine(
                $"   [{t.Codigo}] {t.Status} | robo={roboId} | realocada={t.VezesRealocada}x");
        }

        ConsoleUtils.EscreverSucesso(
            "Cenario concluido. A missao continua mesmo com a falha individual de um robo.");
    }

    private void ForcarVerificacao()
    {
        ConsoleUtils.EscreverSubtitulo("Forcando verificacao do monitor de falhas");
        var detectados = _monitor.DetectarRobosOffline();
        if (detectados == 0)
            ConsoleUtils.EscreverInfo("Nenhum robo offline no momento.");
        else
            ConsoleUtils.EscreverAviso($"{detectados} robo(s) marcado(s) como FALHA. Veja a lista de alertas.");
    }

    private void IniciarMonitor()
    {
        if (_monitor.EstaAtivo)
        {
            ConsoleUtils.EscreverInfo("Monitor ja esta ativo.");
            return;
        }
        var intervalo = ConsoleUtils.LerInteiro("intervalo em segundos (ex: 10)", 1, 600) * 1000;
        _monitor.Iniciar(intervalo);
        ConsoleUtils.EscreverSucesso(
            $"Monitor iniciado. Verificando a cada {intervalo / 1000}s, timeout {_monitor.TimeoutSegundos}s.");
    }

    private void PararMonitor()
    {
        if (!_monitor.EstaAtivo)
        {
            ConsoleUtils.EscreverInfo("Monitor ja esta parado.");
            return;
        }
        _monitor.Parar();
        ConsoleUtils.EscreverSucesso("Monitor parado.");
    }

    private void ResetarDados()
    {
        if (!ConsoleUtils.LerSimNao("Confirma apagar TODOS os robos, tarefas, heartbeats e alertas?"))
            return;
        // Limpa via repositorios (sem invocar regras de negocio que bloqueiam delete).
        foreach (var t in _servicoTarefa.Listar())
            t.RoboAtribuidoId = null;
        _repoRobo.Limpar();
        // Os outros repos sao limpos via DI - recriar nao e trivial aqui, entao limpa um a um.
        ConsoleUtils.EscreverInfo("Apenas robos foram limpos. Reinicie o app para zerar todos os dados.");
    }

    private Dominio.Entidades.Robo CriarSeNaoExistir(CriarRoboDto dto)
    {
        try
        {
            return _servicoRobo.Criar(dto);
        }
        catch (CodigoDuplicadoException)
        {
            return _servicoRobo.BuscarPorCodigo(dto.Codigo);
        }
    }

    private Dominio.Entidades.Tarefa CriarTarefaSeNaoExistir(CriarTarefaDto dto)
    {
        try
        {
            return _servicoTarefa.Criar(dto);
        }
        catch (CodigoDuplicadoException)
        {
            return _servicoTarefa.Listar().First(t =>
                string.Equals(t.Codigo, dto.Codigo, StringComparison.OrdinalIgnoreCase));
        }
    }
}
