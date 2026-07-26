Add-Type -AssemblyName System.Speech
$synth = New-Object System.Speech.Synthesis.SpeechSynthesizer
$outputPath = Join-Path $PSScriptRoot "sample_meeting.wav"
$synth.SetOutputToWaveFile($outputPath)

$text = @"
Welcome everyone to the SafeScribe AI project kickoff meeting. 
Today we are testing privacy-first meeting transcription and automated AI summary generation.
Primary objectives:
1. Verify local Whisper speech recognition and WASAPI audio capture.
2. Test export options including Obsidian markdown notes, Slack integration, and Outlook calendar events.
3. Ensure 100 percent data privacy with zero data leaving the local machine.

Action Items:
- Piyush will test the audio import and summary features.
- Team will benchmark model performance on 8 gigabyte RAM systems.
"@

$synth.Speak($text)
$synth.Dispose()
Write-Host "Generated sample meeting audio at: $outputPath"
