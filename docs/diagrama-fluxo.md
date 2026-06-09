# Diagramas de fluxo - SwarmBuild

Este documento detalha os dois fluxos principais do sistema: **atribuicao de tarefa** e **deteccao de falha + realocacao automatica**.

---

## 1. Atribuicao de tarefa ao melhor robo

```mermaid
flowchart TD
    A[Usuario: criar tarefa] --> B[ServicoTarefa.Criar]
    B --> C{Codigo ja existe?}
    C -->|Sim| D[CodigoDuplicadoException]
    C -->|Nao| E[Persiste tarefa PENDENTE]
    E --> F[Usuario: atribuir ao melhor robo]
    F --> G[ServicoTarefa.AtribuirMelhorRobo]
    G --> H[Orquestrador.EscolherMelhorRobo]
    H --> I[Repositorio.BuscarDisponiveis]
    I --> J[Filtra por tipo compativel]
    J --> K{Tarefa tem coordenada?}
    K -->|Sim| L[Ordena por distancia euclidiana]
    K -->|Nao| M[Ordena por bateria desc.]
    L --> N{Tem candidato?}
    M --> N
    N -->|Nao| O[RegraDeNegocioException]
    N -->|Sim| P[Robo.Status = EM_TAREFA]
    P --> Q[Tarefa.AtribuirA robo]
    Q --> R[Persiste robo + tarefa]
```

---

## 2. Deteccao de falha + realocacao automatica

```mermaid
sequenceDiagram
    autonumber
    participant U as Usuario
    participant MON as MonitorDeFalhasService
    participant REPO as Repositorios JSON
    participant ORQ as OrquestradorEnxame
    participant ALT as Repositorio de Alertas

    U->>MON: Iniciar() ou DetectarRobosOffline()
    Note right of MON: Timer dispara a cada 10s
    MON->>REPO: BuscarOffline(agora - 60s)
    REPO-->>MON: [Robo A]
    MON->>REPO: Robo A.Status = FALHA
    MON->>ALT: Alerta ROBO_OFFLINE (CRITICO)
    MON->>REPO: BuscarEmExecucaoPorRobo(Robo A)
    REPO-->>MON: [Tarefa T-001]
    MON->>ORQ: RealocarTarefa(T-001)
    ORQ->>REPO: T-001.Desatribuir + MarcarRealocada
    ORQ->>REPO: BuscarDisponiveis (mesmo tipo)
    REPO-->>ORQ: [Robo B]
    alt Tem substituto
        ORQ->>REPO: Robo B.Status = EM_TAREFA
        ORQ->>REPO: T-001.AtribuirA(Robo B)
        ORQ->>ALT: Alerta TAREFA_REALOCADA (AVISO)
    else Sem substituto
        ORQ->>ALT: Alerta TAREFA_SEM_ROBO_DISPONIVEL (CRITICO)
    end
```

---

## 3. Camadas e injecao de dependencia

```mermaid
flowchart LR
    subgraph Apresentacao
        MP[MenuPrincipal]
        MR[MenuRobos]
        MT[MenuTarefas]
        MH[MenuHeartbeats]
        MA[MenuAlertas]
        MS[MenuSimulacao]
    end
    subgraph Aplicacao
        SR[ServicoRobo]
        ST[ServicoTarefa]
        SH[ServicoHeartbeat]
        SA[ServicoAlerta]
        OR[OrquestradorEnxame]
        MON[MonitorDeFalhasService]
    end
    subgraph Dominio
        E[Entidades]
        EX[Excecoes]
        STR[Structs]
    end
    subgraph Infraestrutura
        RR[RepositorioRoboJson]
        RT[RepositorioTarefaJson]
        RA[RepositorioAlertaJson]
        RH[RepositorioHeartbeatJson]
        REL[RelogioDoSistema]
    end

    MP --> MR & MT & MH & MA & MS
    MR --> SR
    MT --> ST
    MH --> SH
    MA --> SA
    MS --> SR & ST & SH & SA & MON

    SR --> RR & RT & RA & RH & OR
    ST --> RT & RR & OR
    SH --> RH & RR & RA & SR & REL
    SA --> RA
    OR --> RR & RT & RA
    MON --> RR & RT & RA & OR & REL

    SR -.usa.-> E & EX
    ST -.usa.-> E & EX
    OR -.usa.-> E & STR
```

A apresentacao depende apenas dos servicos. Os servicos dependem apenas das interfaces dos repositorios e do orquestrador. As implementacoes concretas (`*Json`) ficam isoladas em `Infraestrutura/` — isso e o que **inversao de dependencia** garante.
