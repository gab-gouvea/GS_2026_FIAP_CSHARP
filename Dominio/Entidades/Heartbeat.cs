using SwarmBuild.Dominio.Enums;
using SwarmBuild.Dominio.Structs;

namespace SwarmBuild.Dominio.Entidades;

/// <summary>
/// Pulso periodico enviado pelo robo. Contem bateria, posicao e status reportado.
/// Se o sistema parar de receber heartbeats, o MonitorDeFalhas marca o robo como FALHA.
/// </summary>
public sealed class Heartbeat
{
    public long Id { get; set; }
    public long RoboId { get; set; }
    public DateTime Timestamp { get; set; }
    public int Bateria { get; set; }
    public Coordenada? Coordenada { get; set; }
    public StatusRobo StatusReportado { get; set; }
    public string? Mensagem { get; set; }

    public Heartbeat() { }

    public void AoCriar()
    {
        if (Timestamp == default)
            Timestamp = DateTime.Now;
    }
}
