using System;
using System.IO;
using NAudio.Wave;

namespace SafeScribe.Core.Audio
{
    public class WasapiAudioCapture : IDisposable
    {
        private WasapiLoopbackCapture? _loopbackCapture;
        private WaveFileWriter? _writer;
        private bool _isRecording = false;

        public bool IsRecording => _isRecording;

        public event EventHandler<WaveInEventArgs>? DataAvailable;

        public void StartRecording(string outputFilePath)
        {
            if (_isRecording) return;

            string? dir = Path.GetDirectoryName(outputFilePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            _loopbackCapture = new WasapiLoopbackCapture();
            _writer = new WaveFileWriter(outputFilePath, _loopbackCapture.WaveFormat);

            _loopbackCapture.DataAvailable += (s, e) =>
            {
                if (_writer != null && e.BytesRecorded > 0)
                {
                    _writer.Write(e.Buffer, 0, e.BytesRecorded);
                    DataAvailable?.Invoke(this, e);
                }
            };

            _loopbackCapture.RecordingStopped += (s, e) =>
            {
                _writer?.Flush();
                _writer?.Dispose();
                _writer = null;
            };

            _loopbackCapture.StartRecording();
            _isRecording = true;
        }

        public void StopRecording()
        {
            if (!_isRecording) return;

            _loopbackCapture?.StopRecording();
            _loopbackCapture?.Dispose();
            _loopbackCapture = null;

            _writer?.Flush();
            _writer?.Dispose();
            _writer = null;

            _isRecording = false;
        }

        public void Dispose()
        {
            StopRecording();
        }
    }
}
