<div align="center">
  <h1>🎙️ SafeScribe AI</h1>
  <h3>The Privacy-First, 100% Local AI Meeting Assistant &amp; Intelligence Suite</h3>

  <p align="center">
    <b>Zero Cloud Leakage • 100% Air-Gapped Data Sovereignty • Zero Subscriptions</b>
  </p>

  <p align="center">
    <a href="#-key-features"><img src="https://img.shields.io/badge/Framework-.NET%2010-512BD4?style=for-the-badge&logo=dotnet" alt=".NET 10"></a>
    <a href="#-architecture"><img src="https://img.shields.io/badge/UI_Engine-Avalonia_UI-purple?style=for-the-badge" alt="Avalonia UI"></a>
    <a href="#-supported-platforms"><img src="https://img.shields.io/badge/Platforms-Windows_%7C_macOS_%7C_Linux-blue?style=for-the-badge" alt="Platforms"></a>
    <a href="#-resource-efficiency"><img src="https://img.shields.io/badge/UI_Baseline-~40MB_RAM-brightgreen?style=for-the-badge" alt="UI Baseline Memory"></a>
    <a href="LICENSE"><img src="https://img.shields.io/badge/License-MIT-green?style=for-the-badge" alt="License"></a>
  </p>

  <p align="center">
    <b>SafeScribe AI</b> captures, transcribes, diarizes, and summarizes your meetings <b>entirely on your local machine</b>.<br>
    Powered by local Whisper speech recognition and local Ollama LLMs — <b>no data ever leaves your computer</b>.
  </p>
</div>

---

## 🔒 Why SafeScribe AI?

Cloud-based meeting bots (Otter, Fireflies, Zoom AI) introduce major privacy, compliance, and legal risks:
* 🔴 **Corporate Confidentiality Exposure**: Transcripts stored on third-party cloud servers you don't control.
* 🔴 **Regulatory Compliance Violations**: GDPR, HIPAA, and NDA breaches from cloud data transmission.
* 🔴 **Expensive Recurring Subscriptions**: Paying per-user monthly fees for cloud processing.

### 🛡️ The SafeScribe AI Solution
SafeScribe AI is built from the ground up for **complete data sovereignty**:
- 🟢 **100% Offline / Air-Gapped**: Works seamlessly offline on planes, remote sites, or restricted enterprise networks.
- 🟢 **Local Speech-To-Text**: Uses Whisper.cpp / `Whisper.net` with GPU acceleration (CUDA / Metal / Vulkan).
- 🟢 **Ultra-Low ~40MB UI Footprint**: Built on **.NET 10 Avalonia UI**, preserving 95%+ of system RAM specifically for your local AI models (optimized for 8GB RAM laptops).

#### 📊 Transparent Memory Allocation Breakdown
| Component | Memory Allocation | Purpose |
| :--- | :---: | :--- |
| **SafeScribe Desktop App & UI** | **~40 MB – 60 MB** | Lightweight .NET 10 Avalonia process baseline |
| **Local Whisper STT Model** | **~150 MB – 480 MB** | Local speech recognition model (`base` / `small`) |
| **Local Ollama LLM Model** | **~1.8 GB – 4.5 GB** | Local LLM (`llama3.2:3b`, `phi3`, `llama3:8b`) |
| **Total Active Memory** | **~2.2 GB – 4.9 GB** | Runs comfortably on **8GB RAM laptops** |

---

## ✨ Key Features

| Feature | Description |
| :--- | :--- |
| 🎙️ **Dual-Stream WASAPI & CoreAudio Capture** | Simultaneously captures system speaker audio (loopback) and microphone input with RMS ducking. |
| 🗣️ **Speaker Diarization** | Automatically attributes transcript segments (*"Who said what"*). |
| 📋 **Structured AI Meeting Intelligence** | Automatically extracts: <ul><li><b>2-3 Paragraph Meeting Summary</b></li><li><b>Key Takeaways</b></li><li><b>Action Items</b> with Assignees &amp; Due Dates</li></ul> |
| 📤 **1-Click Productivity Exporters** | Export formatted notes directly to **Obsidian Vaults**, copy pre-formatted **Slack/Teams** channel snippets, or generate **ICS Calendar** events. |
| ⚡ **Decoupled Architecture** | Core engine (`SafeScribe.Core`) has <b>zero UI dependencies</b> and can run standalone, headless, or as a CLI/service. |

---

## 📐 System Architecture

```
SafeScribe.slnx (.NET 10 Solution)
│
├── 🧠 SafeScribe.Core (100% C# .NET 10 Core Engine — ZERO UI Dependencies)
│   ├── Abstractions/        # Clean Interface Contracts (IAudioCapture, ISpeechToText, ILlmService, IStorage)
│   ├── Audio/               # Dual WASAPI (Windows) & CoreAudio/ALSA (macOS/Linux) audio streams
│   ├── SpeechToText/        # Whisper.net local speech recognition engine
│   ├── Ollama/              # Local LLM summary engine (llama3, phi3, deepseek-r1)
│   ├── Diarization/         # Speaker Diarization logic ("Who said what")
│   ├── Data/                # SQLite local persistence (Microsoft.Data.Sqlite)
│   └── Exporters/           # Obsidian MD, Slack/Teams, Notion, ICS Calendar export
│
└── 🎨 SafeScribe.UI.Avalonia (Cross-Platform Desktop Frontend)
    ├── Views/               # Avalonia XAML Views (MainWindow, FloatingWidget)
    ├── ViewModels/          # Reactive MVVM ViewModels
    └── Styles/              # Modern Dark Fluent Design Theme (~40MB RAM footprint)
```

---

## 💻 Supported Platforms

* 🪟 **Windows 10 / 11** (x64, ARM64)
* 🍎 **macOS 12+** (Intel & Apple Silicon M1/M2/M3/M4 via Metal GPU acceleration)
* 🐧 **Linux** (Ubuntu, Debian, Fedora, Arch via X11/Wayland)

---

## 🚀 Quickstart Guide

### Prerequisites
1. [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) installed.
2. [Ollama](https://ollama.com) installed and running locally with your model of choice (e.g. `ollama run llama3`).

### Running the App

#### Method A: 1-Click Batch Script (Windows)
Double-click `Run-SafeScribe-Avalonia.bat` in the project root.

#### Method B: Terminal Command (Windows, macOS, Linux)
```bash
# Clone the repository
git clone https://github.com/piyushjain201096/SafeScribeAI.git
cd SafeScribeAI

# Run the cross-platform Avalonia UI desktop app
dotnet run --project SafeScribe.UI.Avalonia/SafeScribe.UI.Avalonia.csproj
```

---

## 🧪 How to Test End-to-End

1. **Launch App**: Run the app using Method A or B above.
2. **Import Sample Audio**: Click **`📁 Import Audio File (WAV/MP3)`** and choose the included [`sample_meeting.wav`](sample_meeting.wav).
3. **Review Local AI Summary**: Watch local Whisper transcribe the audio and local Ollama output a structured meeting summary & action items.
4. **Export**: Click **`Obsidian MD`** or **`Copy for Slack/Teams`** to export your notes instantly.

---

## 📜 License

SafeScribe AI is open-source software licensed under the [MIT License](LICENSE).
