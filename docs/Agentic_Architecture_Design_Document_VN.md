
# Agentic Architecture Design Document
## VnMLStudio.Plugin.LlamaCppSharp — Agentic Orchestration Layer

**Version:** 1.0  
**Date:** 2026-08-14  
**Scope:** P0 Reflection → P1 Planning & Multi-Agent → P2 Learning & Proactive → P3 Dynamic Tool Synthesis  
**Author:** Hoang Nguyen Cong
---

## 1. Executive Summary

Tài liệu này mô tả kiến trúc agentic từ **Level 2 (Simple Agent)** lên **Level 5 (Self-improving Agent)** cho hệ thống LLM local (LlamaCppSharp). Kiến trúc được thiết kế theo dạng **layered, drop-in replacement**, cho phép nâng cấp từng level mà không phá vỡ harness gốc.

| Level | Tên | Đặc điểm chính |
|:-----:|-----|----------------|
| 0 | Static LLM | Chỉ trả lời, không tool |
| 1 | Tool-using LLM | RAG, Function Calling |
| 2 | Simple Agent | ReAct + Memory + Tools |
| 3 | Autonomous Agent | Planning + Reflection + Multi-step |
| 4 | Multi-Agent System | Collaboration + Delegation |
| 5 | Self-improving Agent | Learning + Adaptation + Tool Synthesis |

---

## 2. High-Level Design

### 2.1 System Architecture

```mermaid
flowchart TB
    subgraph APP["Application Layer"]
        UI[User Interface]
        API[API Gateway]
    end

    subgraph ORCH["Agentic Orchestration Layer"]
        direction TB
        PA[ProactiveAgentOrchestrator<br/>P2 — Scheduler]
        LA[LearningReflectiveOrchestrator<br/>P2 — Experience]
        HA[HierarchicalReflectiveOrchestrator<br/>P1 — Planner]
        MA[AgentTeamOrchestrator<br/>P1 — Multi-Agent]
        RA[ReflectiveReActStreamingOrchestrator<br/>P0 — Reflection]
    end

    subgraph HARNESS["Existing Harness (Giữ nguyên)"]
        SH[StreamingAgentHarness]
        RE[ReAct Orchestrator]
        GR[Guardrails]
    end

    subgraph MEM["Memory & Tools (Mở rộng)"]
        VM[VectorMemory<br/>Qdrant/SQLite]
        ES[EpisodicStore<br/>Experience]
        SB[SkillBank<br/>Vector Store]
        DTR[DynamicToolRegistry<br/>SQLite]
        TS[ToolSynthesizer<br/>LLM + Roslyn]
    end

    UI --> API
    API --> PA
    PA --> LA
    LA --> HA
    HA --> MA
    MA --> RA
    RA --> SH
    SH --> RE
    RE --> GR
    
    LA --> VM
    LA --> ES
    LA --> SB
    HA --> DTR
    HA --> TS
    PA --> VM
```

### 2.2 Orchestrator Chain (Decorator Pattern)

```mermaid
flowchart LR
    subgraph LEVEL5["Level 5 — Self-improving"]
        direction LR
        P[Proactive] --> L[Learning]
        L --> T[Team]
        T --> H[Hierarchical]
        H --> R[Reflective]
    end
    
    R --> HARNESS[Existing Harness]
```
```mermaid
graph TD
    Input[User Prompt / Request] --> P3[Proactive Level 4 - Outermost]
    P3 --> P2[Learning Level 3]
    P2 --> P1[Hierarchical Planner Level 2]
    P1 --> P0[Reflective ReAct Level 1 - Innermost]
    P0 --> Llama[Llama.cpp / Gemma 4 E2B]
    
    style P0 fill:#f9f,stroke:#333,stroke-width:2px
    style P1 fill:#bbf,stroke:#333,stroke-width:2px
    style P2 fill:#fbf,stroke:#333,stroke-width:2px
    style P3 fill:#f96,stroke:#333,stroke-width:2px
```
**Nguyên tắc:** Mỗi orchestrator wrap orchestrator bên trong qua `IStreamingAgentOrchestrator`. Có thể bật/tắt từng layer qua DI mà không sửa code.

### 2.3 Data Flow — End to End

```mermaid
sequenceDiagram
    actor U as User
    participant P as ProactiveOrchestrator
    participant L as LearningOrchestrator
    participant H as HierarchicalOrchestrator
    participant R as ReflectiveOrchestrator
    participant LLM as LlamaCppApiClient
    participant TS as ToolSynthesizer
    participant DB as SQLite/Qdrant

    U->>P: "Calculate mortgage..."
    P->>L: RunStreamingAsync
    L->>L: Retrieve skills (RAG)
    L->>H: Enriched input
    H->>H: CreatePlan (DAG)
    H->>R: Execute sub-task
    R->>LLM: ChatAsync
    LLM-->>R: Need CalculateMortgage
    R->>R: Tool not found!
    R-->>H: SubTaskFailed
    H->>TS: ShouldSynthesize?
    TS->>TS: Generate C# code
    TS->>TS: Roslyn compile
    TS->>DB: Save Active tool
    TS-->>H: New tool ready
    H->>H: Replan with new tool
    H->>R: Retry sub-task
    R->>LLM: Tool success
    R-->>H: SubTaskComplete
    H-->>L: PlanComplete
    L->>L: Save episode + Learn
    L->>DB: EpisodicStore + SkillBank
    L-->>P: FinalAnswer
    P->>P: Parse proactive intent?
    P->>DB: Schedule job (optional)
    P-->>U: Answer + Job ID
```

---

## 3. Agent Capability Levels

### 3.1 Capability Matrix

| Tiêu chí | L0 | L1 | L2 | L3 | L4 | L5 |
|----------|:--:|:--:|:--:|:--:|:--:|:--:|
| Tool Use | ❌ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Memory | ❌ | ⚠️ | ✅ | ✅ | ✅ | ✅ |
| Planning | ❌ | ❌ | ⚠️ | ✅ | ✅ | ✅ |
| Autonomy | ❌ | ❌ | ⚠️ | ✅ | ✅ | ✅ |
| Reflection | ❌ | ❌ | ❌ | ✅ | ✅ | ✅ |
| Task Decomposition | ❌ | ❌ | ❌ | ✅ | ✅ | ✅ |
| Multi-Agent | ❌ | ❌ | ❌ | ❌ | ✅ | ✅ |
| Learning | ❌ | ❌ | ❌ | ❌ | ❌ | ✅ |
| Event-Driven | ❌ | ❌ | ❌ | ❌ | ❌ | ✅ |
| Tool Synthesis | ❌ | ❌ | ❌ | ❌ | ❌ | ✅ |

### 3.2 Level Transition Diagram

```mermaid
stateDiagram-v2
    [*] --> L0: Static LLM
    L0 --> L1: Add ToolRegistry
    L1 --> L2: Add ReAct + SlidingWindowMemory
    L2 --> L3: Add ReflectionEngine + HierarchicalTaskPlanner
    L3 --> L4: Add AgentTeam + SupervisorOrchestrator
    L4 --> L5: Add LearningEngine + ProactiveScheduler + ToolSynthesizer
    L5 --> L5: Self-improve loop
```

---

## 4. Detailed Design

### 4.1 P0 — Reflection Engine

```mermaid
classDiagram
    class IReflectionEngine {
        +EvaluateStepAsync() ReflectionResult
        +EvaluateFinalAnswerAsync() ReflectionResult
    }
    class DefaultReflectionEngine {
        +HeuristicCheck()
        +LlmDeepEvaluation()
    }
    class ReflectionContext {
        +OriginalGoal
        +GeneratedContent
        +ConversationHistory
        +CurrentIteration
    }
    class ReflectionIssue {
        +Type: StuckLoop|Hallucination|Incomplete|Safety
        +Severity
        +Suggestion
    }
    IReflectionEngine <|-- DefaultReflectionEngine
    DefaultReflectionEngine --> ReflectionContext
    DefaultReflectionEngine --> ReflectionIssue
```

**Phát hiện:**
- Stuck loop: gọi cùng tool 3+ lần
- Hallucination: claim có source nhưng chưa gọi tool
- Misinterpret: tool trả empty nhưng claim "found X"
- Context window risk: auto-summarize

### 4.2 P1 — Hierarchical Task Planner

```mermaid
flowchart TD
    A[User Goal] --> B[HierarchicalTaskPlanner]
    B --> C[LLM phân rã goal]
    C --> D[Plan: DAG of SubTasks]
    D --> E[Topological Sort]
    E --> F[PlanExecutor]
    F --> G[ReflectiveReAct per SubTask]
    G --> H[Aggregate Results]
    H --> I[Final Reflection]
    I --> J[Final Answer]
    
    G -->|Fail| K[Replan or Retry]
    K --> F
```

**Components:**
- `ITaskPlanner`: tạo/refine plan
- `Plan`: DAG với dependency
- `PlanExecutor`: topological sort + parallel execution nếu độc lập

### 4.3 P1 — Agent Team

```mermaid
flowchart TD
    U[User Goal] --> S[SupervisorOrchestrator]
    S --> R[ResearchAgent]
    S --> C[CoderAgent]
    S --> V[ReviewerAgent]
    R -->|Result| S
    C -->|Result| S
    V -->|Approval/Critique| S
    S -->|Aggregate| A[Final Answer]
```

### 4.4 P2 — Learning from Experience

```mermaid
flowchart LR
    subgraph EPISODE["Episode"]
        E1[UserQuery]
        E2[ToolCalls]
        E3[Reflection]
        E4[Outcome]
    end
    
    EPISODE --> LE[LearningEngine]
    LE --> SE[SkillExtractor<br/>LLM prompt]
    LE --> PM[PatternMiner]
    
    SE --> SKILL[Skill]
    PM --> SKILL
    
    SKILL --> SB[SkillBank<br/>Vector Store]
    EPISODE --> ES[EpisodicStore<br/>SQLite+Qdrant]
    
    SB -->|Retrieve| ORCH[LearningReflectiveOrchestrator]
    ES -->|Search| ORCH
```

**Skill lifecycle:**
```
Draft → Validate on 3 similar episodes → Active (rate ≥ 70%)
                                    → Rejected (rate < 30% after 5 uses)
```

### 4.5 P2 — Proactive Scheduler

```mermaid
flowchart TD
    A[LLM Output] --> B{Parse Proactive Intent}
    B -->|Interval| C[IntervalTrigger]
    B -->|Scheduled| D[ScheduledTrigger]
    B -->|Condition| E[ConditionTrigger]
    C --> F[AgentScheduler]
    D --> F
    E --> F
    F --> G[Background Loop]
    G -->|Fire| H[Execute Job<br/>Silent]
    H -->|Has new info| I[Notify User]
    H -->|No change| J[Silent continue]
```

### 4.6 P3 — Dynamic Tool Synthesis

```mermaid
sequenceDiagram
    participant O as HierarchicalOrchestrator
    participant S as DynamicToolSynthesizer
    participant G as ToolCodeGenerator<br/>(LLM)
    participant V as ToolValidator
    participant R as DynamicToolRegistry
    participant C as Roslyn Compiler

    O->>S: Tool "CalculateMortgage" not found
    S->>S: ShouldSynthesize? (heuristic)
    S->>G: Generate C# source
    G-->>S: SourceCode + JsonSchema
    S->>V: ValidateAsync
    V->>C: Compile to DLL
    C-->>V: Success / Errors
    alt Compile OK
        V-->>S: Passed
        S->>R: Save (Status=Active)
        S-->>O: Return tool
        O->>O: Replan + Retry
    else Compile Fail
        V-->>S: Rejected
        S->>R: Save (Status=Rejected)
        S-->>O: Null
    end
```

**Safety guards:**
- SQL: block DROP, DELETE, TRUNCATE, EXEC, sp_, xp_
- C#: compile trong MemoryStream, không load vào app domain chính
- Max synthesis per plan: default 2

---

## 5. Quick Implementation Guideline

### 5.1 Phase 1: P0 Reflection (1-2 tuần)

```bash
# 1. Implement ReflectionEngine
# 2. Wrap ReflectiveStreamingAgentHarness
# 3. Test với các case: stuck loop, hallucination
```

```csharp
services.AddSingleton<IReflectionEngine>(sp => new DefaultReflectionEngine(
    sp.GetRequiredService<LlamaCppApiClient>(),
    new ReflectionEngineOptions(),
    sp.GetRequiredService<ILoggerFactory>()));
```

### 5.2 Phase 2: P1 Planning + Multi-Agent (2-3 tuần)

```bash
# 1. Implement HierarchicalTaskPlanner
# 2. Implement AgentTeam (Research + Coder + Reviewer)
# 3. Wire vào HierarchicalReflectiveOrchestrator
```

```csharp
services.AddSingleton<ITaskPlanner, HierarchicalTaskPlanner>();
services.AddSingleton<IStreamingAgentOrchestrator>(sp => new HierarchicalReflectiveOrchestrator(
    sp.GetRequiredService<ITaskPlanner>(),
    sp.GetRequiredService<ReflectiveReActStreamingOrchestrator>(),
    planReflection: sp.GetRequiredService<IReflectionEngine>(),
    loggerFactory: sp.GetRequiredService<ILoggerFactory>()));
```

### 5.3 Phase 3: P2 Learning + Proactive (2-3 tuần)

```bash
# 1. Setup HybridEpisodicStore (SQLite + embedding)
# 2. Implement LearningEngine
# 3. Implement ProactiveAgentOrchestrator
# 4. Wrap: Proactive → Learning → Hierarchical
```

```csharp
services.AddSingleton<IEpisodicStore>(sp => new HybridEpisodicStore(
    sp.GetRequiredService<IEmbeddingService>(),
    sp.GetRequiredService<ILoggerFactory>()));

services.AddSingleton<ISkillBank, InMemorySkillBank>();
services.AddSingleton<ILearningEngine>(sp => new LearningEngine(
    sp.GetRequiredService<LlamaCppApiClient>(),
    sp.GetRequiredService<IEpisodicStore>(),
    sp.GetRequiredService<IEmbeddingService>(),
    sp.GetRequiredService<ISkillBank>(),
    sp.GetRequiredService<ILoggerFactory>()));

services.AddSingleton<IStreamingAgentOrchestrator>(sp =>
{
    var baseOrc = new ReflectiveReActStreamingOrchestrator(...);
    var learningOrc = new LearningReflectiveOrchestrator(baseOrc, ...);
    var hierarchicalOrc = new HierarchicalReflectiveOrchestrator(
        sp.GetRequiredService<ITaskPlanner>(), learningOrc, ...);
    return new ProactiveAgentOrchestrator(hierarchicalOrc, ...);
});
```

### 5.4 Phase 4: P3 Tool Synthesis (2-3 tuần)

```bash
# 1. Implement DynamicToolRegistry
# 2. Implement ToolCodeGenerator + ToolValidator
# 3. Wire vào HierarchicalReflectiveOrchestrator
```

```csharp
services.AddSingleton<IDynamicToolRegistry>(sp => new DynamicToolRegistry(
    sp.GetRequiredService<ILoggerFactory>()));
services.AddSingleton<IToolCodeGenerator>(sp => new ToolCodeGenerator(
    sp.GetRequiredService<LlamaCppApiClient>(), ...));
services.AddSingleton<IToolValidator, ToolValidator>();
services.AddSingleton<IToolSynthesizer, DynamicToolSynthesizer>();
```

---

## 6. Best Practices

### 6.1 Memory & Storage

| Practice | Lý do |
|----------|-------|
| Dùng `DateTimeOffset` + `.ToString("O")` | Consistent timezone handling |
| `SemaphoreSlim` cho SQLite access | Thread safety |
| Hybrid: SQLite metadata + Qdrant vector | Balance query vs semantic search |
| Embed cả `UserQuery + FinalAnswer + Tags` | Semantic search chính xác hơn |

### 6.2 Reflection

- **Heuristic trước, LLM sau:** heuristic nhanh, LLM đắt → chỉ gọi LLM khi heuristic ambiguous
- **Max iterations:** luôn giới hạn `MaxIterations` và `MaxToolRounds`
- **Context window:** monitor token count, auto-summarize khi > 80% limit

### 6.3 Learning

- **Chỉ learn từ episodes rõ ràng:** success score < 0.7 hoặc failure score > 0.3 → skip
- **Skill validation:** test trên 3 episodes lịch sử trước khi promote Active
- **EMA success rate:** `rate = 0.9 * old + 0.1 * new` — smooth, không nhạy với outlier

### 6.4 Tool Synthesis

- **Sandbox compile:** dùng `CSharpCompilation.Emit(MemoryStream)`, không write disk nếu có thể
- **Max synthesis per plan:** default 2, tránh infinite synthesis loop
- **Heuristic trigger:** chỉ synthesize khi error chứa "not found", "missing", "unsupported" — không trigger với runtime error
- **SQL blacklist:** block DROP, DELETE, TRUNCATE, EXEC, sp_, xp_

### 6.5 Orchestrator Chain

```
Proactive (outermost)
  → Learning
    → AgentTeam
      → HierarchicalPlanner
        → ReflectiveReAct (innermost)
```

**Nguyên tắc:** Outer layers enrich input / schedule follow-up. Inner layers execute. Reflection ở mọi layer.

---

## 7. Use Case — When to Use Which Level

### 7.1 Decision Matrix

| Use Case | Level | Lý do |
|----------|:-----:|-------|
| FAQ chatbot đơn giản | **L0-L1** | Không cần tool, chỉ cần RAG |
| Tìm kiếm tài liệu + trích xuất thông tin | **L1-L2** | ReAct + Search tool |
| Phân tích dữ liệu đa bước | **L3** | Hierarchical planner để phân rã |
| Code review + generate + review lại | **L4** | Multi-Agent: Coder + Reviewer |
| Agent tự học cách debug lỗi mới | **L5** | Learning + Tool Synthesis |
| Theo dõi giá cổ phiếu, notify khi đạt ngưỡng | **L5** | Proactive + Learning |
| Tích hợp API mà không có SDK sẵn | **L5** | Dynamic Tool Synthesis |

### 7.2 Level Selection Flowchart

```mermaid
flowchart TD
    A[Start] --> B{Need tools?}
    B -->|No| C[Level 0: Static LLM]
    B -->|Yes| D{Need multi-step reasoning?}
    D -->|No| E[Level 1-2: Tool-using Agent]
    D -->|Yes| F{Need planning?}
    F -->|No| G[Level 2: ReAct Agent]
    F -->|Yes| H{Need collaboration?}
    H -->|No| I[Level 3: Autonomous Agent]
    H -->|Yes| J{Need self-improve?}
    J -->|No| K[Level 4: Multi-Agent]
    J -->|Yes| L[Level 5: Self-improving Agent]
```

### 7.3 Concrete Examples

| Scenario | Level | Components Active |
|----------|:-----:|-------------------|
| "Tìm giá iPhone 15" | L2 | ReAct + SearchWeb tool |
| "So sánh giá iPhone 15 trên 3 trang, tính trung bình" | L3 | HierarchicalPlanner: [SearchA]→[SearchB]→[SearchC]→[CalcAvg] |
| "Viết API endpoint, viết test, review code" | L4 | AgentTeam: Coder → Reviewer → Coder (fix) |
| "Debug lỗi này, nhớ cách fix để lần sau dùng lại" | L5 | LearningEngine + EpisodicStore + SkillBank |
| "Theo dõi giá Bitcoin, báo tôi khi xuống dưới 50k" | L5 | ProactiveScheduler + ConditionTrigger |
| "Tôi cần tool tính IRR nhưng không có sẵn" | L5 | DynamicToolSynthesizer → Generate C# → Compile → Retry |

---

## 8. Appendix

### 8.1 Full DI Registration (Level 5)

```csharp
// Program.cs
builder.Services
    // P0: Reflection
    .AddSingleton<IReflectionEngine>(sp => new DefaultReflectionEngine(
        sp.GetRequiredService<LlamaCppApiClient>(),
        new ReflectionEngineOptions(),
        sp.GetRequiredService<ILoggerFactory>()))
    
    // P1: Planning
    .AddSingleton<ITaskPlanner, HierarchicalTaskPlanner>()
    
    // P2: Learning
    .AddSingleton<IEpisodicStore>(sp => new HybridEpisodicStore(
        sp.GetRequiredService<IEmbeddingService>(),
        sp.GetRequiredService<ILoggerFactory>(),
        "Data Source=agent.db;Mode=ReadWriteCreate;Cache=Shared"))
    .AddSingleton<ISkillBank, InMemorySkillBank>()
    .AddSingleton<ILearningEngine>(sp => new LearningEngine(
        sp.GetRequiredService<LlamaCppApiClient>(),
        sp.GetRequiredService<IEpisodicStore>(),
        sp.GetRequiredService<IEmbeddingService>(),
        sp.GetRequiredService<ISkillBank>(),
        sp.GetRequiredService<ILoggerFactory>()))
    
    // P2: Proactive
    .AddSingleton<IAgentScheduler, InMemoryAgentScheduler>()
    
    // P3: Tool Synthesis
    .AddSingleton<IDynamicToolRegistry>(sp => new DynamicToolRegistry(
        sp.GetRequiredService<ILoggerFactory>(),
        "Data Source=agent.db;Mode=ReadWriteCreate;Cache=Shared"))
    .AddSingleton<IToolCodeGenerator>(sp => new ToolCodeGenerator(
        sp.GetRequiredService<LlamaCppApiClient>(),
        sp.GetRequiredService<ILoggerFactory>()))
    .AddSingleton<IToolValidator, ToolValidator>()
    .AddSingleton<IToolSynthesizer>(sp => new DynamicToolSynthesizer(
        sp.GetRequiredService<IToolCodeGenerator>(),
        sp.GetRequiredService<IToolValidator>(),
        sp.GetRequiredService<IDynamicToolRegistry>(),
        sp.GetRequiredService<ILoggerFactory>()))
    
    // Orchestrator Chain (inner → outer)
    .AddSingleton<IStreamingAgentOrchestrator>(sp =>
    {
        var client = sp.GetRequiredService<LlamaCppApiClient>();
        var reflection = sp.GetRequiredService<IReflectionEngine>();
        var planner = sp.GetRequiredService<ITaskPlanner>();
        var learning = sp.GetRequiredService<ILearningEngine>();
        var episodic = sp.GetRequiredService<IEpisodicStore>();
        var embedder = sp.GetRequiredService<IEmbeddingService>();
        var scheduler = sp.GetRequiredService<IAgentScheduler>();
        var synthesizer = sp.GetRequiredService<IToolSynthesizer>();
        var toolRegistry = sp.GetRequiredService<IDynamicToolRegistry>();
        var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

        // Innermost
        var baseOrc = new ReflectiveReActStreamingOrchestrator(
            client, reflection, options: null);

        // P2: Learning
        var learningOrc = new LearningReflectiveOrchestrator(
            baseOrc, learning, episodic, embedder, loggerFactory);

        // P1: Hierarchical
        var hierarchicalOrc = new HierarchicalReflectiveOrchestrator(
            planner, learningOrc,
            planReflection: reflection,
            toolSynthesizer: synthesizer,
            dynamicToolRegistry: toolRegistry,
            options: new HierarchicalOptions { MaxSynthesisPerPlan = 2 },
            loggerFactory: loggerFactory);

        // P2: Proactive (outermost)
        return new ProactiveAgentOrchestrator(
            hierarchicalOrc, scheduler, reflection,
            new ProactiveOptions(), loggerFactory);
    });
```

### 8.2 Database Schema Summary

| Table | Purpose | Module |
|-------|---------|--------|
| `Episodes` | Lưu trajectory (query, tools, reflection, outcome) | P2 Learning |
| `SynthesizedTools` | Lưu tool tự sinh (source code, schema, status) | P3 Tool Synthesis |
| `ModuleSettings` | Settings app | Existing |
| `EncryptedSettings` | Settings mã hóa | Existing |

### 8.3 Event Flow Summary

```mermaid
flowchart LR
    subgraph EVENTS["Cross-Module Events"]
        E1[EpisodeLearned]
        E2[SkillExtracted]
        E3[ToolSynthesized]
        E4[ToolRejected]
        E5[JobScheduled]
    end
    
    E1 -->|Trigger| E2
    E3 -->|Register| E1
    E5 -->|After turn| E1
```

---

**End of Document**
