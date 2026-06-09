using SwarmBuild.Dominio.Entidades;
using SwarmBuild.Dominio.Enums;

namespace SwarmBuild.Aplicacao.Servicos;

/// <summary>
/// Parte da classe OrquestradorEnxame dedicada exclusivamente ao registro de alertas.
/// Separar em um arquivo proprio facilita a leitura e evidencia o uso de classes partial.
/// </summary>
public sealed partial class OrquestradorEnxame
{
    private void RegistrarAlertaRealocacao(Tarefa tarefa, Robo? anterior, Robo novo)
    {
        var alerta = new Alerta
        {
            Tipo = TipoAlerta.TAREFA_REALOCADA,
            Severidade = SeveridadeAlerta.AVISO,
            Mensagem = $"Tarefa {tarefa.Codigo} realocada de {anterior?.Codigo ?? "(sem robo)"} para {novo.Codigo}",
            TarefaId = tarefa.Id,
            RoboId = novo.Id
        };
        _repoAlerta.Salvar(alerta);
    }

    private void RegistrarAlertaSemRoboDisponivel(Tarefa tarefa, Robo? anterior)
    {
        var alerta = new Alerta
        {
            Tipo = TipoAlerta.TAREFA_SEM_ROBO_DISPONIVEL,
            Severidade = SeveridadeAlerta.CRITICO,
            Mensagem = $"Tarefa {tarefa.Codigo} sem robo {tarefa.TipoRoboRequerido} disponivel para realocar",
            TarefaId = tarefa.Id,
            RoboId = anterior?.Id
        };
        _repoAlerta.Salvar(alerta);
    }
}
