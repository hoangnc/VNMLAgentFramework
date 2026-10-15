# Assistant Message

**Exported:** 15/10/2026 14:40:35
**Entry ID:** e3
**Session:** agent-c85f41b91123453fb647d9f8bf3e1da8

---

Đây là nội dung đã được định dạng lại theo chuẩn Markdown, đảm bảo cấu trúc, tiêu đề, đoạn văn, và các khối mã (code blocks) được trình bày rõ ràng và dễ đọc nhất.

***

# ADR-001: Chọn thư viện để tạo chart trong Excel export

> **Status:** Accepted
> **Date:** 2026-10-12
> **Deciders:** Nguyễn Công Hoàng
> **Affects:** `VnMLStudio.Plugin.MMO.DocumentIO.Exporters.ExcelDocumentExporter`

---

## Context

Khi implement feature "Export conversation sang Excel", requirement ban đầu chỉ là ghi data dạng bảng. Sau đó, user feedback yêu cầu **tự động generate chart** từ numeric data trong markdown tables — để report trực quan hơn, không cần user mở Excel rồi tự vẽ chart.

Chúng tôi đã chọn `ClosedXML` cho Excel data operations từ đầu vì:
- API cao cấp hơn `DocumentFormat.OpenXml` raw
- Fluent style, dễ maintain
- MIT license, không lo commercial
- Performance tốt cho data size vừa phải

Vấn đề phát sinh: cần thêm chart support, nhưng chưa rõ `ClosedXML` có hỗ trợ hay không.

---

## Decision Drivers

1. **Chart support** — requirement cốt lõi, không thể compromise
2. **License compatibility** — không dùng commercial license cho open-source framework
3. **Maintenance cost** — API không quá phức tạp để maintain lâu dài
4. **Migration cost** — nếu phải đổi library, chi phí viết lại code hiện tại bao nhiêu
5. **Community + maturity** — thư viện phải stable, có documentation, có StackOverflow answers

---

## Options Considered

### Option A: ClosedXML (giữ nguyên)

**Ưu điểm:**
- Đã dùng cho data operations
- Không cần đổi code hiện tại
- MIT license

**Nhược điểm — DEAD END:**
- **KHÔNG hỗ trợ tạo chart.** Không phải "hỗ trợ hạn chế" — mà là **zero support**.
- Class `XLChart` tồn tại trong source nhưng **chỉ là stub rỗng**. Không có method, không có implementation.
- GitHub issue #1294 (2019), #2721 (2025) xác nhận: maintainers nói rõ "no chart support, will not implement".
- Đây là architectural limitation, không phải backlog item.

**Verdict:** ❌ Loại bỏ. Không thể dùng cho requirement chart.

---

### Option B: EPPlus

**Ưu điểm:**
- Chart support **xuất sắc nhất** trong các option
- Fluent API đẹp, dễ code
- Full chart types: Line, Bar, Pie, Scatter, Bubble, Stock, Radar, Surface
- Documentation tốt, community lớn

**Nhược điểm — LICENSE BLOCKER:**
- Từ version 5 (2020), EPPlus chuyển sang **Polyform Noncommercial License**.
- Commercial use requires **paid license** (~$500-2000/year depending on team size).
- VNML Agent Framework là open-source (MIT), nhưng VNML Studio app có thể dùng trong enterprise environment.
- Không thể đảm bảo mọi user của VNML sẽ không dùng commercial → risk legal.

**Verdict:** ❌ Loại bỏ. License không phù hợp với open-source framework.

---

### Option C: `DocumentFormat.OpenXml` (raw SDK)

**Ưu điểm:**
- Microsoft official, MIT license
- Đã có sẵn trong project (dùng cho Word export)
- Full control — có thể tạo mọi chart type Excel hỗ trợ
- Zero additional dependency

**Nhược điểm:**
- API **rất low-level** — manipulate XML trực tiếp
- Cần ~200-400 dòng code cho **một chart cơ bản** (chart XML structure phức tạp: `ChartPart`, `DrawingPart`, relationships, series, axes, plot area)
- Debug khó — bug là XML invalid, Excel mở không được
- Không có IntelliSense tốt cho chart schema
- Time-to-implement cao

**Verdict:** ⚠️ Feasible nhưng không recommend cho scope hiện tại.

---

### Option D: NPOI

**Ưu điểm:**
- **Apache 2.0** — miễn phí commercial, tương thích mọi use case
- Chart support **đầy đủ** — Line, Bar, Area, Radar, Pie, Scatter, Bubble, Surface
- API trung bình (không fluent như EPPlus, nhưng cao hơn OpenXml raw)
- Mature — port từ Apache POI (Java), tồn tại từ 2010
- ~50M downloads trên NuGet
- Community VN + quốc tế active

**Nhược điểm:**
- API hơi "Java-style" (getter/setter thay vì property)
- Documentation ít hơn EPPlus
- Chart API mới (`chart.CreateData(ChartTypes.X, ...)`) chưa được document đầy đủ — phải đọc source code để hiểu
- Không có đầy đủ chart types như EPPlus (thiếu 3D variants)

**Verdict:** ✅ Recommended.

---

### Option E: Hybrid — ClosedXML (data) + OpenXml (chart)

**Ưu điểm:**
- Giữ code hiện tại cho data
- Chỉ thêm ~250 dòng cho chart injection
- Cả 2 library đều MIT

**Nhược điểm:**
- Phải save file bằng ClosedXML → mở lại bằng OpenXml → inject chart → save lại
- **Performance hit** cho file lớn (đọc/ghi 2 lần)
- **Complex state management** — 2 library giữ 2 model khác nhau của cùng file
- Debug phức tạp hơn nhiều (2 layers)

**Verdict:** ⚠️ Không đáng. Complexity không tương xứng với benefit.

---

## Decision

**Chọn Option D: NPOI.**

Chúng tôi thay `ClosedXML` bằng `NPOI` cho Excel export operation. Cụ thể:

```xml
<!-- Bỏ -->
<PackageReference Include="ClosedXML" Version="0.104.1" />

<!-- Thêm -->
<PackageReference Include="NPOI" Version="2.7.1" />
```

**Lý do:**

*   **License fit** — Apache 2.0 cho phép mọi use case (commercial + open-source), không cần consider legal.
*   **Chart support thực sự** — có API thật, không phải stub, có thể rely on.
*   **Migration effort hợp lý** — API NPOI tuy khác nhưng pattern tương tự (workbook → sheet → row → cell). ~1 ngày refactor.
*   **Không over-engineer** — không phải viết low-level OpenXml XML, tiết kiệm 300-400 dòng code.
*   **Trade-off chấp nhận được** — thiếu 3D chart không critical cho use case "conversation export".

### Consequences

**Positive**
*   Excel export có chart hoạt động (Line/Bar/Column/Pie/Area/Radar/Scatter).
*   Zero licensing risk cho commercial use.
*   Không có "dependency lock-in" từ closed-source library.
*   Đã validate end-to-end với Vietnamese data — không mojibake.

**Negative**
*   Refactor cost: ExcelDocumentExporter phải viết lại từ ClosedXML API → NPOI API (~350 dòng).
*   API learning curve: Chart API mới của NPOI chưa document đầy đủ, phải đọc source code.
*   Missing chart types: Không có 3D variants (Pie3D, Bar3D). Có thể thêm sau nếu cần.

**Import side unchanged?** — Import vẫn giữ ClosedXML vì đọc data không cần chart. Có 2 library cho Excel operations (ClosedXML import + NPOI export).

**Neutral**
*   Binary size tăng nhẹ: NPOI ~3 MB, ClosedXML ~5 MB. Cả 2 cùng tồn tại → ~8 MB. Acceptable cho desktop app.
*   2 libraries cho Excel: Không ideal về mặt "one tool per job", nhưng mỗi library mạnh ở 1 mặt (ClosedXML đọc tốt hơn, NPOI viết chart tốt hơn).

### Implementation Notes

#### Chart type mapping
`ChartKind` enum → `NPOI ChartTypes`:

| ChartKind | NPOI ChartTypes |
| :--- | :--- |
| Line | `ChartTypes.LINE` |
| Bar | `ChartTypes.BAR` (horizontal by default) |
| Column | `ChartTypes.BAR` (swap axes roles — tricky, may not work perfectly) |
| Pie | `ChartTypes.PIE` (no axes) |
| Area | `ChartTypes.AREA` |
| Radar | `ChartTypes.RADAR` |
| Scatter | `ChartTypes.SCATTER` (requires 2 numeric columns) |

#### NPOI chart API pattern (verified)
```csharp
// Verified from official NPOI examples:
// https://github.com/nissl-lab/npoi-examples/blob/main/xssf/LineChart/Program.cs

var drawing = (XSSFDrawing)sheet.CreateDrawingPatriarch();
var anchor = drawing.CreateAnchor(0, 0, 0, 0, col1, row1, col2, row2);
var chart = (XSSFChart)drawing.CreateChart(anchor);

chart.SetTitleText("My Chart");
var legend = chart.GetOrAddLegend();
legend.Position = LegendPosition.Bottom;

var bottomAxis = chart.CreateCategoryAxis(AxisPosition.Bottom);
var leftAxis = chart.CreateValueAxis(AxisPosition.Left);

var data = chart.CreateData(ChartTypes.LINE, bottomAxis, leftAxis);
var categoryData = DataSources.FromStringCellRange(sheet, categoryRange);
var valueData = DataSources.FromNumericCellRange(sheet, valueRange);
data.AddSeries(categoryData, valueData);

chart.Plot(data);
```

#### Known pitfalls
*   **Pie chart** — pass null for both axes: `chart.CreateData(ChartTypes.PIE, null, null)`.
*   **Scatter chart** — requires numeric X AND Y columns. Fallback to Line if only 1 numeric column.
*   **Column vs Bar** — NPOI treats "ColumnClustered" via `ChartTypes.BAR` (orientation via axes). Không có enum `COLUMN` riêng.

#### Alternatives Rejected — Why
| Option | Rejected because |
| :--- | :--- |
| A. ClosedXML | Zero chart support — dead end |
| B. EPPlus | License blocker — commercial use requires payment |
| C. OpenXml raw | Too much code for scope — ~400 lines per chart |
| E. Hybrid | Complexity not justified — 2 model views of same file |

### Follow-up Actions

☑ Replace ClosedXML with NPOI in `ExcelDocumentExporter`
☑ Implement `NpoiChartBuilder` with Line/Bar/Column/Pie/Area/Radar/Scatter
□ TODO: Add 3D chart variants (Pie3D, Bar3D) if user requests
□ TODO: Consider keeping ClosedXML only for import (current state) — evaluate consolidating to NPOI for import too in v1.2

***

## 📄 File 2 — Sequence Diagrams cho DocumentIO

**Path gợi ý:** `docs/DocumentIO-SequenceDiagrams.md`

```mermaid
sequenceDiagram
    autonumber
    actor User
    participant UI as InputBar
    participant Facade as DocumentIOFacade
    participant Reg as DocumentImporterRegistry
    participant Imp as WordDocumentImporter
    participant OpenXml as DocumentFormat.OpenXml
    participant FS as File System

    User->>UI: Click "+" / paste file
    UI->>UI: FileOpenPicker.PickMultipleFilesAsync()
    UI-->>User: Chip preview + spinner
    
    Note over UI: addFiles() starts
    
    UI->>UI: detect extension = ".docx"
    UI->>UI: .docx ∈ ImportableExtensions → true
    UI->>UI: isImporting.Set(true)
    
    UI->>Facade: ImportAsync("report.docx")
    Facade->>Reg: Resolve("report.docx")
    Reg->>Reg: First importer where CanImport()
    Reg-->>Facade: WordDocumentImporter
    
    Facade->>Imp: ImportAsync("report.docx", ct)
    Imp->>FS: File.Exists(path)?
    FS-->>Imp: true
    Imp->>Imp: Task.Run(() => ImportInternal(path))
    
    Imp->>OpenXml: WordprocessingDocument.Open(path, false)
    OpenXml-->>Imp: MainDocumentPart
    Imp->>Imp: iterate Body.Elements()
    
    loop Each Paragraph/Table
        Imp->>Imp: ExtractParagraphText / AppendTable
    end
    
    Imp->>OpenXml: Read CoreProperties (Title, Creator)
    OpenXml-->>Imp: metadata
    
    Imp->>Imp: ComputeWordCount, EstimatePageCount
    Imp-->>Facade: ImportResult.Ok(text, metadata)
    
    Facade->>Facade: LogInformation(word count, page count)
    Facade-->>UI: ImportResult
    
    UI->>UI: SaveTextAsTempAttachmentAsync(text, "report.docx")
    UI->>FS: WriteAllText(tempPath, text, UTF8)
    FS-->>UI: OK
    
    UI->>UI: attachments.Set([..., tempAttachment])
    UI->>UI: isImporting.Set(false)
    UI-->>User: Chip 📄 report.docx (12.4 KB)
```

### 2. Import Flow — Error Path
File `.docx` corrupt hoặc password-protected.

```mermaid
sequenceDiagram
    autonumber
    actor User
    participant UI as InputBar
    participant Facade as DocumentIOFacade
    participant Imp as WordDocumentImporter
    participant OpenXml as DocumentFormat.OpenXml

    User->>UI: Attach "corrupt.docx"
    UI->>Facade: ImportAsync("corrupt.docx")
    Facade->>Imp: ImportAsync(path)
    Imp->>OpenXml: WordprocessingDocument.Open()
    OpenXml--xImp: FileFormatException
    
    Imp->>Imp: catch (Exception ex)
    Imp->>Imp: return ImportResult.Fail("Word import failed: ...")
    Imp-->>Facade: ImportResult { Success=false, ErrorMessage="..." }
    
    Facade->>Facade: LogWarning(import failed)
    Facade-->>UI: ImportResult.Fail
    
    UI->>UI: Attachment.FromError(fileName, path, error)
    UI->>UI: attachments.Set([..., errorAttachment])
    UI-->>User: Chip ⚠️ corrupt.docx (border đỏ)
    
    Note over User,UI: Hover chip → tooltip shows full error
    
    User->>UI: Click chip ⚠️
    UI->>UI: ShowErrorFlyout(anchor, att)
    UI-->>User: MenuFlyout { Retry, Reveal, Copy error }
```

### 3. Import Flow — Retry After Fix
User fix file, click "Thử import lại" từ error flyout.

```mermaid
sequenceDiagram
    autonumber
    actor User
    participant UI as InputBar
    participant Flyout as MenuFlyout
    participant Facade as DocumentIOFacade
    participant Imp as WordDocumentImporter
    participant Storage as StorageFile

    Note over User: User fixed "corrupt.docx" externally
    
    User->>UI: Click error chip
    UI->>Flyout: ShowErrorFlyout(att)
    Flyout-->>User: Menu options
    
    User->>Flyout: Click "🔄 Thử import lại"
    Flyout->>UI: RetryImportAsync(att)
    
    UI->>Storage: GetFileFromPathAsync(path)
    Storage-->>UI: StorageFile
    
    UI->>UI: isImporting.Set(true)
    UI->>Facade: ImportAsync(path)
    Facade->>Imp: ImportAsync(path)
    Imp-->>Facade: ImportResult.Ok(text, metadata)
    Facade-->>UI: ImportResult
    
    UI->>UI: SaveTextAsTempAttachmentAsync(text, fileName)
    UI->>UI: attachments = replace(old, newImportedAttachment)
    UI->>UI: isImporting.Set(false)
    UI-->>User: Chip changes ⚠️ → 📄
```

### 4. Export Flow — Save As Dialog + Rendering
User export assistant message → PDF với syntax highlighting.

```mermaid
sequenceDiagram
    autonumber
    actor User
    participant UI as ChatTimeline
    participant Picker as FileSavePicker
    participant Facade as DocumentIOFacade
    participant Reg as DocumentExporterRegistry
    participant Exp as PdfDocumentExporter
    participant Quest as QuestPDF
    participant FS as File System

    User->>UI: Click "Export" button on assistant msg
    UI->>UI: ExportAssistantMessageAsync(text, entryId)
    UI->>UI: BuildMarkdownContent(text) → "# Assistant Message\n..."
    
    UI->>Picker: new FileSavePicker
    Picker->>Picker: Add choices: .docx, .xlsx, .pdf, .md, .txt
    Picker-->>User: Save dialog
    
    User->>Picker: Choose "report.pdf"
    Picker-->>UI: StorageFile { Path }
    
    Note over UI: FileSavePicker creates empty placeholder file
    
    UI->>FS: File.Exists(path) → true
    UI->>FS: File.Delete(path)
    Note over UI,FS: OpenXml requires exclusive write access
    
    UI->>UI: format = InferFormatFromPath(".pdf") → Pdf
    UI->>Facade: ExportAsync(Pdf, request)
    
    Facade->>Reg: Resolve(Pdf)
    Reg-->>Facade: PdfDocumentExporter
    
    Facade->>Exp: ExportAsync(request, ct)
    Exp->>Exp: Task.Run(() => ExportInternal(request))
    
    Exp->>Exp: MarkdownParser.Parse(content) → blocks
    Exp->>Quest: Document.Create(container)
    
    loop Each Block
        alt Heading
            Exp->>Exp: RenderBlock → Text.Bold.FontSize
        else Code Block
            Exp->>Exp: SyntaxHighlighter.Tokenize(code, lang)
            loop Each Token
                Exp->>Exp: Set FontColor from SyntaxTheme
            end
        else Table
            Exp->>Exp: RenderTable → Column(RelativeColumn)
        end
    end
    
    Exp->>Quest: .GeneratePdf(targetPath)
    Quest->>FS: Write PDF bytes
    FS-->>Quest: OK
    Quest-->>Exp: void
    
    Exp-->>Facade: ExportResult.Ok(path)
    Facade->>Facade: LogInformation("Exported {filename}")
    Facade-->>UI: ExportResult
    
    UI->>UI: ShowNotification("Export thành công", path)
    UI-->>User: InfoBar success
```

### 5. Export Flow — Excel with Charts
User export markdown table with numeric data → auto chart.

```mermaid
sequenceDiagram
    autonumber
    participant UI as ChatTimeline
    participant Facade as DocumentIOFacade
    participant Exp as ExcelDocumentExporter
    participant Parser as MarkdownParser
    participant Builder as NpoiChartBuilder
    participant NPOI as NPOI Library

    UI->>Facade: ExportAsync(Excel, request)
    Note right of request: chart_type = "auto" or "line" etc.
    
    Facade->>Exp: ExportAsync(request, ct)
    Exp->>Parser: Parse(request.Content)
    Parser-->>Exp: List<MarkdownBlock>
    
    Exp->>Exp: tables = blocks.Where(Kind == Table)
    
    alt Tables found
        loop Each Table
            Exp->>NPOI: workbook.CreateSheet(name)
            Exp->>Exp: WriteTable(sheet, table)
            Note right of Exp: Header row + data rows<br/>Auto-detect numeric → NumberFormat
            
            Exp->>Exp: ParseChartTypeHint(request) → Auto / Explicit / None
            
            alt chartHint != None
                Exp->>Exp: ResolveChartType(hint, table)
                alt Hint Explicit
                    Exp->>Builder: TryAddChart(sheet, table, kind)
                else Auto infer
                    Exp->>Exp: numericCols = detectNumericColumns()
                    alt 1 numeric col + ≤8 rows
                        Exp->>Builder: TryAddChart(sheet, table, Pie)
                    else ≥8 rows + ≤2 series
                        Exp->>Builder: TryAddChart(sheet, table, Line)
                    else
                        Exp->>Builder: TryAddChart(sheet, table, Column)
                    end
                end
                
                Builder->>NPOI: drawing.CreateChart(anchor)
                Builder->>NPOI: chart.CreateCategoryAxis(Bottom)
                Builder->>NPOI: chart.CreateValueAxis(Left)
                Builder->>NPOI: chart.CreateData(ChartTypes.X, ...)
                Builder->>NPOI: data.AddSeries(categoryData, valueData)
                Builder->>NPOI: chart.Plot(data)
                Builder-->>Exp: true / false
            end
        end
    else
        Note over Exp: No tables → fallback content dump
        Exp->>NPOI: CreateSheet("Content") with 2 cols
    end
    
    Exp->>NPOI: workbook.Write(fs)
    NPOI-->>Exp: OK
    Exp-->>Facade: ExportResult.Ok
    Facade-->>UI: ExportResult
```

### 6. Import Cache + Prompt Merge
Full lifecycle: user attach file → import → merge into LLM prompt.

```mermaid
sequenceDiagram
    autonumber
    actor User
    participant InputBar
    participant Facade as DocumentIOFacade
    participant Importer as WordDocumentImporter
    participant Attachment as Attachment Record
    participant Page as CrawlSeoPage
    participant Provider as AgentChatProvider
    participant Harness as ReflectiveStreamingAgentHarness
    participant LLM

    User->>InputBar: Attach "report.docx"
    InputBar->>Facade: ImportAsync(path)
    Facade->>Importer: ImportAsync(path)
    Importer-->>Facade: text + metadata
    Facade-->>InputBar: ImportResult.Ok
    
    InputBar->>Attachment: Create TempAttachment(text, fileName="report.docx")
    Note right of Attachment: IsTempFile=true<br/>DisplayName="report.docx"<br/>Path=temp.txt
    
    InputBar-->>User: Chip 📄 report.docx
    
    User->>InputBar: Click Send
    InputBar->>InputBar: SendMessageAsync()
    InputBar->>Page: OnSendFromInputBar(text, [tempAttachment])
    
    Page->>Page: ConvertToChatAttachments(uiAttachments)
    Note right of Page: Skip error chips<br/>Keep temp files (already extracted text)
    
    Page->>Provider: SendMessageAsync(threadId, text, chatAttachments)
    
    Provider->>Provider: ChatTimelineReducer.AddLocalUser(timeline, text, nonce, attachments)
    Note right of Provider: Attachment metadata saved to timeline.bin<br/>Text stored separately (user input only)
    
    Provider->>Harness: RunTurnAsync(threadId, text, attachments)
    
    Harness->>Harness: BuildPromptWithAttachments(text, attachments)
    
    loop Each Attachment
        alt Text file < 200 KB
            Harness->>Harness: File.ReadAllText(path)
            Harness->>Harness: Wrap in "UNTRUSTED DATA" fence
        else Large / binary file
            Harness->>Harness: Emit metadata + tool hint
        end
    end
    
    Harness->>LLM: Prompt = user text + attachments content
    LLM-->>Harness: Response
    Harness-->>Provider: Stream events
    Provider-->>Page: Timeline updated
    Page-->>InputBar: Re-render
    InputBar-->>User: Assistant response
```

### 7. FileOpenPicker + Attachment Workflow — Detailed
Zoom vào đoạn `addFiles` với merge/lock.

```mermaid
sequenceDiagram
    autonumber
    actor User
    participant UI as InputBar
    participant Picker as FileOpenPicker
    participant Files as StorageFile[]
    participant Lock as _attachmentLock
    participant State as attachments (UseState)

    User->>UI: Click "+" button
    UI->>Picker: PickMultipleFilesAsync()
    Picker-->>User: File dialog
    User->>Picker: Select 3 files (2 docx, 1 png)
    Picker-->>UI: StorageFile[3]
    
    UI->>UI: addFiles(files)
    UI->>UI: hasImportable = true (.docx present)
    UI->>UI: isImporting.Set(true)
    
    par Sequential import per file
        UI->>UI: ImportDocumentAsync("a.docx")
        Note right of UI: documentFacade.ImportAsync()<br/>→ SaveTextAsTempAttachmentAsync()
        
        UI->>UI: ImportDocumentAsync("b.docx")
        Note right of UI: same flow
    and Regular file
        UI->>UI: AttachRawFileAsync("c.png")
        Note right of UI: GetBasicPropertiesAsync()<br/>→ Attachment (no import)
    end
    
    UI->>UI: newItems = [tempA, tempB, rawC]
    
    UI->>Lock: lock (_attachmentLock)
    
    Note over Lock: Critical section — prevent lost update
    
    UI->>State: merged = current + newItems (dedupe by path)
    UI->>State: attachments.Set(merged)
    UI->>Lock: release
    
    UI->>UI: isImporting.Set(false)
    UI-->>User: 3 chips: 📄 a.docx, 📄 b.docx, 📎 c.png
```

### 8. Error Flyout Interaction
Zoom vào error chip flyout với 3 actions.

```mermaid
sequenceDiagram
    autonumber
    actor User
    participant Chip as Error Chip UI
    participant Flyout as MenuFlyout
    participant Retry as RetryImportAsync
    participant Reveal as RevealInExplorer
    participant Clipboard
    participant Explorer as File Explorer

    User->>Chip: Click error chip ⚠️
    Chip->>Flyout: ShowErrorFlyout(anchor, att)
    
    Note right of Flyout: MenuFlyoutItems:<br/>1. 🔄 Thử import lại<br/>2. 📂 Mở trong File Explorer<br/>3. 📋 Sao chép chi tiết lỗi
    
    Flyout-->>User: Menu shown
    
    alt User picks Retry
        User->>Flyout: Click "🔄"
        Flyout->>Retry: RetryImportAsync(att)
        Retry->>Retry: StorageFile.GetFileFromPathAsync
        Retry->>Retry: ImportDocumentAsync again
        Retry->>Chip: Replace attachment in state
        Chip-->>User: Chip changes ⚠️ → 📄
    else User picks Reveal
        User->>Flyout: Click "📂"
        Flyout->>Reveal: RevealInExplorer(path)
        Reveal->>Explorer: Process.Start("explorer.exe /select,path")
        Explorer-->>User: Folder opened, file highlighted
    else User picks Copy
        User->>Flyout: Click "📋"
        Flyout->>Clipboard: DataPackage.SetText(errorDetail)
        Clipboard-->>User: Toast "Copied"
    end
```

### 9. Save File Picker + Format Inference
Zoom vào `ChatTimeline.ExportMarkdownAsync`.

```mermaid
sequenceDiagram
    autonumber
    participant UI as ChatTimeline
    participant Picker as FileSavePicker
    participant Infer as DocumentIOFacade.InferFormatFromPath
    participant Facade as DocumentIOFacade
    participant FS as File System

    UI->>Picker: new FileSavePicker
    Picker->>Picker: Add FileTypeChoices:
    Note right of Picker: - Word (.docx)<br/>- Excel (.xlsx)<br/>- PDF (.pdf)<br/>- Markdown (.md)<br/>- Text (.txt)
    
    Picker-->>UI: User picks "conversation.pdf"
    
    UI->>FS: Check if placeholder exists
    alt Placeholder exists
        UI->>FS: File.Delete(path)
        Note right of FS: OpenXml/ClosedXML require exclusive write
    end
    
    UI->>Infer: InferFormatFromPath("conversation.pdf")
    Infer->>Infer: switch(extension)
    Infer-->>UI: DocumentFormat.Pdf
    
    alt format == Unknown
        UI-->>UI: ShowNotification("Unsupported format")
    else format != Unknown
        UI->>Facade: ExportAsync(Pdf, request)
        Facade-->>UI: ExportResult
        UI-->>UI: Notify success/fail
    end
```

### 10. Concurrency — Multiple Files, Parallel Import
3 file cùng lúc, giả định `addFiles` called concurrently.

```mermaid
sequenceDiagram
    autonumber
    participant UI1 as addFiles task 1
    participant UI2 as addFiles task 2
    participant Lock as _attachmentLock
    participant State as attachments (shared)

    Note over UI1,UI2: Both tasks triggered concurrently
    
    UI1->>UI1: Import 2 files → newItems1 = [A, B]
    UI2->>UI2: Import 1 file  → newItems2 = [C]
    
    par Race on state write
        UI1->>Lock: lock() → acquire
        UI1->>State: read current
        UI1->>State: merged1 = current + [A,B]
        UI1->>State: Set(merged1)
        UI1->>Lock: release
        
        UI2->>Lock: lock() → wait
        Note right of UI2: UI1 held lock
        UI1-->>UI2: Lock released
        UI2->>Lock: acquire
        UI2->>State: read current = merged1
        UI2->>State: merged2 = merged1 + [C]
        UI2->>State: Set(merged2)
        UI2->>Lock: release
    end
    
    Note over State: Final state = current + [A, B, C]<br/>No lost update
```

### 11. Font Fallback Chain — PDF Export
Zoom vào PDF export khi text chứa box-drawing + emoji.

```mermaid
sequenceDiagram
    autonumber
    participant Exp as PdfDocumentExporter
    participant Quest as QuestPDF Engine
    participant Lato as Lato (primary)
    participant Noto1 as Noto Sans Symbols 2
    participant Noto2 as Noto Emoji
    participant PDF as PDF Output

    Exp->>Quest: Document.Create with FontFamily(Lato, Noto1, Noto2)
    
    Quest->>Lato: Render text "═" (U+2550)
    Lato-->>Quest: Glyph not found
    
    Quest->>Noto1: Fallback: render "═"
    Noto1-->>Quest: Glyph found ✓
    
    Quest->>Lato: Render text "🚀" (U+1F680)
    Lato-->>Quest: Glyph not found
    
    Quest->>Noto1: Fallback: render "🚀"
    Noto1-->>Quest: Glyph not found
    
    Quest->>Noto2: Fallback: render "🚀"
    Noto2-->>Quest: Glyph found ✓
    
    Quest->>PDF: Write glyphs to output
    
    Note over Quest: If ALL fallbacks fail:<br/>- ThrowOnMissingTextGlyphs=true → exception<br/>- false → render placeholder □
```

### 12. Summary Table — Key Interaction Points

| Flow | Participants | Critical path |
| :--- | :--- | :--- |
| Import happy | InputBar → Facade → Registry → Importer | File read + parse |
| Import error | InputBar → Facade → Importer catch | Return ImportResult.Fail |
| Retry | Error chip → Flyout → Facade | Same as import, replace state |
| Export | ChatTimeline → Facade → Exporter | Parse markdown + render |
| Prompt merge | Harness → BuildPromptWithAttachments | Inline under budget |
| Concurrency | Multiple addFiles | Guard by `_attachmentLock` |
| Font fallback | QuestPDF → font chain | First glyph match wins |
