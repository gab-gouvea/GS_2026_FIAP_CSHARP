using SwarmBuild.Dominio.Excecoes;

namespace SwarmBuild.Apresentacao;

/// <summary>
/// Utilitarios estaticos para leitura e formatacao no console.
/// Encapsula parsing seguro, lancando EntradaInvalidaException quando aplicavel.
/// </summary>
public static class ConsoleUtils
{
    public static void EscreverTitulo(string titulo)
    {
        var linha = new string('=', Math.Max(40, titulo.Length + 4));
        Console.WriteLine();
        Console.WriteLine(linha);
        Console.WriteLine($"  {titulo}");
        Console.WriteLine(linha);
    }

    public static void EscreverSubtitulo(string subtitulo)
    {
        Console.WriteLine();
        Console.WriteLine($"-- {subtitulo} --");
    }

    public static void EscreverSucesso(string mensagem) => EscreverComCor(ConsoleColor.Green, $"OK: {mensagem}");
    public static void EscreverAviso(string mensagem) => EscreverComCor(ConsoleColor.Yellow, $"AVISO: {mensagem}");
    public static void EscreverErro(string mensagem) => EscreverComCor(ConsoleColor.Red, $"ERRO: {mensagem}");
    public static void EscreverInfo(string mensagem) => EscreverComCor(ConsoleColor.Cyan, mensagem);

    private static void EscreverComCor(ConsoleColor cor, string texto)
    {
        var anterior = Console.ForegroundColor;
        try
        {
            Console.ForegroundColor = cor;
            Console.WriteLine(texto);
        }
        finally
        {
            Console.ForegroundColor = anterior;
        }
    }

    public static string LerTexto(string rotulo, bool obrigatorio = true)
    {
        while (true)
        {
            Console.Write($"{rotulo}: ");
            var entrada = Console.ReadLine()?.Trim() ?? string.Empty;
            if (!obrigatorio || entrada.Length > 0)
                return entrada;
            EscreverAviso("Campo obrigatorio. Tente novamente.");
        }
    }

    public static int LerInteiro(string rotulo, int? minimo = null, int? maximo = null)
    {
        while (true)
        {
            var entrada = LerTexto(rotulo);
            if (!int.TryParse(entrada, out var valor))
            {
                EscreverAviso("Digite um numero inteiro valido.");
                continue;
            }
            if (minimo.HasValue && valor < minimo.Value)
            {
                EscreverAviso($"Valor minimo: {minimo}");
                continue;
            }
            if (maximo.HasValue && valor > maximo.Value)
            {
                EscreverAviso($"Valor maximo: {maximo}");
                continue;
            }
            return valor;
        }
    }

    public static long LerLong(string rotulo)
    {
        while (true)
        {
            var entrada = LerTexto(rotulo);
            if (long.TryParse(entrada, out var valor))
                return valor;
            EscreverAviso("Digite um numero inteiro valido.");
        }
    }

    public static double LerDouble(string rotulo, bool obrigatorio = true)
    {
        while (true)
        {
            var entrada = LerTexto(rotulo, obrigatorio);
            if (!obrigatorio && string.IsNullOrEmpty(entrada))
                return double.NaN;
            if (double.TryParse(entrada.Replace(',', '.'),
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var valor))
                return valor;
            EscreverAviso("Digite um numero valido (use ponto para decimais).");
        }
    }

    public static double? LerDoubleOpcional(string rotulo)
    {
        var valor = LerDouble(rotulo, obrigatorio: false);
        return double.IsNaN(valor) ? null : valor;
    }

    public static TEnum LerEnum<TEnum>(string rotulo) where TEnum : struct, Enum
    {
        var nomes = Enum.GetNames<TEnum>();
        Console.WriteLine($"Opcoes para {rotulo}:");
        for (var i = 0; i < nomes.Length; i++)
            Console.WriteLine($"  {i + 1}) {nomes[i]}");

        while (true)
        {
            var escolha = LerInteiro("Escolha o numero", 1, nomes.Length);
            if (Enum.TryParse<TEnum>(nomes[escolha - 1], out var valor))
                return valor;
            EscreverErro("Opcao invalida.");
        }
    }

    public static bool LerSimNao(string rotulo)
    {
        while (true)
        {
            Console.Write($"{rotulo} (s/n): ");
            var entrada = Console.ReadLine()?.Trim().ToLowerInvariant() ?? string.Empty;
            if (entrada is "s" or "sim" or "y" or "yes") return true;
            if (entrada is "n" or "nao" or "no") return false;
            EscreverAviso("Digite s ou n.");
        }
    }

    public static void Pausar()
    {
        Console.WriteLine();
        Console.Write("Pressione ENTER para continuar...");
        Console.ReadLine();
    }
}
