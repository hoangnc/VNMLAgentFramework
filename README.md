<div align="center">

<img src="https://raw.githubusercontent.com/hoangnc/VNMLAgentFramework/main/VNMLStudio_UI_Preview.png" alt="VNML Studio" width="100%"/>

# VNML Agent Framework

**A high-performance multi-agent framework in .NET 10 — running 100% local, zero API cost.**

Build self-improving AI agents with reflection, planning, learning, and dynamic tool synthesis.

[![.NET](https://img.shields.io/badge/.NET-10-512BD4?style=flat-square&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![License: BSL 1.1](https://img.shields.io/badge/License-BSL%201.1-blue.svg)](https://mariadb.com/bsl11/)
[![Platform](https://img.shields.io/badge/Platform-Windows%20%7C%20Linux%20%7C%20macOS-blue?style=flat-square)]()
[![Local](https://img.shields.io/badge/100%25-Local-orange?style=flat-square)]()
[![API Cost](https://img.shields.io/badge/API%20Cost-%240-success?style=flat-square)]()

[Getting Started](#-quick-start) · [Features](#-features) · [Architecture](#-architecture) · [Blog](http://vnmlstudio.runasp.net/en/blogs/) · [Dev.to](https://dev.to/hoangnc)

</div>

---

## 🎯 Why VNML Agent Framework?

Most AI agent frameworks assume you have OpenAI API keys, cloud budget, and reliable internet.

**VNML Agent Framework is different.** It's built for developers who need:

- 🔒 **Privacy** — Data never leaves your machine
- 💰 **Zero API cost** — No OpenAI/Anthropic/Gemini bills
- 🌐 **Offline-first** — Works air-gapped, on planes, in secure facilities
- 🖥️ **Desktop-native** — Built for WPF/WinUI/Avalonia, not web servers
- ⚡ **Performance** — C# native, no GIL, no Python dependency hell

> **Perfect for:** Banking, healthcare, government, enterprise — environments where cloud AI is not an option.

---

## ⚡ Features

### 🧠 P0 — Reflection Engine
The core differentiator. Heuristic-first, LLM-second.
- Detects **stuck loops**, **hallucination**, **tool misinterpretation**
- Auto-summarizes when context window is critical
- Triggers auto-retry with improved prompts
- **<100ms overhead** per reflection cycle

### 📋 P1 — Planning & Multi-Agent
- Hierarchical Task Planner with DAG decomposition
- Topological sort + parallel execution
- Auto-replan when sub-task fails
- Multi-Agent Team with Supervisor pattern

### 📚 P2 — Learning
- **EpisodicStore** — SQLite-backed trajectory storage
- **SkillExtractor** — Auto-extract reusable skills from successful runs
- **Proactive Scheduler** — Auto-schedule follow-up tasks
- **SkillBank** — Vector-based retrieval (Qdrant)

### 🛠️ P3 — Dynamic Tool Synthesis
- LLM generates C# code → **Roslyn compiles at runtime** → tool activates
- SQL safety guards + JSON Schema validation
- Max 2 synthesis per plan (prevents runaway)
- Agents **grow capabilities** without redeployment

### 🚀 Zero API Cost
Optimized for small local LLMs via llama.cpp:
- **Qwen 3.8B** — Fast, runs on 4GB VRAM
- **Gemma 4 E2B / E4B** — Google's efficient models
- **Runs on CPU-only laptops** — 8GB RAM sufficient

---
## 🚧 Project Status

**Current state:** In active development. Publishing in phases.

| Component | Status | ETA |
|---|:---:|---|
| P0 Reflection Engine | 🟡 Publishing soon | Week 2 |
| P1 Planning & Multi-Agent | 🟡 Publishing soon | Week 3 |
| P2 Learning | ⚪ Planned | Week 4 |
| P3 Tool Synthesis | ⚪ Planned | Week 4 |
| Full Framework | ⚪ Planned | Month 2 |
| NuGet Packages | ⚪ Planned | Month 3 |

**Want early access?** Star this repo or [subscribe to updates](http://vnmlstudio.runasp.net/en/blogs/).

---

## 📊 Comparison

| Feature | LangChain | AutoGen | CrewAI | **VNML** |
|---|:---:|:---:|:---:|:---:|
| Language | Python | Python | Python | **C# / .NET** |
| 100% Local | ❌ | ❌ | ❌ | ✅ |
| Zero API Cost | ❌ | ❌ | ❌ | ✅ |
| Desktop Native | ❌ | ❌ | ❌ | ✅ |
| Runtime Tool Synthesis | ❌ | ❌ | ❌ | ✅ |
| Heuristic Reflection (<100ms) | ❌ | ❌ | ❌ | ✅ |
| Hierarchical Planning | ⚠️ | ✅ | ⚠️ | ✅ |
| Skill Learning | ❌ | ❌ | ❌ | ✅ |

---

## 🚀 Quick Start

### Prerequisites
- .NET 10 SDK
- llama.cpp server (bundled or separate)
- 8GB RAM minimum (16GB recommended)

### 1. Clone & Build

```bash
git clone https://github.com/hoangnc/VNMLAgentFramework.git
cd VNMLAgentFramework
dotnet build -c Release
```
### 2. Download a Model
```bash
# Qwen 3.8B Q4 (recommended, ~2.8GB)
# Get from HuggingFace: [https://huggingface.co/empero-ai/Qwen3.8-4B-Distill-GGUF]
```
### 3. Run First Agent
```csharp
using VnMLStudio.Agent;

// Create agent with reflection enabled
var agent = new ReflectiveAgentBuilder()
    .WithModel("qwen-3.8b-q4.gguf")
    .WithReflection()          // P0 - self-critique
    .WithPlanning()            // P1 - task decomposition
    .WithTools(builder =>
    {
        builder.AddTool<WebScraperTool>();
        builder.AddTool<FileWriterTool>();
        builder.AddTool<SynthesisTool>(); // P3 - dynamic C# synthesis
    })
    .Build();

// Execute task
var result = await agent.ExecuteAsync(
    "Research the top 5 AI agent frameworks and write a comparison report");

Console.WriteLine(result.Output);
```
---

### 4. Watch It Self-Improve
```text
[Planning] Created plan with 4 sub-tasks
[Reflective] Executing: Research frameworks...
[ToolSynthesis] Tool "FrameworkComparer" not found
[ToolSynthesis] Generating C# source...
[ToolSynthesis] Roslyn compile: SUCCESS
[ToolSynthesis] Tool activated. Replanning...
[Reflective] Result complete. Skill extracted.
[Learning] Episode saved to SkillBank
```
---

## 🏗️ Architecture
```text
┌─────────────────────────────────────────────────────────────┐
│  Application Layer (WPF / WinForms / Console / API)         │
├─────────────────────────────────────────────────────────────┤
│  Orchestration Layer (Decorator Chain)                      │
│   Proactive → Learning → Team → Planner → Reflective        │
├─────────────────────────────────────────────────────────────┤
│  Harness Layer                                              │
│   Streaming Agent · ReAct · Guardrails                      │
├─────────────────────────────────────────────────────────────┤
│  Memory & Tools                                             │
│   VectorMemory · EpisodicStore · SkillBank · ToolRegistry   │
├─────────────────────────────────────────────────────────────┤
│  Inference Layer                                            │
│   LlamaCppSharp · llama.cpp · GGUF models                   │
└─────────────────────────────────────────────────────────────┘
```
**Design Principles:**

**Drop-in upgrade —** Enable P0 → P1 → P2 → P3 without refactoring

**Decorator pattern —** Each capability wraps the previous

**Fail-safe defaults —** Reflection catches errors before they propagate

---

## 🎬 Real-World Examples
### SEO Content Automation
https://raw.githubusercontent.com/hoangnc/VNMLAgentFramework/main/VNMLAgentframework_App_SEO_CRAWL_BATCH_ORCHESTRATOR.png

Batch crawl competitor URLs → deep SEO analysis → write 100% original content with 7-layer quality check → overnight.

### PriceHunter Agent
https://raw.githubusercontent.com/hoangnc/VNMLAgentFramework/main/VNMLAgentFramework_Example_Agent_PriceHunter.png

Multi-step price tracking across e-commerce sites with dynamic tool synthesis.

### VNML Studio
https://raw.githubusercontent.com/hoangnc/VNMLAgentFramework/main/VNMLStudio_PluginMMO_Screen_01.png

Full desktop IDE with plugin system, docking, and Monaco editor.

## 📚 Documentation & Resources
- **Source Code:** [Coming soon]
- **📖 Blog**: vnmlstudio.runasp.net/blogs

- **🎥 YouTube**: [Coming soon]

- **💬 Dev.to**: @hoangnc

- **🐦 X/Twitter**: @HoangNC_VNML

## Deep Dives
- **5 Layers of Agentic Architecture:** http://vnmlstudio.runasp.net/en/blogs/5-tang-kien-tao-agentic.html 
- **Why We Removed Tool Calls from the LLM:** http://vnmlstudio.runasp.net/en/blogs/removing-tool-calls-llm.html
- **Dynamic Timeout with Thinking Mode:** http://vnmlstudio.runasp.net/en/blogs/dynamic-timeout-thinking-mode.html
- **Microsoft Copilot Search Indexed Our Framework:** http://vnmlstudio.runasp.net/en/blogs/microsoft-copilot-search-indexed-framework.html

## 🗺️ Roadmap
- ☑ P0 — Reflection Engine
- ☑ P1 — Planning & Multi-Agent
- ☑ P2 — Learning & Skill Extraction
- ☑ P3 — Dynamic Tool Synthesis
- □ v2.0 — Plugin ecosystem + NuGet packages
- □ v2.1 — Cross-platform GUI (Avalonia)
- □ v2.2 — Cloud GPU orchestration (Vast.ai, RunPod)
- □ v3.0 — Multi-language support (EN, VI, ZH, JA)
**Vote for features:** GitHub Discussions

## 🤝 Contributing
We welcome contributions! Especially:

- 🐛 Bug reports — Open an issue with reproduction steps
- 📝 Documentation — Fix typos, add examples
- 🔧 Tools — Add new tool implementations
- 🌐 Translations — Help translate docs

See CONTRIBUTING.md for guidelines.

## Contributors
<a href="https://github.com/hoangnc/VNMLAgentFramework/graphs/contributors"> <img src="https://contrib.rocks/image?repo=hoangnc/VNMLAgentFramework" /> </a>
### ⭐ Star History
https://api.star-history.com/svg?repos=hoangnc/VNMLAgentFramework&type=Date

---

## 📄 License

VNML Agent Framework is licensed under the **Business Source License 1.1 (BSL 1.1)**.

### What This Means

| Use Case | Free? |
|---|:---:|
| Personal projects | ✅ |
| Educational use | ✅ |
| Non-commercial open source | ✅ |
| Development & testing | ✅ |
| **Commercial production** | 💼 Requires license |
| **Hosted SaaS** | 💼 Requires license |
| **Embedded in commercial product** | 💼 Requires license |

### Why BSL?

- 🎁 **Free for individuals** — học tập, nghiên cứu, dự án cá nhân
- 💼 **Sustainable** — công ty trả tiền để nuôi dev
- 🔓 **Auto-open-source** — sau 4 năm, chuyển thành Apache 2.0

### Commercial Licensing

For production commercial use, contact: **contact@vnmlstudio.ai**

See [COMMERCIAL.md](COMMERCIAL.md) for pricing and terms.

### Long-Term Commitment

After 4 years from each release date, that version automatically becomes **Apache 2.0** licensed. Community sẽ được dùng mãi mãi.

*Inspired by MariaDB, HashiCorp, and Redis licensing models.*

## 💬 About the Author
Built by Nguyễn Công Hoàng — a solo developer from Vietnam with 20 years of experience, building local-first AI tools for developers who need privacy and zero-cost solutions.

"If OpenAI can be shut down tomorrow, your agents shouldn't be."

## Connect:

- 🐦 X: @HoangNC_VNML
- 💼 LinkedIn: www.linkedin.com/in/hoang-nguyen-404486142
- 📧 Email: diemhoang8488@gmail.com

## ⭐ If this project helps you, please consider giving it a star!

Report Bug · Request Feature · Discussions

## Made with ❤️ in Vietnam
