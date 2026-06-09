using SwarmBuild.Aplicacao.Interfaces;

namespace SwarmBuild.Infraestrutura.Tempo;

/// <summary>
/// Implementacao padrao de IRelogio que retorna o horario real do sistema.
/// </summary>
public sealed class RelogioDoSistema : IRelogio
{
    public DateTime Agora => DateTime.Now;
}
