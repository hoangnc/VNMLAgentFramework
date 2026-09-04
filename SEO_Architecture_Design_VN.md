# SEOCrawlSupervisorOrchestrator - High-Level Design & Detail Design

## Document Revision History

| Version | Date | Author | Changes | Description |
|---|---|---|---|---|
| 1.0.0 | 2026-08-28 | System | Initial Release | First version with basic ReAct loop |
| 1.1.0 | 2026-08-29 | System | Architecture Refactor | Introduced Chain of Responsibility pattern with 6 handlers |
| 1.2.0 | 2026-08-30 | System | Dynamic Timeout | Added token-based timeout calculation for LLM operations |
| 1.3.0 | 2026-08-31 | System | Event-Driven Progress | Unified streaming/non-streaming via Progress events with emoji logging |
| 1.4.0 | 2026-08-31 | System | Reflection Engine | Extracted Reflection logic into dedicated engine with 4-layer heuristic |
| 1.5.0 | 2026-09-04 | System | File-Based Processing | BREAKING: Removed LLM tool calls for file operations; handlers now read/write files directly and pass content via context |

# 1. Executive Summary

## 1.1 System Overview
The **SEOCrawlSupervisorOrchestrator** is a production-grade, pipeline-based orchestration system designed for automated SEO content processing. It manages end-to-end workflows including:

**Web crawling** – extract content from target URLs

**SEO analysis** – evaluate keyword density, readability, structure

**Content writing** – generate SEO-optimized articles

**Quality reflection** – multi-layer quality assurance with auto-correction

**Result persistence** – save articles and generate comprehensive reports

## 1.2 Key Features

| Feature | Description | Status |
| --- | --- | --- |
| Pipeline Architecture | Chain of Responsibility with 7 handlers | ✅ Complete |
| File-Based Processing | Handlers read/write files directly, LLM no longer calls file tools | ✅ Complete |
| Dynamic Timeout | Token-based timeout calculation per LLM with thinking mode | ✅ Complete |
| Event-Driven Progress | Real-time UI updates via Progress events | ✅ Complete |
| Vietnamese SEO | Custom heuristic for Vietnamese content | ✅ Complete |
| Auto Tool Synthesis | Dynamic tool generation for missing capabilities | ✅ Complete |
| 4-Layer Reflection | Duplicate words, templates, density, readability | ✅ Complete |
| Auto-Correction | Rewrite content when quality fails | ✅ Complete |
| Emoji Logging | Rich visual feedback with emojis | ✅ Complete |
## 1.3 Technology Stack

| Layer | Technology |
| --- | --- |
| Language | C# (.NET 10) |
| AI/LLM | LlamaCppSharp |
| Design Pattern | Chain of Responsibility, Pipeline, Observer |
| Logging | Serilog, Microsoft.Extensions.Logging |
| Data Format | JSON, Markdown |
| Concurrency | Async/Await, CancellationToken, Channels |
# 2. High-Level Design (HLD)

## 2.1 System Architecture

```mermaid
flowchart TD
    subgraph UI["User Interface"]
        A[User Input]
        B[Real-time Progress]
        C[Final Report]
    end

    subgraph Orchestrator["SEOCrawlSupervisorOrchestrator"]
        D[URL Parser]
        E[Pipeline Executor]
        F[Event Subscriber]
        G[Report Generator]
    end

    subgraph Pipeline["Pipeline (Chain of Responsibility)"]
        H1["UrlValidationHandler"]
        H2["UrlQuantityValidationHandler"]
        H3["ScrapingHandler"]
        H4["AnalysisHandler"]
        H5["ContentWritingHandler"]
        H6["ReflectionHandler"]
        H7["SavingHandler"]
    end

    subgraph Engine["Core Engines"]
        E1[TimeoutCalculator]
        E2[ReflectionEngine]
        E3[LoggingFactory]
    end

    subgraph Workers["Specialist Agents"]
        W1[Scraper Agent]
        W2[SEO Analyzer]
        W3[Content Writer]
    end

    subgraph Storage["Data Storage"]
        S1[(JSON Files)]
        S2[(Markdown Files)]
        S3[(Reports)]
    end

    A --> D
    D --> E
    E --> Pipeline
    H1 --> H2 --> H3 --> H4 --> H5 --> H6 --> H7
    H3 -.-> W1
    H4 -.-> W2
    H5 -.-> W3
    H6 -.-> E2
    E --> F
    F --> B
    E --> G
    G --> C
    E1 --> Pipeline
    E3 --> Pipeline
    W1 --> S1
    W2 --> S1
    W3 --> S2
    H7 --> S2
    G --> S3
```

## 2.2 Data Flow – File-Based Processing (v1.5.0)

```mermaid
flowchart TD
    subgraph Input["Input"]
        URL[User Input]
    end

    subgraph Handlers["Pipeline Handlers"]
        H1[1. Validation]
        H2[2. Quantity Check]
        H3[3. Scraping]
        H4[4. Analysis]
        H5[5. Writing]
        H6[6. Reflection]
        H7[7. Saving]
    end

    subgraph Data["Data Flow"]
        D1[ScrapedFilePath]
        D2[ScrapedContent]
        D3[AnalysisFilePath]
        D4[AnalysisContent]
        D5[PrimaryKeyword]
        D6[ArticleFilePath]
        D7[ArticleContent]
    end

    H3 -->|writes| D1
    H3 -->|extracts| D2
    H4 -->|reads| D2
    H4 -->|writes| D3
    H4 -->|extracts| D4
    H4 -->|extracts| D5
    H5 -->|reads| D2
    H5 -->|reads| D4
    H5 -->|reads| D5
    H5 -->|writes| D6
    H5 -->|extracts| D7
    H6 -->|reads| D7
    H7 -->|reads| D6
    H7 -->|reads| D7
    H7 -->|generates| Final[SEOUrlResult]
```

## 2.3 Component Overview

```mermaid
classDiagram
    class IStreamingAgentOrchestrator {
        <<interface>>
        +RunAsync()
        +RunStreamingAsync()
    }
    
    class SEOCrawlSupervisorOrchestrator {
        -AgentTeam _team
        -SEOCrawlSupervisorOptions _options
        -UrlProcessingHandler _pipelineHead
        -Dictionary~string,SEOReflectionResult~ _reflectionCache
        -Dictionary~string,HashSet~string~~ _scrapedEntityCache
        -HashSet~string~ _attemptedSynthesis
        +BuildPipeline()
        +RunAsync()
        +RunStreamingAsync()
        -FormatProgressLog()
        -ExtractScore()
    }
    
    class UrlProcessingHandler {
        <<abstract>>
        #ILogger _logger
        #UrlProcessingHandler _next
        +event Progress
        +SetNext()
        +HandleAsync()
        #OnProgress()
        #StartPhase()
        #CompletePhase()
        #CreateTimeoutCts()
        #CreateDynamicTimeoutCts()
    }
    
    class UrlValidationHandler {
        +HandleAsync()
    }
    
    class UrlQuantityValidationHandler {
        +HandleAsync()
    }
    
    class ScrapingHandler {
        +HandleAsync()
        #BuildScraperPrompt()
        #PrecalculateTimeouts()
    }
    
    class AnalysisHandler {
        +HandleAsync()
        #BuildAnalysisPrompt()
        -ExtractScoreFromAnalysis()
    }
    
    class ContentWritingHandler {
        +HandleAsync()
        #BuildWritePrompt()
        -ReadFileAndExtractContent()
    }
    
    class ReflectionHandler {
        -ReflectionEngine _engine
        +HandleAsync()
        -PerformRewriteAsync()
        -OnEngineProgress()
    }
    
    class ReflectionEngine {
        -ILogger _logger
        -SEOCrawlSupervisorOptions _options
        +event Progress
        +RunReflectionAsync()
        -CheckDuplicateWordsAsync()
        -CheckTemplatePhrasesAsync()
        -CheckKeywordDensityAsync()
        -CheckReadabilityAsync()
        -CheckLLMAsync()
    }
    
    class SavingHandler {
        +HandleAsync()
        -SaveFinalReport()
        -CopyArticle()
    }
    
    class TimeoutCalculator {
        <<static>>
        +CalculateTimeout()
        +CalculateByContentLength()
        +CalculateAllTimeouts()
    }
    
    class LoggingFactory {
        <<static>>
        +CreateLogger()
        +DisposeAll()
    }
    
    IStreamingAgentOrchestrator <|.. SEOCrawlSupervisorOrchestrator
    SEOCrawlSupervisorOrchestrator --> UrlProcessingHandler : uses
    UrlProcessingHandler <|-- UrlValidationHandler
    UrlProcessingHandler <|-- UrlQuantityValidationHandler
    UrlProcessingHandler <|-- ScrapingHandler
    UrlProcessingHandler <|-- AnalysisHandler
    UrlProcessingHandler <|-- ContentWritingHandler
    UrlProcessingHandler <|-- ReflectionHandler
    UrlProcessingHandler <|-- SavingHandler
    ReflectionHandler --> ReflectionEngine : uses
    SEOCrawlSupervisorOrchestrator --> TimeoutCalculator : uses
    UrlProcessingHandler --> TimeoutCalculator : uses
    UrlProcessingHandler --> LoggingFactory : uses
```

# 3. Detail Design (DD)

## 3.1 Chain of Responsibility Implementation

```mermaid
sequenceDiagram
    participant O as Orchestrator
    participant V as Validation
    participant Q as Quantity
    participant S as Scraping
    participant A as Analysis
    participant W as Writing
    participant R as Reflection
    participant SV as Saving

    O->>V: HandleAsync(context)
    activate V
    V->>V: Validate URL
    V-->>O: Progress Event
    V->>Q: HandleAsync(context)
    activate Q
    Q->>Q: Check URL count
    Q-->>O: Progress Event
    Q->>S: HandleAsync(context)
    activate S
    S->>S: Call Scraper Worker
    S->>S: Read scraped file → extract content
    S->>S: Store ScrapedContent in context
    S-->>O: Progress Event
    S->>A: HandleAsync(context)
    activate A
    A->>A: Read ScrapedContent from context
    A->>A: Call Analyzer Worker (JSON output)
    A->>A: Save JSON to file
    A->>A: Store AnalysisContent + PrimaryKeyword
    A-->>O: Progress Event
    A->>W: HandleAsync(context)
    activate W
    W->>W: Read ScrapedContent + AnalysisContent
    W->>W: Call Writer Worker (Markdown output)
    W->>W: Save Markdown to file
    W->>W: Store ArticleContent in context
    W-->>O: Progress Event
    W->>R: HandleAsync(context)
    activate R
    R->>R: Read ArticleContent from context
    R->>R: Run Reflection Engine (4 layers)
    R-->>O: Progress Event
    R->>SV: HandleAsync(context)
    activate SV
    SV->>SV: Copy article to final directory
    SV->>SV: Generate SEOUrlResult
    SV-->>O: Progress Event
    deactivate SV
    deactivate R
    deactivate W
    deactivate A
    deactivate S
    deactivate Q
    deactivate V
```

## 3.2 File-Based Data Flow (Key Changes in v1.5.0)

```mermaid
flowchart TD
    subgraph Context["UrlProcessingContext - WorkerOutputs"]
        K1["ScrapedFilePath: string"]
        K2["ScrapedContent: string (JSON)"]
        K3["AnalysisFilePath: string"]
        K4["AnalysisContent: string (JSON)"]
        K5["AnalysisScore: string"]
        K6["PrimaryKeyword: string"]
        K7["ArticleFilePath: string"]
        K8["ArticleContent: string (Markdown)"]
        K9["ReflectionResult: SEOReflectionResult"]
    end

    subgraph Handlers["Handlers"]
        S[ScrapingHandler]
        A[AnalysisHandler]
        W[ContentWritingHandler]
        R[ReflectionHandler]
        SV[SavingHandler]
    end

    S -->|writes| K1
    S -->|extracts| K2
    A -->|reads| K2
    A -->|writes| K3
    A -->|extracts| K4
    A -->|extracts| K5
    A -->|extracts| K6
    W -->|reads| K2
    W -->|reads| K4
    W -->|reads| K6
    W -->|writes| K7
    W -->|extracts| K8
    R -->|reads| K8
    R -->|writes| K9
    SV -->|reads| K7
    SV -->|reads| K8
    SV -->|reads| K9
```

## 3.3 Reflection Engine - 4 Layers with Status Notification

```mermaid
flowchart TD
    subgraph Input["Input"]
        C[Article Content]
        U[Source URL]
    end

    subgraph L1["Layer 1: Duplicate Words"]
        L1I[Split into words]
        L1C[Count frequency]
        L1D[Detect >3 occurrences]
        L1R[Pass if none found]
        L1N[Notify: ✅ PASS / ❌ FAIL]
    end

    subgraph L1B["Layer 1B: Template Phrases"]
        L1BI[Detect common templates]
        L1BC[Count occurrences]
        L1BD[Detect ≥2 occurrences]
        L1BR[Pass if none found]
        L1BN[Notify: ✅ PASS / ❌ FAIL]
    end

    subgraph L2["Layer 2: Keyword Density"]
        L2I[Extract keywords]
        L2C[Calculate density]
        L2D[Check 1.2%-3.5% range]
        L2R[Pass if in range]
        L2N[Notify: ✅ PASS / ❌ FAIL]
    end

    subgraph L3["Layer 3: Readability"]
        L3I[Count sentences]
        L3C[Average length]
        L3D[Detect long sentences]
        L3R[Pass if score ≥ 60]
        L3N[Notify: ✅ PASS / ❌ FAIL]
    end

    subgraph L4["Layer 4: LLM Reflection"]
        L4I[Build prompt]
        L4C[Call LLM]
        L4D[JSON output]
        L4R[Pass if all good]
        L4N[Notify: ✅ PASS / ❌ FAIL]
    end

    subgraph Output["Output"]
        SUM[Summary Result]
        ISS[Issues List]
        SUG[Suggestions List]
        SCR[Score Calculation]
        NOT[Final Notification]
    end

    C --> L1
    C --> L1B
    C --> L2
    C --> L3
    C --> L4
    U --> L4

    L1 --> L1R --> L1N
    L1B --> L1BR --> L1BN
    L2 --> L2R --> L2N
    L3 --> L3R --> L3N
    L4 --> L4R --> L4N

    L1N --> SUM
    L1BN --> SUM
    L2N --> SUM
    L3N --> SUM
    L4N --> SUM

    L1R --> ISS
    L1BR --> ISS
    L2R --> ISS
    L3R --> ISS
    L4R --> ISS

    ISS --> SUG
    SUG --> SCR
    SUM --> SCR
    SCR --> NOT
```

## 3.4 Dynamic Timeout with Thinking Mode

```mermaid
flowchart TD
    subgraph Input["Input Parameters"]
        CL[Content Length]
        PL[Prompt Length]
        RL[Response Length]
        HM[Handler Multiplier]
    end

    subgraph Config["Configuration"]
        TPS[Tokens Per Second]
        SBM[Safety Buffer Multiplier]
        MIN[Min Timeout]
        MAX[Max Timeout]
        CPT[Chars Per Token]
        ER[Enable Reasoning]
        RTP[Reasoning Time Per Token]
        RBS[Reasoning Base Seconds]
        RM[Reasoning Multiplier]
    end

    subgraph Calculate["Calculation"]
        TE["Total Chars = PL + RL"]
        ET["Estimated Tokens = TE / CPT"]
        BT["Base Time = ET / TPS"]
        ST["Safe Time = BT * HM * SBM"]

        RT["Reasoning Time = ET * RTP + RBS * RM"]
        FT["Final Time = ST + RT"]
        TO["Clamped Timeout = Clamp(FT, MIN, MAX)"]
    end

    CL --> TE
    PL --> TE
    RL --> TE
    TE --> ET
    CPT --> ET
    ET --> BT
    TPS --> BT
    BT --> ST
    HM --> ST
    SBM --> ST

    ET --> RT
    RTP --> RT
    RBS --> RT
    RM --> RT

    ST --> FT
    RT --> FT
    FT --> TO
    MIN --> TO
    MAX --> TO

    TO --> T[Timeout]
```

## 3.5 Event-Driven Progress Flow

```mermaid
sequenceDiagram
    participant H as Handler
    participant E as Event
    participant O as Orchestrator
    participant C as Channel
    participant UI as UI Renderer

    H->>H: Process phase
    H->>E: OnProgress()
    E->>O: Progress Event
    O->>O: FormatProgressLog()
    O->>C: Write to Channel
    C->>UI: Read from Channel
    UI->>UI: Render Markdown

    Note over H,UI: For each handler phase

    H->>H: Complete phase
    H->>E: OnProgress(isComplete=true)
    E->>O: Progress Event
    O->>O: FormatProgressLog()
    O->>C: Write to Channel
    C->>UI: Read from Channel
    UI->>UI: Render completion

# 4. Component Details
```

## 4.1 UrlProcessingContext (v1.5.0)

```csharp
public class UrlProcessingContext
{
    // === Core Info ===
    public string Url { get; set; } = string.Empty;
    public int Index { get; set; }
    public int Total { get; set; }
    public int ContentLength { get; set; }
    public int PromptLength { get; set; }

    // === Worker Outputs (File-Based) ===
    public Dictionary<string, string> WorkerOutputs { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    // === Phase Tracking ===
    public List<PhaseTiming> PhaseTimings { get; set; } = new();
    public bool Success { get; set; } = true;
    public string? FailureReason { get; set; }
    public int RewriteRounds { get; set; }
    public SEOUrlResult? Result { get; set; }

    // === Timeout ===
    public Dictionary<string, TimeSpan> AdjustedTimeouts { get; set; } = new();

    // === Dependencies ===
    public SEOCrawlSupervisorOptions Options { get; set; } = null!;
    public AgentTeam Team { get; set; } = null!;
    public Dictionary<string, SEOReflectionResult> ReflectionCache { get; set; } = null!;
    public Dictionary<string, HashSet<string>> ScrapedEntityCache { get; set; } = null!;
    public HashSet<string> AttemptedSynthesis { get; set; } = null!;
    public DynamicToolSynthesizer? ToolSynthesizer { get; set; }
    public IDynamicToolRegistry? DynamicRegistry { get; set; }
    public ILoggerFactory? LoggerFactory { get; set; }
    public CancellationToken CancellationToken { get; set; }
    public ILogger Logger { get; set; } = null!;
}
```

## 4.2 WorkerOutputs - Standard Keys

| Key | Type | Description | Written By | Read By |
| --- | --- | --- | --- | --- |
| ScrapedFilePath | string | Path to scraped JSON file | ScrapingHandler | (reference) |
| ScrapedContent | string | Raw JSON content of scraped file | ScrapingHandler | AnalysisHandler, ContentWritingHandler |
| AnalysisFilePath | string | Path to analysis JSON file | AnalysisHandler | (reference) |
| AnalysisContent | string | Analysis JSON content | AnalysisHandler | ContentWritingHandler, SavingHandler |
| AnalysisScore | string | SEO score extracted from analysis | AnalysisHandler | SavingHandler |
| PrimaryKeyword | string | Primary keyword from analysis | AnalysisHandler | ContentWritingHandler |
| ArticleFilePath | string | Path to article MD file | ContentWritingHandler | SavingHandler |
| ArticleContent | string | Markdown article content | ContentWritingHandler | ReflectionHandler, SavingHandler |
| ReflectionResult | SEOReflectionResult | Reflection result | ReflectionHandler | SavingHandler |
## 4.3 SEOCrawlSupervisorOptions

```csharp
public class SEOCrawlSupervisorOptions
{
    // === Core Settings ===
    public bool EnableFinalReflection { get; set; } = true;
    public bool EnableAutoToolSynthesis { get; set; } = false;
    public bool SaveArticlesToFiles { get; set; } = true;
    public bool SaveFinalReport { get; set; } = true;
    public bool SkipL4IfHeuristicPass { get; set; } = true;
    public int MaxUrls { get; set; } = 10;

    // === Timeout Settings ===
    public int BatchTotalTimeoutMinutes { get; set; } = 120;
    public int? UrlTotalTimeoutMinutes { get; set; } = 15;
    public int MaxCorrectionRounds { get; set; } = 1;
    public int? WorkerTimeoutSeconds { get; set; } = 120;
    public int? AgentTimeoutSeconds { get; set; } = 60;

    // === LLM Token Settings ===
    public LlmConfig LlmConfig { get; set; } = new();

    // === Output Settings ===
    public string OutputDirectory { get; set; } = "./output";

    // === Handler Multipliers ===
    public Dictionary<string, double> HandlerTokenMultipliers { get; set; } = new()
    {
        ["Scrape"] = 0.5,
        ["Analyze"] = 1.0,
        ["Write"] = 2.5,
        ["Reflect"] = 1.8,
        ["Save"] = 0.3
    };
}

public class LlmConfig
{
    // === Token & Timeout ===
    public double TokensPerSecond { get; set; } = 50;
    public double SafetyBufferMultiplier { get; set; } = 1.5;
    public int MinTimeoutSeconds { get; set; } = 30;
    public int MaxTimeoutSeconds { get; set; } = 900;
    public double CharsPerToken { get; set; } = 4.0;

    // === Thinking / Reasoning ===
    public bool EnableReasoning { get; set; } = true;
    public double ReasoningTimePerToken { get; set; } = 0.02;
    public double ReasoningBaseSeconds { get; set; } = 5.0;
    public double ReasoningMultiplier { get; set; } = 1.8;
}
```

## 4.4 ProgressEventArgs

```csharp
public class ProgressEventArgs : EventArgs
{
    public string Phase { get; set; } = string.Empty;
    public int Step { get; set; }
    public int TotalSteps { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsComplete { get; set; }
    public DateTime Timestamp { get; set; }
    public Exception? Error { get; set; }
    public Dictionary<string, object>? Data { get; set; }
    public int PercentComplete { get; set; }
    public string? Status { get; set; }
    public string? Detail { get; set; }
    public int Score { get; set; }
}
```
# 5. Handler Implementation Details

## 5.1 ScrapingHandler

```mermaid
flowchart LR
    subgraph Steps["Processing Steps"]
        S1[Find Scraper Worker]
        S2[Call LLM with prompt]
        S3[Receive file path]
        S4[Read file → extract content]
        S5[Store ScrapedContent in context]
        S6[Cache entities]
    end

    S1 --> S2 --> S3 --> S4 --> S5 --> S6
```

**Key Implementation**:

- Calls LLM to crawl URL

- LLM returns path to JSON file

- Handler reads file and extracts content

- Stores ScrapedContent and ScrapedFilePath in WorkerOutputs

- Caches entities for later use

## 5.2 AnalysisHandler

```mermaid
flowchart LR
    subgraph Steps["Processing Steps"]
        A1[Read ScrapedContent from context]
        A2[Build prompt with scraped content]
        A3[Call LLM → receive JSON]
        A4[Save JSON to file]
        A5[Parse JSON → extract data]
        A6[Store AnalysisContent + PrimaryKeyword + Score]
    end

    A1 --> A2 --> A3 --> A4 --> A5 --> A6
```

**Key Implementation**:

- Reads ScrapedContent from context

- LLM returns JSON directly (no file tool calls)

- Handler saves JSON to file

- Extracts top_keywords, score, readability

- Stores AnalysisContent, PrimaryKeyword, AnalysisScore

## 5.3 ContentWritingHandler

```mermaid
flowchart LR
    subgraph Steps["Processing Steps"]
        W1[Read ScrapedContent + AnalysisContent]
        W2[Extract PrimaryKeyword]
        W3[Build prompt with strict structure]
        W4[Call LLM → receive Markdown]
        W5[Save Markdown to file]
        W6[Store ArticleContent in context]
    end

    W1 --> W2 --> W3 --> W4 --> W5 --> W6
```

**Key Implementation:**

- Reads ScrapedContent, AnalysisContent, PrimaryKeyword

- LLM returns Markdown directly (no file tool calls)

- Handler saves Markdown to file

- Stores ArticleContent in context

- Prompt enforces:

- Meta Title & Description

- H1/H2/H3 structure

- 800-1000 words

- No template phrases

- 5+ entities, 2+ numbers

## 5.4 ReflectionHandler

```mermaid
flowchart LR
    subgraph Steps["Processing Steps"]
        R1[Read ArticleContent from context]
        R2[Run 4-layer heuristic]
        R3[Emit PASS/FAIL for each layer]
        R4[Store ReflectionResult]
        R5[If FAIL → trigger rewrite]
    end

    R1 --> R2 --> R3 --> R4 --> R5
```

**Key Implementation:**

- Reads ArticleContent from context

- 4-layer heuristic with status notifications:

- L1: Duplicate Words → ✅ PASS / ❌ FAIL

- L1B: Template Phrases → ✅ PASS / ❌ FAIL

- L2: Keyword Density → ✅ PASS / ❌ FAIL

- L3: Readability → ✅ PASS / ❌ FAIL

- L4: LLM Reflection → ✅ PASS / ❌ FAIL

- Stores SEOReflectionResult

- Triggers rewrite if needed

## 5.5 SavingHandler

```mermaid
flowchart LR
    subgraph Steps["Processing Steps"]
        SV1[Read ArticleContent]
        SV2[Read ReflectionResult]
        SV3[Calculate final score]
        SV4[Copy article to final directory]
        SV5[Generate SEOUrlResult]
    end

    SV1 --> SV3
    SV2 --> SV3
    SV3 --> SV4
    SV4 --> SV5
```
# 6. API Reference

## 6.1 Public Interface

```csharp
/// <summary>
/// Main orchestrator for SEO crawling and content generation pipeline
/// </summary>
public sealed class SEOCrawlSupervisorOrchestrator : IStreamingAgentOrchestrator
{
    /// <summary>
    /// Constructor with dependency injection
    /// </summary>
    public SEOCrawlSupervisorOrchestrator(
        AgentTeam team,
        IReflectionEngine? reflection = null,
        IPromptTemplateEngine? promptEngine = null,
        SEOCrawlSupervisorOptions? options = null,
        ILoggerFactory? loggerFactory = null,
        DynamicToolSynthesizer? toolSynthesizer = null,
        IDynamicToolRegistry? dynamicRegistry = null);

    /// <summary>
    /// Non-streaming execution - returns complete result set
    /// </summary>
    public IAsyncEnumerable<AgentStep> RunAsync(
        string userInput,
        AgentDefinition definition,
        IConversationMemory memory,
        IToolRegistry? tools,
        ISkillRegistry? skills,
        IReadOnlyList<IGuardrail> guardrails,
        CancellationToken ct = default);

    /// <summary>
    /// Streaming execution - returns real-time progress updates
    /// </summary>
    public IAsyncEnumerable<AgentStepUpdate> RunStreamingAsync(
        string userInput,
        AgentDefinition definition,
        IConversationMemory memory,
        IToolRegistry? tools,
        ISkillRegistry? skills,
        IReadOnlyList<IGuardrail> guardrails,
        CancellationToken ct = default);
}

# 7. Performance & Monitoring

```

## 7.1 Performance Metrics

| Metric | Target | Description |
| --- | --- | --- |
| URL Throughput | 5-10 URLs/min | URLs processed per minute |
| Per URL Latency | 2-5 minutes | Average time per URL |
| Success Rate | > 95% | Percentage of successful URLs |
| Reflection Accuracy | > 85% | Accuracy of quality detection |
| Memory Usage | < 2GB | Peak memory consumption |
| CPU Usage | < 80% | CPU utilization during batch |
## 7.2 Logging & Monitoring

| Level | Use Case | Example |
| --- | --- | --- |
| Debug | Detailed flow tracing | Worker invocation details |
| Information | Normal operation | Phase completion, URL processed |
| Warning | Recoverable issues | Retry attempts, timeout warnings |
| Error | Non-recoverable | Worker crash, file write failure |
# 8. Revision Summary

## 8.1 Changes from v1.4.0 to v1.5.0
| Component | v1.4.0 | v1.5.0 | Benefit |
| --- | --- | --- | --- |
| File Operations | LLM calls ReadFile/WriteFile tools | Handlers read/write files directly | Reduces LLM tool calls, faster, more reliable |
| Data Flow | LLM returns file paths only | LLM returns content directly, handler saves files | Eliminates file-reading round trips |
| Context Keys | Ad-hoc keys | Standardized keys (ScrapedContent, AnalysisContent, etc.) | Clear data flow, no duplication |
| Content Writing | LLM writes to file via tool | LLM returns Markdown, handler saves | More control, validates content before save |
| Reflection | Reads from file path | Reads from ArticleContent key | No disk I/O during reflection |
| Error Recovery | Hard to trace | Clear fallback with context | Better debugging |
## 8.2 Key Decisions

Why remove LLM file tool calls?

LLM often fails to call tools correctly (duplicate calls, wrong format)

File operations are deterministic – handlers can do it faster and more reliably

Reduces token usage (no tool call overhead)

Eliminates "file not found" errors

Why store content in context?

Avoids reading same file multiple times

Enables fallback if file is missing

Faster processing (no disk I/O for each handler)

Why use standardized keys?

Clear data flow visibility

Easier debugging

Prevents data collision between URLs

Why strict prompt for Writing?

Ensures consistent output format

Reduces post-processing needed

Guarantees meta title/description presence

# 9. Configuration Example

```json
{
  "SEOCrawlSupervisorOptions": {
    "EnableFinalReflection": true,
    "EnableAutoToolSynthesis": false,
    "SaveArticlesToFiles": true,
    "SaveFinalReport": true,
    "SkipL4IfHeuristicPass": true,
    "MaxUrls": 10,
    "BatchTotalTimeoutMinutes": 120,
    "MaxCorrectionRounds": 1,
    "WorkerTimeoutSeconds": 120,
    "LlmConfig": {
      "TokensPerSecond": 50,
      "SafetyBufferMultiplier": 1.5,
      "MinTimeoutSeconds": 30,
      "MaxTimeoutSeconds": 600,
      "CharsPerToken": 4.0,
      "EnableReasoning": true,
      "ReasoningTimePerToken": 0.02,
      "ReasoningBaseSeconds": 5.0,
      "ReasoningMultiplier": 1.8
    },
    "OutputDirectory": "./output",
    "HandlerTokenMultipliers": {
      "Scrape": 0.5,
      "Analyze": 1.0,
      "Write": 2.5,
      "Reflect": 1.8,
      "Save": 0.3
    }
  }
}
Document Version: 1.5.0
```
