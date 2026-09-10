# SEOCrawlSupervisorOrchestrator - High-Level Design & Detail Design
## Document Revision History
| Version | Date | Author | Changes | Description |
| --- | --- | --- | --- | --- |
| 1.0.0| 2026-08-28 | Hoang Nguyen | Initial Release | First version with basic ReAct loop  |
|1.1.0 |2026-08-29	| Hoang Nguyen	|Architecture Refactor|	Introduced Chain of Responsibility pattern with 6 handlers|
|1.2.0 |2026-08-30|	Hoang Nguyen	|Dynamic Timeout|	Added token-based timeout calculation for LLM operations|
| 1.3.0 |2026-08-31	| Hoang Nguyen	|Event-Driven Progress|	Unified streaming/non-streaming via Progress events with emoji logging|
| 1.4.0| 2026-08-31| Hoang Nguyen|	Reflection Engine	|Extracted Reflection logic into dedicated engine with 4-layer heuristic|
|1.5.0 |2026-09-04|	Hoang Nguyen|	File-Based Processing|	BREAKING: Removed LLM tool calls for file operations; handlers read/write files directly|
|1.5.1 |2026-09-07	|Hoang Nguyen|	Memory Isolation|	Fixed memory leak between URLs; added ResetMemory() to all agents|
|1.5.2| 2026-09-08|	Hoang Nguyen|	Adaptive Timeout|	Nonlinear timeout factor + adaptive max scaling + LLM config presets|
|1.6.0 |2026-09-10|	Hoang Nguyen	|Response Cleaner & Chunking	|Added LlmResponseCleaner, content chunking for large inputs, and QualityValidationHandler|

# 1. Executive Summary
## 1.1 System Overview

**SEOCrawlSupervisorOrchestrator** là hệ thống orchestration pipeline-based cho tự động hóa xử lý nội dung SEO. Hệ thống quản lý workflow end-to-end bao gồm:

- **Web crawling** – trích xuất nội dung từ URL mục tiêu

- **SEO analysis** – đánh giá keyword density, readability, structure

- **Content writing** – sinh bài viết tối ưu SEO

- **Quality reflection** – đảm bảo chất lượng đa tầng với tự động sửa lỗi

- **Quality validation** – xác thực chất lượng tổng thể với báo cáo JSON

- **Result persistence** – lưu bài viết và sinh báo cáo tổng hợp

## 1.2 Key Features (Updated v1.6.0)

| Feature | Description | Status |
| --- | --- | --- |
| Pipeline Architecture | Chain of Responsibility với 8 handlers | ✅ Complete |
| File-Based Processing | Handlers đọc/ghi file trực tiếp, LLM không dùng file tools | ✅ Complete |
| Memory Isolation | Reset memory trước mỗi URL, không leak context | ✅ v1.5.1 |
| Adaptive Dynamic Timeout | Timeout scale theo content length + nonlinear factor | ✅ v1.5.2 |
| LLM Response Cleaner | Xử lý markdown fence, JSON, file path tự động | ✅ v1.6.0 |
| Content Chunking | Chia nhỏ content > threshold, merge kết quả | ✅ v1.6.0 |
| Event-Driven Progress | Real-time UI updates với emoji logging | ✅ Complete |
| Vietnamese SEO | Custom heuristic cho tiếng Việt | ✅ Complete |
| Auto Tool Synthesis | Sinh tool động khi thiếu capability | ✅ Complete |
| 4-Layer Reflection | Duplicate words, templates, density, readability | ✅ Complete |
| Quality Validation | Báo cáo JSON với overall_score | ✅ v1.6.0 |
| Auto-Correction | Rewrite nội dung khi chất lượng không đạt | ✅ Complete |## 1.3 Technology Stack

| Layer | Technology |
| --- | --- |
| Language | C# (.NET 10) |
| AI/LLM | LlamaCppSharp |
| Design Pattern | Chain of Responsibility, Pipeline, Observer |
| Logging | Serilog, Microsoft.Extensions.Logging |
| Data Format | JSON, Markdown |
| Concurrency | Async/Await, CancellationToken, Channels |2. High-Level Design (HLD)
## 2.1 System Architecture (v1.6.0)

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
        H[Memory Reset Manager]
    end

    subgraph Pipeline["Pipeline (Chain of Responsibility)"]
        H1[UrlValidationHandler]
        H2[UrlQuantityValidationHandler]
        H3[ScrapingHandler]
        H4[AnalysisHandler]
        H5[ContentWritingHandler]
        H6[ReflectionHandler]
        H7[QualityValidationHandler]
        H8[SavingHandler]
    end

    subgraph Engines["Core Engines"]
        E1[AdvancedTimeoutCalculator]
        E2[ReflectionEngine]
        E3[LoggingFactory]
        E4[LlmResponseCleaner]
    end

    subgraph Storage["Data Storage"]
        S1[(Scraped Files)]
        S2[(Analysis Files)]
        S3[(Articles)]
        S4[(Reports)]
    end

    A --> D
    D --> E
    E --> H
    E --> Pipeline
    H1 --> H2 --> H3 --> H4 --> H5 --> H6 --> H7 --> H8
    H3 -.-> E1
    H4 -.-> E1
    H4 -.-> E4
    H7 -.-> E4
    H6 -.-> E2
    E --> F
    F --> B
    E --> G
    G --> C
    H3 --> S1
    H4 --> S2
    H5 --> S3
    H8 --> S4
```
## 2.2 Pipeline Flow (v1.6.0)

```mermaid
flowchart LR
    subgraph Reset["Memory Reset"]
        M0[Reset All Agent Memories]
    end

    subgraph Phases["Execution Phases"]
        P1[1. Validation]
        P2[2. Quantity Check]
        P3[3. Scraping]
        P4[4. Analysis + Chunking]
        P5[5. Writing]
        P6[6. Reflection]
        P7[7. Quality Validation]
        P8[8. Saving]
    end

    M0 --> P1 --> P2 --> P3 --> P4 --> P5 --> P6 --> P7 --> P8
```
## 2.3 Data Flow – File-Based Processing với Chunking (v1.6.0)

```mermaid
flowchart TD
    subgraph Input["Input"]
        URL[User Input]
    end

    subgraph Handlers["Pipeline Handlers"]
        H3[3. Scraping]
        H4[4. Analysis]
        H4C[4b. Chunking]
        H4M[4c. Merge Chunks]
        H5[5. Writing]
        H6[6. Reflection]
        H7[7. Quality Validation]
        H8[8. Saving]
    end

    subgraph Data["Data Flow - WorkerOutputs"]
        D1[ScrapedFilePath]
        D2[ScrapedContent]
        D3[AnalysisFilePath]
        D4[AnalysisContent]
        D5[PrimaryKeyword]
        D6[ArticleFilePath]
        D7[ArticleContent]
        D8[ReflectionResult]
        D9[ValidationResult]
    end

    H3 -->|writes| D1
    H3 -->|extracts| D2
    H4 -->|reads| D2
    H4 -->|if content > threshold| H4C
    H4C -->|per chunk| H4M
    H4M -->|writes| D3
    H4M -->|extracts| D4
    H4M -->|extracts| D5
    H5 -->|reads| D2
    H5 -->|reads| D4
    H5 -->|reads| D5
    H5 -->|writes| D6
    H5 -->|extracts| D7
    H6 -->|reads| D7
    H6 -->|writes| D8
    H7 -->|reads| D7
    H7 -->|reads| D8
    H7 -->|writes| D9
    H8 -->|reads| D6
    H8 -->|reads| D9
    H8 -->|generates| Final[SEOUrlResult]
```
## 2.4 Component Overview (v1.6.0)

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
        -ResetAllMemories()
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
        #EstimateResponseLength()
    }

    class ScrapingHandler {
        +HandleAsync()
        -TryFindRecentScrapedFile()
    }

    class AnalysisHandler {
        +HandleAsync()
        -AnalyzeSingleAsync()
        -AnalyzeWithChunkingAsync()
        -SplitIntoChunks()
        -MergeChunkResults()
        -SaveAnalysisResultAsync()
    }

    class ContentWritingHandler {
        +HandleAsync()
    }

    class ReflectionHandler {
        -ReflectionEngine _engine
        +HandleAsync()
        -PerformRewriteAsync()
    }

    class QualityValidationHandler {
        +HandleAsync()
        -BuildValidatorPrompt()
        -BuildReportFromContext()
    }

    class SavingHandler {
        +HandleAsync()
        -SaveArticleWithMetadata()
    }

    class AdvancedTimeoutCalculator {
        <<static>>
        +CalculateTimeout()
        +CalculateByContentLength()
        +EstimateTokens()
        -ComputeNonlinearFactor()
        -ComputeAdaptiveMax()
    }

    class LlmResponseCleaner {
        <<static>>
        +CleanJson()
        +CleanFilePath()
        +CleanMarkdown()
        +StripMarkdownFence()
        +IsValidJson()
    }

    class LlmConfig {
        +double TokensPerSecond
        +double CharsPerToken
        +int MaxTimeoutSeconds
        +bool EnableAdaptiveMax
        +int ChunkingThresholdChars
        +ForCpu()
        +ForGpuConsumer()
        +ForGpuServer()
        +ForCloudApi()
    }

    IStreamingAgentOrchestrator <|.. SEOCrawlSupervisorOrchestrator
    SEOCrawlSupervisorOrchestrator --> UrlProcessingHandler
    UrlProcessingHandler <|-- ScrapingHandler
    UrlProcessingHandler <|-- AnalysisHandler
    UrlProcessingHandler <|-- ContentWritingHandler
    UrlProcessingHandler <|-- ReflectionHandler
    UrlProcessingHandler <|-- QualityValidationHandler
    UrlProcessingHandler <|-- SavingHandler
    ScrapingHandler --> LlmResponseCleaner
    AnalysisHandler --> AdvancedTimeoutCalculator
    AnalysisHandler --> LlmResponseCleaner
    QualityValidationHandler --> LlmResponseCleaner
    UrlProcessingHandler --> AdvancedTimeoutCalculator
```
# 3. Detailed Design (DD)
## 3.1 Memory Isolation Flow (v1.5.1)

```mermaid
sequenceDiagram
    participant O as Orchestrator
    participant W1 as Scraper Agent
    participant W2 as Analyzer Agent
    participant W3 as Writer Agent
    participant M as Messenger

    Note over O: URL 1 Processing
    O->>W1: ExecuteTask (URL 1)
    W1->>W1: Memory: [user, assistant, tool]
    O->>W2: ExecuteTask (URL 1)
    W2->>W2: Memory: [user, assistant, tool]

    Note over O: URL 2 - RESET MEMORY
    O->>W1: ResetMemory()
    W1->>W1: Memory: [] (cleared)
    O->>W2: ResetMemory()
    W2->>W2: Memory: [] (cleared)
    O->>M: Clear()
    M->>M: Queue: [] (cleared)

    Note over O: URL 2 Processing
    O->>W1: ExecuteTask (URL 2)
    W1->>W1: Memory: [user, assistant] (fresh)
```
## 3.2 Adaptive Dynamic Timeout (v1.5.2)

```mermaid
flowchart TD
    subgraph Input["Input Parameters"]
        CL[Content Length]
        PL[Prompt Length]
        RL[Response Length]
        HM[Handler Multiplier]
    end

    subgraph Estimate["Token Estimation"]
        TE[Total Chars = PL + RL]
        ET[Estimated Tokens = TE / CharsPerToken]
    end

    subgraph Base["Base Time"]
        BT[Base Time = Tokens / TokensPerSecond]
    end

    subgraph Nonlinear["Nonlinear Factor"]
        NF{Tokens > 2000?}
        NF1[≤ 8000: 1 + k*0.15]
        NF2[> 8000: 1.9 + k*0.25]
        NFC[Cap 5.0x]
    end

    subgraph Adaptive["Adaptive Max"]
        AM[Max = BaseMax + content/1000 * 300]
    end

    subgraph Thinking["Thinking Mode"]
        TM[+ tokens * 0.02s + 5 * 1.8s]
    end

    subgraph Final["Final Timeout"]
        FT[Clamp min, max]
    end

    CL --> TE
    PL --> TE
    RL --> TE
    TE --> ET
    ET --> BT
    BT --> NF
    NF -->|No| NF1
    NF -->|Yes| NF2
    NF1 --> NFC
    NF2 --> NFC
    NFC --> AM
    AM --> TM
    TM --> FT
```

## Bảng tính timeout cho các content size khác nhau (ForCpu preset):

| Content | Tokens | Base | Nonlinear | Required | Adaptive Max | Kết quả |
| --- | --- | --- | --- | --- | --- | --- |
| 2,579 | 2,316 | 193s | 1.05x | 304s | 2,760s | ✅ OK |
| 8,070 | 6,892 | 574s | 1.73x | 1,490s | 4,665s | ✅ OK |
| 12,000 | 10,167 | 847s | 2.44x | 3,101s | 6,120s | ✅ OK |
| 20,000 | 16,833 | 1,403s | 4.11x | 8,648s | 9,000s | ✅ Chunking |
| 30,000 | 25,167 | 2,097s | 5.00x | 15,728s | 12,600s | ⚠️ Chunking bắt buộc |## 3.3 LLM Response Cleaner (v1.6.0)

```mermaid
flowchart TD
    Start[Raw LLM Response] --> CheckType{Response Type?}
    CheckType -->|JSON expected| CleanJson[CleanJson]
    CheckType -->|File path expected| CleanPath[CleanFilePath]
    CheckType -->|Markdown expected| CleanMd[CleanMarkdown]

    CleanJson --> Strip1[Strip Markdown Fence]
    Strip1 --> Extract1[Extract braces]
    Extract1 --> Validate1{Valid JSON?}
    Validate1 -->|Yes| Return1[Return JSON]
    Validate1 -->|No| Return1b[Return empty]

    CleanPath --> Strip2[Strip Markdown Fence]
    Strip2 --> RemovePrefix[Remove 'Path:', 'File:']
    RemovePrefix --> FirstLine[Take first line]
    FirstLine --> TrimQuotes[Trim quotes and backticks]
    TrimQuotes --> Return2[Return clean path]

    CleanMd --> Strip3[Strip ```markdown fence]
    Strip3 --> Return3[Return markdown]
```
## 3.4 Content Chunking Flow (v1.6.0)

```mermaid
flowchart TD
    Start[Scraped Content] --> Check{Length > Threshold?}
    Check -->|No| Single[Single Analysis Call]
    Check -->|Yes| Split[SplitIntoChunks]

    Split --> Loop[For each chunk]
    Loop --> Prompt[Build Chunk Prompt]
    Prompt --> Call[Call LLM với chunk timeout]
    Call --> Clean[CleanJson response]
    Clean --> Collect[Collect valid chunks]

    Collect --> Merge[MergeChunkResults]
    Merge --> Keywords[Merge top_keywords]
    Merge --> Suggestions[Merge suggestions]
    Merge --> Scores[Average scores]
    Merge --> Save[Save merged JSON]

    Single --> Save
    Save --> End[End]
```
## 3.5 Pipeline Execution Sequence (v1.6.0)

```mermaid
sequenceDiagram
    participant User
    participant Orchestrator
    participant Reset as Memory Reset
    participant Pipeline
    participant UI

    User->>Orchestrator: Submit URLs
    Orchestrator->>Orchestrator: Parse URLs

    loop Each URL
        Orchestrator->>Reset: ResetMemory() tất cả agents
        Reset->>Reset: Clear messenger queue
        Reset-->>Orchestrator: Done

        Orchestrator->>Pipeline: HandleAsync(context)
        Pipeline->>Pipeline: Validation
        Pipeline-->>UI: Progress Event
        Pipeline->>Pipeline: Scraping + CleanFilePath
        Pipeline-->>UI: Progress Event
        Pipeline->>Pipeline: Analysis (single/chunked)
        Pipeline-->>UI: Progress Event
        Pipeline->>Pipeline: Writing
        Pipeline-->>UI: Progress Event
        Pipeline->>Pipeline: Reflection
        Pipeline-->>UI: Progress Event
        Pipeline->>Pipeline: Quality Validation
        Pipeline-->>UI: Progress Event
        Pipeline->>Pipeline: Saving
        Pipeline-->>UI: Progress Event
        Pipeline-->>Orchestrator: SEOUrlResult
    end

    Orchestrator->>UI: Final Report
```
# 4. Component Details (v1.6.0)
## 4.1 UrlProcessingContext

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
    public CancellationToken CancellationToken { get; set; }
    public ILogger Logger { get; set; } = null!;
    public bool EnableReasoning { get; set; } = false;
}
```

## 4.2 Standardized WorkerOutputs Keys (v1.6.0)

| Key | Type | Description | Written By | Read By |
| --- | --- | --- | --- | --- |
| ScrapedFilePath | string | Path to scraped JSON file | ScrapingHandler | (reference) |
| ScrapedContent | string | Raw JSON content of scraped file | ScrapingHandler | AnalysisHandler, ContentWritingHandler, ReflectionHandler |
| AnalysisFilePath | string | Path to analysis JSON file | AnalysisHandler | (reference) |
| AnalysisContent | string | Analysis JSON content | AnalysisHandler | ContentWritingHandler, QualityValidationHandler, SavingHandler |
| AnalysisScore | string | SEO score from analysis | AnalysisHandler | SavingHandler |
| PrimaryKeyword | string | Primary keyword from analysis | AnalysisHandler | ContentWritingHandler |
| ArticleFilePath | string | Path to article MD file | ContentWritingHandler | SavingHandler |
| ArticleContent | string | Markdown article content | ContentWritingHandler | ReflectionHandler, QualityValidationHandler, SavingHandler |
| ValidationResult | string | Quality validation JSON | QualityValidationHandler | SavingHandler |
| ValidationScore | string | Quality score | QualityValidationHandler | SavingHandler |
| ValidationStatus | string | APPROVED / NEEDS_REVISION | QualityValidationHandler | SavingHandler |## 4.3 LlmConfig Presets (v1.6.0)

```csharp
public static LlmConfig ForCpu() => new()
{
    TokensPerSecond = 12,           // CPU chậm
    CharsPerToken = 3.0,            // JSON + tiếng Việt
    SafetyBufferMultiplier = 1.8,
    MinTimeoutSeconds = 45,
    MaxTimeoutSeconds = 1800,
    EnableAdaptiveMax = true,
    AdaptiveMaxSecondsPerKChars = 360,
    ChunkingThresholdChars = 10000,  // Chunking sớm cho CPU
    ChunkSizeChars = 4500,
    ChunkOverlapChars = 200
};

public static LlmConfig ForGpuConsumer() => new()
{
    TokensPerSecond = 40,           // RTX 3060/4060
    CharsPerToken = 3.0,
    SafetyBufferMultiplier = 1.5,
    MinTimeoutSeconds = 30,
    MaxTimeoutSeconds = 1200,
    EnableAdaptiveMax = true,
    AdaptiveMaxSecondsPerKChars = 240,
    ChunkingThresholdChars = 15000,
    ChunkSizeChars = 6000,
    ChunkOverlapChars = 200
};

public static LlmConfig ForGpuServer() => new()
{
    TokensPerSecond = 90,           // A100/H100
    CharsPerToken = 3.2,
    SafetyBufferMultiplier = 1.3,
    MinTimeoutSeconds = 20,
    MaxTimeoutSeconds = 600,
    EnableAdaptiveMax = true,
    AdaptiveMaxSecondsPerKChars = 120,
    ChunkingThresholdChars = 30000,
    ChunkSizeChars = 12000,
    ChunkOverlapChars = 300
};

public static LlmConfig ForCloudApi() => new()
{
    TokensPerSecond = 100,          // OpenAI/Claude
    CharsPerToken = 3.5,
    SafetyBufferMultiplier = 1.5,
    MinTimeoutSeconds = 30,
    MaxTimeoutSeconds = 300,
    EnableAdaptiveMax = false,       // API nhanh, không cần
    ChunkingThresholdChars = 50000,
    ChunkSizeChars = 20000,
    ChunkOverlapChars = 500
};
```

## 4.4 QualityValidationHandler

```mermaid
flowchart TD
    Start[HandleAsync] --> Read[Read ArticleContent + ScrapedContent + AnalysisContent]
    Read --> FindWorker[Find QualityValidator Worker]
    FindWorker --> HasWorker{Worker Exists?}


    HasWorker -->|No| Fallback[BuildReportFromContext]
    HasWorker -->|Yes| Prompt[BuildValidatorPrompt]
    Prompt --> Call[Call LLM]
    Call --> Clean[LlmResponseCleaner.CleanJson]
    Clean --> Valid{Valid JSON?}
    Valid -->|No| Fallback
    Valid -->|Yes| Save[Save to WorkerOutputs]

    Fallback --> Save
    Save --> End[End]
```
# 5. Error Handling & Recovery (v1.6.0)
## 5.1 Error Handling Strategy

| Error Type | Recovery Strategy | Handler |
| --- | --- | --- |
| Markdown fence trong JSON response | LlmResponseCleaner.CleanJson | Analysis, QualityValidation |
| Markdown fence trong file path | LlmResponseCleaner.CleanFilePath + fallback tìm file | Scraping |
| Memory leak giữa URLs | ResetMemory() trước mỗi URL | Orchestrator |
| Timeout cho content lớn | Adaptive max + chunking | Analysis |
| Duplicate tool calls | HashSet<string> executedToolSignatures | Orchestrator |
| Reflection rewrite treo | Timeout động multiplier 3.0 | ReflectionHandler |## 5.2 Retry Policy

| Component | Max Retries | Backoff | Timeout |
| --- | --- | --- | --- |
| Scraping | 3 | 2s (exponential) | Dynamic |
| Analysis | 2 | 2s | Dynamic |
| Writing | 2 | 3s | Dynamic |
| Reflection | 1 | 2s | Dynamic |
| Quality Validation | 1 | 2s | Dynamic |
| Saving | 1 | 1s | Dynamic |6. Performance & Scalability (v1.6.0)
## 6.1 Performance Metrics

| Metric | Target | Current (v1.6.0) |
| --- | --- | --- |
| URL Throughput | 5-10 URLs/min | ~4 URLs/min (CPU) |
| Per URL Latency | 2-5 minutes | ~15 min (content lớn) |
| Success Rate | > 95% | 96% |
| Memory Leak | 0 | ✅ Fixed |
| Timeout Errors | < 1% | ✅ Fixed với adaptive |
| Chunking Support | Content > 30K chars | ✅ Supported |## 6.2 Scalability Considerations

```mermaid
flowchart TD
    subgraph Scalability["Scalability Features"]
        F1[Memory Isolation]
        F2[Adaptive Timeout]
        F3[Content Chunking]
        F4[LLM Config Presets]
        F5[Response Cleaning]
    end
    subgraph Benefits["Benefits"]
        B1[Không leak giữa URLs]
        B2[Timeout phù hợp mọi content size]
        B3[Xử lý content > 30K chars]
        B4[Tối ưu cho từng loại máy]
        B5[Robust với mọi LLM output]
    end

    F1 --> B1
    F2 --> B2
    F3 --> B3
    F4 --> B4
    F5 --> B5
```
# 7. Configuration (v1.6.0)
## 7.1 Recommended Configuration

```json
{
  "SEOCrawlSupervisorOptions": {
    "EnableFinalReflection": true,
    "EnableAutoToolSynthesis": false,
    "SaveArticlesToFiles": true,
    "SaveFinalReport": true,
    "SkipL4IfHeuristicPass": true,
    "MaxUrls": 5,
    "BatchTotalTimeoutMinutes": 240,
    "MaxCorrectionRounds": 1,
    "WorkerTimeoutSeconds": 2400,
    "LlmConfig": {
      "TokensPerSecond": 12,
      "CharsPerToken": 3.0,
      "SafetyBufferMultiplier": 1.8,
      "MinTimeoutSeconds": 45,
      "MaxTimeoutSeconds": 1800,
      "EnableAdaptiveMax": true,
      "AdaptiveMaxSecondsPerKChars": 360,
      "ChunkingThresholdChars": 10000,
      "ChunkSizeChars": 4500,
      "ChunkOverlapChars": 200,
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
      "Rewrite": 2.5,
      "Reflection": 1.8,
      "Validate": 1.5,
      "Save": 0.3
    }
  }
}
```
# 8. API Reference
## 8.1 Public Interface

```csharp
/// <summary>
/// Main orchestrator cho SEO crawling và content generation pipeline
/// </summary>
public sealed class SEOCrawlSupervisorOrchestrator : IStreamingAgentOrchestrator
{
    public SEOCrawlSupervisorOrchestrator(
        AgentTeam team,
        IReflectionEngine? reflection = null,
        IPromptTemplateEngine? promptEngine = null,
        SEOCrawlSupervisorOptions? options = null,
        ILoggerFactory? loggerFactory = null,
        DynamicToolSynthesizer? toolSynthesizer = null,
        IDynamicToolRegistry? dynamicRegistry = null);

    public IAsyncEnumerable<AgentStep> RunAsync(...);
    public IAsyncEnumerable<AgentStepUpdate> RunStreamingAsync(...);
}
```

## 8.2 Helper Classes

```csharp
// LlmResponseCleaner - Xử lý mọi loại LLM response
public static class LlmResponseCleaner
{
    public static string CleanJson(string raw);
    public static string CleanFilePath(string raw);
    public static string CleanMarkdown(string raw);
    public static string StripMarkdownFence(string text);
    public static bool IsValidJson(string text);
}

// AdvancedTimeoutCalculator - Tính timeout động
public static class AdvancedTimeoutCalculator
{
    public static TimeSpan CalculateTimeout(...);
    public static TimeSpan CalculateByContentLength(...);
    public static int EstimateTokens(string content, double charsPerToken = 3.0);
    public static int EstimateTokensFromLength(int length, double charsPerToken = 3.0);
}
```
# 9. Revision Summary
## 9.1 Changes from v1.4.0 → v1.6.0

| Component | v1.4.0 | v1.6.0 | Benefit |
| --- | --- | --- | --- |
| File Operations | LLM calls ReadFile/WriteFile | Handlers read/write directly | Nhanh hơn 40% |
| Memory | Không reset giữa URLs | ResetMemory() trước mỗi URL | Không leak context |
| Timeout | Fixed 60s | Adaptive với nonlinear factor | Timeout phù hợp |
| Max Timeout | 600s | 1800s + adaptive scaling | Content lớn không bị cắt |
| Chunking | Không có | Content > 10K chars → chunks | Xử lý content > 30K |
| Response Cleaning | Manual | LlmResponseCleaner tự động | Robust với mọi LLM output |
| Quality Validation | Không có | QualityValidationHandler | Báo cáo JSON chuẩn |
| Config Presets | 1 config | 4 presets (CPU/GPU/Cloud) | Tối ưu cho từng máy |
## 9.2 Breaking Changes

v1.5.0: LLM không còn dùng file tools. Handlers đọc/ghi trực tiếp.

v1.6.0: LlmConfig cần cấu hình EnableAdaptiveMax, ChunkingThresholdChars.

## 9.3 Migration Guide

Từ v1.4.0 lên v1.6.0:

Cập nhật SEOCrawlSupervisorOptions với LlmConfig.ForCpu() (hoặc preset phù hợp).

Đảm bảo IConversationMemory có method Clear().

Thêm ResetMemory() vào SpecialistAgent.

Gọi ResetMemory() trước mỗi URL trong RunStreamingAsync.

Sử dụng LlmResponseCleaner cho mọi LLM response.

10. Appendix
## 10.1 Timeout Calculation Examples

Ví dụ 1: Content 8,070 chars (ForCpu preset)

```text
Prompt = 8,070 + 500 (system) + 2,500 (template) = 11,070 chars
Response = 8,070 × 1.5 = 12,105 chars
Total chars = 23,175
Estimated tokens = 23,175 / 3.0 = 7,725 tokens
Base time = 7,725 / 12 = 644s
Nonlinear factor = 1.0 + (7725-2000)/1000 × 0.15 = 1.86x
Required = 644 × 1.0 × 1.8 × 1.86 = 2,156s
Adaptive max = 1800 + (11,070/1000) × 360 = 5,785s
Final = min(2156, 5785) = 2,156s ✅
Ví dụ 2: Content 15,000 chars (ForCpu preset + chunking)

```

```text
Do 15,000 > 10,000 → Chunking thành 4 chunks (4500 chars each)

Per chunk:
Prompt = 4,500 + 500 + 2,500 = 7,500 chars
Response = 4,500 × 1.5 = 6,750 chars
Total = 14,250 chars
Estimated tokens = 4,750 tokens
Base = 4,750 / 12 = 396s
Nonlinear = 1.0 + (4750-2000)/1000 × 0.15 = 1.41x
Required = 396 × 1.0 × 1.8 × 1.41 = 1,005s
Chunk max = 900s (baseMax cho chunk)
Final per chunk = 900s

```

# 4 chunks × 900s = 3,600s total ✅

## 10.2 Environment Variables

| Variable | Description | Default |
| --- | --- | --- |
| SEO_OUTPUT_DIR | Output directory path | ./output |
| SEO_BATCH_TIMEOUT | Total batch timeout (minutes) | 240 |
| SEO_WORKER_TIMEOUT | Worker timeout (seconds) | 2400 |
| SEO_TOKENS_PER_SECOND | LLM speed | 12 (CPU) |
| SEO_MAX_TIMEOUT | Max timeout per handler | 1800 |
| SEO_CHUNKING_THRESHOLD | Chunking threshold (chars) | 10000 |

## Document Version: 1.6.0
## Generated: 2026-09-10
## Next planned: v1.7.0 – Parallel chunk processing, Auto-Tool Synthesis 2.0
