using SwarmBuild.Aplicacao.Dtos;
using SwarmBuild.Aplicacao.Servicos;
using SwarmBuild.Dominio.Enums;
using SwarmBuild.Dominio.Excecoes;

namespace SwarmBuild.Apresentacao;

public sealed class MenuRobos
{
    private readonly ServicoRobo _servico;

    public MenuRobos(ServicoRobo servico)
    {
        _servico = servico;
    }

    public void Exibir()
    {
        while (true)
        {
            ConsoleUtils.EscreverTitulo("ROBOS DO ENXAME");
            Console.WriteLine("1) Listar robos");
            Console.WriteLine("2) Criar robo");
            Console.WriteLine("3) Detalhar robo");
            Console.WriteLine("4) Atualizar status manualmente");
            Console.WriteLine("5) Deletar robo");
            Console.WriteLine("0) Voltar");
            var op = ConsoleUtils.LerInteiro("Escolha", 0, 5);

            try
            {
                switch (op)
                {
                    case 1: Listar(); break;
                    case 2: Criar(); break;
                    case 3: Detalhar(); break;
                    case 4: AtualizarStatus(); break;
                    case 5: Deletar(); break;
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
        ConsoleUtils.EscreverSubtitulo("Lista de robos");
        var robos = _servico.Listar();
        if (robos.Count == 0)
        {
            ConsoleUtils.EscreverInfo("Nenhum robo cadastrado.");
            return;
        }
        foreach (var r in robos)
        {
            Console.WriteLine($"  id={r.Id} {r}");
            Console.WriteLine($"      {r.DescricaoCapacidade()}");
            Console.WriteLine($"      coord={r.Coordenada?.ToString() ?? "(sem)"} | ultimoHB={r.UltimoHeartbeat?.ToString("HH:mm:ss") ?? "-"}");
        }
    }

    private void Criar()
    {
        ConsoleUtils.EscreverSubtitulo("Criar novo robo");
        var tipo = ConsoleUtils.LerEnum<TipoRobo>("tipo");
        var codigo = ConsoleUtils.LerTexto("codigo");
        var nome = ConsoleUtils.LerTexto("nome");
        var modelo = ConsoleUtils.LerTexto("modelo");
        var lat = ConsoleUtils.LerDoubleOpcional("latitude (opcional, enter para pular)");
        var lon = ConsoleUtils.LerDoubleOpcional("longitude (opcional, enter para pular)");

        var dto = tipo switch
        {
            TipoRobo.ESCAVADEIRA => new CriarRoboDto(codigo, nome, modelo, tipo, lat, lon,
                CapacidadeCargaKg: ConsoleUtils.LerDouble("capacidade de carga (kg)"),
                ProfundidadeMaximaMetros: ConsoleUtils.LerDouble("profundidade maxima (m)")),
            TipoRobo.TRANSPORTADOR => new CriarRoboDto(codigo, nome, modelo, tipo, lat, lon,
                CapacidadeTransporteKg: ConsoleUtils.LerDouble("capacidade de transporte (kg)"),
                VelocidadeMaximaKmh: ConsoleUtils.LerDouble("velocidade maxima (km/h)")),
            TipoRobo.MONTADOR => new CriarRoboDto(codigo, nome, modelo, tipo, lat, lon,
                PrecisaoMontagemMm: ConsoleUtils.LerDouble("precisao de montagem (mm)"),
                BracosManipuladores: ConsoleUtils.LerInteiro("bracos manipuladores", 1)),
            _ => throw new EntradaInvalidaException("Tipo desconhecido")
        };

        var criado = _servico.Criar(dto);
        ConsoleUtils.EscreverSucesso($"Robo criado: id={criado.Id} | {criado}");
    }

    private void Detalhar()
    {
        var id = ConsoleUtils.LerLong("id do robo");
        var robo = _servico.Buscar(id);
        ConsoleUtils.EscreverSubtitulo($"Robo #{robo.Id}");
        Console.WriteLine($"  {robo}");
        Console.WriteLine($"  capacidade: {robo.DescricaoCapacidade()}");
        Console.WriteLine($"  coordenada: {robo.Coordenada?.ToString() ?? "(sem)"}");
        Console.WriteLine($"  criado em : {robo.CriadoEm:dd/MM/yyyy HH:mm:ss}");
        Console.WriteLine($"  ultimo HB : {robo.UltimoHeartbeat?.ToString("dd/MM/yyyy HH:mm:ss") ?? "(nunca)"}");
    }

    private void AtualizarStatus()
    {
        var id = ConsoleUtils.LerLong("id do robo");
        var novo = ConsoleUtils.LerEnum<StatusRobo>("novo status");
        var atualizado = _servico.AtualizarStatus(id, novo);
        ConsoleUtils.EscreverSucesso($"Status atualizado para {atualizado.Status}");
    }

    private void Deletar()
    {
        var id = ConsoleUtils.LerLong("id do robo");
        if (!ConsoleUtils.LerSimNao($"Confirma exclusao do robo #{id}?"))
            return;
        _servico.Deletar(id);
        ConsoleUtils.EscreverSucesso($"Robo #{id} removido.");
    }
}
