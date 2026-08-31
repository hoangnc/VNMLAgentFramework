# SEOCrawlSupervisorOrchestrator — High-Level Design & Detail Design

## Document Revision History

| Version | Date | Author | Changes | Description |
|---|---|---|---|---|
| 1.0.0 | 2026-08-28 | System | Initial Release | First version with basic ReAct loop |
| 1.1.0 | 2026-08-29 | System | Architecture Refactor | Introduced Chain of Responsibility pattern with 6 handlers |
| 1.2.0 | 2026-08-30 | System | Dynamic Timeout | Added token-based timeout calculation for LLM operations |
| 1.3.0 | 2026-08-31 | System | Event-Driven Progress | Unified streaming/non-streaming via Progress events with emoji logging |
| 1.4.0 | 2026-08-31 | System | Reflection Engine | Extracted Reflection logic into dedicated engine with 4-layer heuristic |


# 1. Executive Summary

1.1 System Overview

The SEOCrawlSupervisorOrchestrator is a production-grade, pipeline-based orchestration system designed for automated SEO content processing. It manages end-to-end workflows including:

- Web crawling – extract content from target URLs
- SEO analysis – evaluate keyword density, readability, structure
- Content writing – generate SEO-optimized articles
- Quality reflection – multi-layer quality assurance with auto-correction
- Result persistence – save articles and generate comprehensive reports
1.2 Key Features

| Feature | Description | Status |
| --- | --- | --- |
| Pipeline Architecture | Chain of Responsibility with 6 handlers | ✅ Complete |
| Dynamic Timeout | Token-based timeout calculation per LLM | ✅ Complete |
| Event-Driven Progress | Real-time UI updates via Progress events | ✅ Complete |
| Vietnamese SEO | Custom heuristic for Vietnamese content | ✅ Complete |
| Auto Tool Synthesis | Dynamic tool generation for missing capabilities | ✅ Complete |
| 4-Layer Reflection | Duplicate words, templates, density, readability | ✅ Complete |
| Auto-Correction | Rewrite content when quality fails | ✅ Complete |
| Emoji Logging | Rich visual feedback with emojis | ✅ Complete |

1.3 Technology Stack

| Layer | Technology |
| --- | --- |
| Language | C# (.NET 10) |
| AI/LLM | LlamaCppSharp |
| Design Pattern | Chain of Responsibility, Pipeline, Observer |
| Logging | Serilog, Microsoft.Extensions.Logging |
| Data Format | JSON, Markdown |
| Concurrency | Async/Await, CancellationToken, Channels |


# 2. High-Level Design (HLD)

2.1 System Architecture


```mermaid
flowchart TD
    subgraph UI["User Interface"]
        A["User Input"]
        B["Real-time Progress"]
        C["Final Report"]
    end

    subgraph ORCH["SEOCrawlSupervisorOrchestrator"]
        D["URL Parser"]
        E["Pipeline Executor"]
        F["Event Subscriber"]
        G["Report Generator"]
    end

    subgraph PIPE["Pipeline - Chain of Responsibility"]
        H1["UrlValidationHandler"]
        H2["UrlQuantityValidationHandler"]
        H3["ScrapingHandler"]
        H4["AnalysisHandler"]
        H5["ContentWritingHandler"]
        H6["ReflectionHandler"]
        H7["SavingHandler"]
        H1 --> H2 --> H3 --> H4 --> H5 --> H6 --> H7
    end

    subgraph ENG["Core Engines"]
        E1["TimeoutCalculator"]
        E2["ReflectionEngine"]
        E3["LoggingFactory"]
    end

    subgraph WORK["Specialist Agents"]
        W1["Scraper Agent"]
        W2["SEO Analyzer"]
        W3["Content Writer"]
    end

    subgraph STORE["Data Storage"]
        S1[("JSON Files")]
        S2[("Markdown Files")]
        S3[("Reports")]
    end

    A --> D
    D --> E
    E --> H1

    H3 -.-> W1
    H4 -.-> W2
    H5 -.-> W3
    H6 -.-> E2

    E --> F
    F --> B
    E --> G
    G --> C

    E1 -.-> H1
    E3 -.-> H1

    W1 --> S1
    W2 --> S1
    W3 --> S2
    H7 --> S2
    G --> S3
```


```mermaid
flowchart LR
    subgraph PHASE["Execution Phases"]
        P1["1. Validation"]
        P2["2. Quantity Check"]
        P3["3. Scraping"]
        P4["4. Analysis"]
        P5["5. Writing"]
        P6["6. Reflection"]
        P7["7. Saving"]
        P1 --> P2 --> P3 --> P4 --> P5 --> P6 --> P7
    end

    subgraph PROGRESS["Real-time Progress"]
        EV1["Progress Event 1"]
        EV2["Progress Event 2"]
        EV3["Progress Event 3"]
        EV4["Progress Event 4"]
        EV5["Progress Event 5"]
        EV6["Progress Event 6"]
        EV7["Progress Event 7"]
    end

    P1 -.-> EV1
    P2 -.-> EV2
    P3 -.-> EV3
    P4 -.-> EV4
    P5 -.-> EV5
    P6 -.-> EV6
    P7 -.-> EV7

    EV1 --> UI["UI Render"]
    EV2 --> UI
    EV3 --> UI
    EV4 --> UI
    EV5 --> UI
    EV6 --> UI
    EV7 --> UI
```


```mermaid
classDiagram
    class IStreamingAgentOrchestrator {
        <<interface>>
        +RunAsync()
        +RunStreamingAsync()
    }

    class SEOCrawlSupervisorOrchestrator {
        -AgentTeam team
        -SEOCrawlSupervisorOptions options
        -UrlProcessingHandler pipelineHead
        -Dictionary reflectionCache
        -Dictionary scrapedEntityCache
        -HashSet attemptedSynthesis
        +BuildPipeline()
        +RunAsync()
        +RunStreamingAsync()
        -FormatProgressLog()
        -ExtractScore()
    }

    class UrlProcessingHandler {
        <<abstract>>
        #ILogger logger
        #UrlProcessingHandler next
        +Progress
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
    }

    class ReflectionHandler {
        -ReflectionEngine engine
        +HandleAsync()
        -PerformRewriteAsync()
        -OnEngineProgress()
    }

    class ReflectionEngine {
        -ILogger logger
        -SEOCrawlSupervisorOptions options
        +Progress
        +RunReflectionAsync()
        -CheckDuplicateWordsAsync()
        -CheckTemplatePhrasesAsync()
        -CheckKeywordDensityAsync()
        -CheckReadabilityAsync()
        -CheckLLMAsync()
    }

    class SavingHandler {
        +HandleAsync()
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

3.1 Chain of Responsibility Implementation


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
    S->>S: Extract Content Length
    S->>S: Precalculate Timeouts
    S-->>O: Progress Event
    S->>A: HandleAsync(context)
    activate A
    A->>A: Call Analyzer Worker
    A-->>O: Progress Event
    A->>W: HandleAsync(context)
    activate W
    W->>W: Call Writer Worker
    W-->>O: Progress Event
    W->>R: HandleAsync(context)
    activate R
    R->>R: Run Reflection Engine
    R->>R: Check 4 Layers
    R-->>O: Progress Event
    R->>SV: HandleAsync(context)
    activate SV
    SV->>SV: Save Article
    SV-->>O: Progress Event
    deactivate SV
    deactivate R
    deactivate W
    deactivate A
    deactivate S
    deactivate Q
    deactivate V
```


```mermaid
flowchart TD
    subgraph INPUT["Input Parameters"]
        CL["Content Length"]
        PL["Prompt Length"]
        RL["Response Length"]
    end

    subgraph CONFIG["Configuration"]
        TPS["Tokens Per Second"]
        SBM["Safety Buffer Multiplier"]
        MIN["Min Timeout"]
        MAX["Max Timeout"]
        CPT["Chars Per Token"]
        HM["Handler Multiplier"]
    end

    subgraph CALC["Calculation"]
        TE["Total Chars = PL + RL"]
        ET["Estimated Tokens = TE / CPT"]
        BT["Base Time = ET / TPS"]
        ST["Safe Time = BT * HM * SBM"]
        FT["Final Time = Clamp(ST, MIN, MAX)"]
    end

    PL --> TE
    RL --> TE
    TE --> ET
    CPT --> ET
    ET --> BT
    TPS --> BT
    BT --> ST
    HM --> ST
    SBM --> ST
    ST --> FT
    MIN --> FT
    MAX --> FT
    FT --> TO["Timeout"]

    CL -.-> TE
```


```mermaid
flowchart TD
    subgraph INPUT["Input"]
        C["Article Content"]
        U["Source URL"]
    end

    subgraph L1["Layer 1: Duplicate Words"]
        L1I["Split into words"]
        L1C["Count frequency"]
        L1D["Detect >3 occurrences"]
        L1R["Pass if none found"]
        L1I --> L1C --> L1D --> L1R
    end

    subgraph L1B["Layer 1B: Template Phrases"]
        L1BI["Detect common templates"]
        L1BC["Count occurrences"]
        L1BD["Detect >=2 occurrences"]
        L1BR["Pass if none found"]
        L1BI --> L1BC --> L1BD --> L1BR
    end

    subgraph L2["Layer 2: Keyword Density"]
        L2I["Extract keywords"]
        L2C["Calculate density"]
        L2D["Check 1.2%-3.5% range"]
        L2R["Pass if in range"]
        L2I --> L2C --> L2D --> L2R
    end

    subgraph L3["Layer 3: Readability"]
        L3I["Count sentences"]
        L3C["Average length"]
        L3D["Detect long sentences"]
        L3R["Pass if score >= 60"]
        L3I --> L3C --> L3D --> L3R
    end

    subgraph L4["Layer 4: LLM Reflection"]
        L4I["Build prompt"]
        L4C["Call LLM"]
        L4D["JSON output"]
        L4R["Pass if all good"]
        L4I --> L4C --> L4D --> L4R
    end

    subgraph OUTPUT["Output"]
        SUM["Summary Result"]
        ISS["Issues List"]
        SUG["Suggestions List"]
        SCR["Score Calculation"]
    end

    C --> L1I
    C --> L1BI
    C --> L2I
    C --> L3I
    C --> L4I
    U --> L4I

    L1R --> SUM
    L1BR --> SUM
    L2R --> SUM
    L3R --> SUM
    L4R --> SUM

    L1D --> ISS
    L1BD --> ISS
    L2D --> ISS
    L3D --> ISS
    L4D --> ISS

    ISS --> SUG
    SUG --> SCR
    SUM --> SCR
```


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
```


```mermaid
flowchart TD
    subgraph INPUT["Input Phase"]
        A["User Input"] --> B["URL Parser"]
        B --> C["URL List"]
    end

    subgraph CONTEXT["Context Initialization"]
        C --> D["Create UrlProcessingContext"]
        D --> E["Initialize Dictionaries"]
        D --> F["Set Options"]
        D --> G["Set Cancellation Token"]
    end

    subgraph PIPE["Pipeline Processing"]
        E --> H1["Validation"]
        H1 -->|Pass| H2["Quantity Check"]
        H2 -->|Pass| H3["Scraping"]
        H3 -->|Content| H4["Analysis"]
        H4 -->|Report| H5["Writing"]
        H5 -->|Article| H6["Reflection"]
        H6 -->|Quality| H7["Saving"]
    end

    subgraph OUTPUT["Output Phase"]
        H7 --> I1["WorkerOutputs"]
        H7 --> I2["PhaseTimings"]
        H7 --> I3["SEOUrlResult"]
        I1 --> J1["JSON Data"]
        I2 --> J2["Markdown Report"]
        I3 --> J3["Final Result"]
    end

    subgraph PROGRESS["Real-time Progress"]
        H1 -.-> K1["Progress Event"]
        H2 -.-> K2["Progress Event"]
        H3 -.-> K3["Progress Event"]
        H4 -.-> K4["Progress Event"]
        H5 -.-> K5["Progress Event"]
        H6 -.-> K6["Progress Event"]
        H7 -.-> K7["Progress Event"]
        K1 --> L["Channel"]
        K2 --> L
        K3 --> L
        K4 --> L
        K5 --> L
        K6 --> L
        K7 --> L
        L --> M["UI Render"]
    end
```


```mermaid
flowchart TD
    subgraph SOURCES["Score Sources"]
        A["Reflection Result"]
        B["Analysis Output"]
        C["Content Length"]
        D["Writer Output"]
    end

    subgraph EXTRACT["Extract Score"]
        A --> E1["ExtractScoreFromReflection"]
        B --> E2["ExtractScoreFromAnalysis"]
        C --> E3["ExtractScoreFromContentLength"]
        D --> E4["ExtractScoreFromWriter"]
    end

    subgraph WEIGHTS["Weighting"]
        E1 --> W1["Weight: 0.4"]
        E2 --> W2["Weight: 0.3"]
        E3 --> W3["Weight: 0.1"]
        E4 --> W4["Weight: 0.2"]
    end

    subgraph COMBINE["Combine"]
        W1 --> C1["Weighted Average"]
        W2 --> C1
        W3 --> C1
        W4 --> C1
        C1 --> F["Final Score"]
    end

    subgraph THRESHOLD["Threshold"]
        F --> T{"Score >= 70?"}
        T -->|Yes| P["PASS - OK"]
        T -->|No| F2["FAIL"]
    end
```


# 4. Component Details

4.1 SEOCrawlSupervisorOptions


```csharp
public class SEOCrawlSupervisorOptions
{
    // === Core Settings ===
    public bool EnableFinalReflection { get; set; } = true;
    public bool EnableAutoToolSynthesis { get; set; } = false;
    public bool SaveArticlesToFiles { get; set; } = true;
    public bool SaveFinalReport { get; set; } = true;
    public bool SkipL4IfHeuristicPass { get; set; } = true;
    
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
    public int? MaxUrls { get; set; } = 10;
    
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
    public double TokensPerSecond { get; set; } = 50;
    public double SafetyBufferMultiplier { get; set; } = 1.5;
    public int MinTimeoutSeconds { get; set; } = 30;
    public int MaxTimeoutSeconds { get; set; } = 900;
    public double CharsPerToken { get; set; } = 4.0;
}
4.2 ProgressEventArgs
```


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
4.3 ReflectionProgressEventArgs
```


```csharp
public class ReflectionProgressEventArgs : EventArgs
{
    public string LayerName { get; set; } = string.Empty;
    public int LayerIndex { get; set; }
    public int TotalLayers { get; set; }
    public bool Passed { get; set; }
    public string Detail { get; set; } = string.Empty;
    public double ElapsedMs { get; set; }
    public bool IsComplete { get; set; }
    public bool AllPassed { get; set; }
    public List<string> Issues { get; set; } = new();
    public List<string> Suggestions { get; set; } = new();
}
```


# 5. API Reference

5.1 Public Interface


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
```


# 6. Deployment & Configuration

6.1 Environment Variables

| Variable | Description | Default |
| --- | --- | --- |
| SEO_OUTPUT_DIR | Output directory path | ./output |
| SEO_BATCH_TIMEOUT | Total batch timeout (minutes) | 120 |
| SEO_WORKER_TIMEOUT | Worker timeout (seconds) | 120 |
| SEO_BASE_TIMEOUT | Base timeout per handler (seconds) | 60 |
| SEO_MAX_TIMEOUT | Maximum timeout (seconds) | 600 |
| SEO_AUTO_SYNTHESIS | Enable auto tool synthesis | false |
| SEO_ENABLE_REFLECTION | Enable final reflection | true |
| SEO_TOKENS_PER_SECOND | LLM tokens per second | 50 |

6.2 Configuration Example


```json
{
  "SEOCrawlSupervisorOptions": {
    "EnableFinalReflection": true,
    "EnableAutoToolSynthesis": false,
    "SaveArticlesToFiles": true,
    "SaveFinalReport": true,
    "SkipL4IfHeuristicPass": true,
    "BatchTotalTimeoutMinutes": 120,
    "MaxCorrectionRounds": 1,
    "WorkerTimeoutSeconds": 120,
    "LlmConfig": {
      "TokensPerSecond": 50,
      "SafetyBufferMultiplier": 1.5,
      "MinTimeoutSeconds": 30,
      "MaxTimeoutSeconds": 600,
      "CharsPerToken": 4.0
    },
    "OutputDirectory": "./output",
    "MaxUrls": 10,
    "HandlerTokenMultipliers": {
      "Scrape": 0.5,
      "Analyze": 1.0,
      "Write": 2.5,
      "Reflect": 1.8,
      "Save": 0.3
    }
  }
}
```


# 7. Performance & Monitoring

7.1 Performance Metrics

| Metric | Target | Description |
| --- | --- | --- |
| URL Throughput | 5-10 URLs/min | URLs processed per minute |
| Per URL Latency | 2-5 minutes | Average time per URL |
| Success Rate | > 95% | Percentage of successful URLs |
| Reflection Accuracy | > 85% | Accuracy of quality detection |
| Memory Usage | < 2GB | Peak memory consumption |
| CPU Usage | < 80% | CPU utilization during batch |

7.2 Logging Levels

| Level | Use Case | Example |
| --- | --- | --- |
| Debug | Detailed flow tracing | Worker invocation details |
| Information | Normal operation | Phase completion, URL processed |
| Warning | Recoverable issues | Retry attempts, timeout warnings |
| Error | Non-recoverable | Worker crash, file write failure |


# 8. Revision Summary

8.1 Changes from v1.0.0 to v1.4.0

| Component | v1.0.0 | v1.4.0 | Benefit |
| --- | --- | --- | --- |
| Architecture | Single monolithic loop | Chain of Responsibility with 7 handlers | Separation of concerns, easier maintenance |
| Progress | Manual yield updates | Event-driven with Channels | Unified streaming, better UI experience |
| Timeout | Fixed values | Dynamic token-based calculation | Adaptive, prevents premature timeout |
| Reflection | Embedded in orchestrator | Dedicated ReflectionEngine with 4 layers | Reusable, testable, extensible |
| Logging | Plain text | Emoji-enhanced with timestamps | Better UX, instant status recognition |
| Scoring | Basic (PASS/FAIL) | Weighted multi-source scoring | More accurate quality measurement |
| Cache | None | Dictionary-based result caching | Avoids redundant processing |
| Auto-Correction | None | Rewrite with worker synthesis | Self-healing, improved quality |

8.2 Key Decisions

Why Chain of Responsibility?

- Each handler has single responsibility
- Easy to add/remove/reorder phases
- Natural fit for pipeline processing
Why Event-Driven Progress?

- Unifies streaming and non-streaming
- Decouples logic from presentation
- Enables multiple subscribers (UI, logging, metrics)
Why Token-Based Timeout?

- LLM processing time is proportional to tokens
- Adaptive to content length and model speed
- Prevents both premature and excessive timeouts
Why 4-Layer Reflection?

- Covers all SEO quality dimensions
- Layer 1-3 are fast heuristics (no LLM)
- Layer 4 is optional LLM-based validation
- SkipL4IfHeuristicPass saves cost

# 9. Appendix

9.1 Vietnamese Stopwords


```csharp
public static readonly HashSet<string> VietnameseStopWords = new()
{
    "và", "của", "là", "để", "thì", "mà", "cho", "trong", "các", "những",
    "một", "có", "được", "đã", "đang", "sẽ", "từ", "đến", "với", "tại",
    // ... full list in source code
};
9.2 Template Phrases Detection
```


```csharp
public static readonly List<string> TemplatePhrases = new()
{
    "ngoài ra bạn cũng nên quan tâm đến",
    "bạn có biết",
    "trong bài viết này chúng tôi sẽ",
    // ... full list in source code
};
```
---

**Document Version:** 1.4.0  

