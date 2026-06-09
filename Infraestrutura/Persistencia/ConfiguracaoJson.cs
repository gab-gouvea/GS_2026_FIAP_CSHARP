using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SwarmBuild.Infraestrutura.Persistencia;

/// <summary>
/// Configuracao centralizada de serializacao JSON.
/// Classe estatica - estado compartilhado e imutavel.
/// </summary>
public static class ConfiguracaoJson
{
    public static readonly JsonSerializerOptions Opcoes = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() },
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
}
