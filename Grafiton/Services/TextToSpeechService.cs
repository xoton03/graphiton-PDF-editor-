using System;
using System.Speech.Synthesis;

namespace Grafiton.Services;

public class TextToSpeechService : ITextToSpeechService, IDisposable
{
    private readonly SpeechSynthesizer _synthesizer;

    public bool IsSpeaking => _synthesizer.State == SynthesizerState.Speaking;
    public bool IsPaused => _synthesizer.State == SynthesizerState.Paused;

    public event EventHandler? SpeechCompleted;

    public TextToSpeechService()
    {
        _synthesizer = new SpeechSynthesizer();
        _synthesizer.SpeakCompleted += (s, e) => SpeechCompleted?.Invoke(this, EventArgs.Empty);
    }

    public void Speak(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

        if (_synthesizer.State == SynthesizerState.Paused)
        {
            _synthesizer.Resume();
            return;
        }

        _synthesizer.SpeakAsyncCancelAll();
        _synthesizer.SpeakAsync(text);
    }

    public void Pause()
    {
        if (_synthesizer.State == SynthesizerState.Speaking)
        {
            _synthesizer.Pause();
        }
    }

    public void Resume()
    {
        if (_synthesizer.State == SynthesizerState.Paused)
        {
            _synthesizer.Resume();
        }
    }

    public void Stop()
    {
        _synthesizer.SpeakAsyncCancelAll();
    }

    public void SetRate(int rate)
    {
        _synthesizer.Rate = Math.Clamp(rate, -10, 10);
    }

    public void Dispose()
    {
        _synthesizer.Dispose();
    }
}
