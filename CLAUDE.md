# CLAUDE.md

This file provides guidance to AI coding assistants when working with code in this repository.

## Project Overview

**SafeScribe AI** is a privacy-first, 100% local AI meeting assistant that captures, transcribes, diarizes, and summarizes meetings entirely on local infrastructure with zero cloud data leakage.

The application is built on a **100% C# .NET 10 solution**:
1. **`SafeScribe.Core`**: Standalone .NET 10 class library holding all domain engine logic (WASAPI/CoreAudio capture, Whisper speech-to-text, Ollama LLM integration, SQLite database, exporters). Zero UI dependencies.
2. **`SafeScribe.UI.Avalonia`**: Cross-platform desktop application (Windows, macOS, Linux) built with **Avalonia UI** (~40MB RAM footprint).

### Key Technology Stack
- **Target Framework**: .NET 10 (`net10.0`)
- **Desktop UI**: Avalonia UI 11.x + Fluent Theme (Cross-Platform)
- **Architecture**: MVVM pattern with clean interface abstractions (`SafeScribe.Core/Abstractions/`)
- **Audio Capture**: Dual-stream WASAPI (Windows) & CoreAudio/ALSA (macOS/Linux) with RMS ducking
- **Transcription**: `Whisper.net` (C# wrapper for whisper.cpp with CUDA/Metal/Vulkan acceleration)
- **Local LLMs**: Ollama API (`llama3`, `phi3`, `deepseek-r1`)
- **Database**: SQLite (`Microsoft.Data.Sqlite`)

## Essential Development Commands

### Building the Solution
```bash
# Build the entire solution (.NET 10)
dotnet build SafeScribe.slnx
```

### Running the App
```bash
# Run the cross-platform Avalonia UI application
dotnet run --project SafeScribe.UI.Avalonia/SafeScribe.UI.Avalonia.csproj

# Windows 1-Click Batch Runner
.\Run-SafeScribe-Avalonia.bat
```

### Running Tests
```bash
# Run unit tests across SafeScribe.Core
dotnet test
```

## High-Level Architecture & Decoupling

```
SafeScribe.slnx
├── SafeScribe.Core (Standalone .NET 10 Engine & Domain Library - 0 UI Dependencies)
│   ├── Abstractions/        # Clean Interface Contracts (IAudioCaptureService, ISpeechToTextService, ILlmService, IStorageService, IEngineCoordinator)
│   ├── Audio/               # Dual-stream WASAPI & platform capture
│   ├── SpeechToText/        # Whisper.net engine integration
│   ├── Ollama/              # Ollama API client & prompt template engine
│   ├── Diarization/         # Speaker Diarization logic ("Who said what")
│   ├── Data/                # SQLite local persistence (AppDbContext)
│   └── Exporters/           # Obsidian MD, Slack/Teams, Notion, ICS Calendar export
│
└── SafeScribe.UI.Avalonia (Cross-Platform Desktop Frontend - Pure Presentation Layer)
    ├── Views/               # Avalonia XAML Views (MainWindow)
    ├── ViewModels/          # Reactive MVVM ViewModels consuming IEngineCoordinator
    └── Styles/              # Modern Dark Fluent Design Theme (~40MB RAM footprint)
```

## Critical Guidelines
1. **Zero Cloud Leakage**: Ensure all speech-to-text and LLM operations remain strictly local.
2. **UI Decoupling**: Keep `SafeScribe.Core` 100% free of UI framework imports (Avalonia, WPF, etc.). All UI components must interact with `SafeScribe.Core` exclusively through interface contracts (`SafeScribe.Core.Abstractions`).
3. **Low Memory Overhead**: Preserve low RAM consumption so 95%+ of system memory remains available for local Whisper and Ollama AI models.
