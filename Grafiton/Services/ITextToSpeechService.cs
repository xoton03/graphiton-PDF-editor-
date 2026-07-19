using System;

namespace Grafiton.Services;

public interface ITextToSpeechService
{
    void Speak(string text);
    void Pause();
    void Resume();
    void Stop();
    void SetRate(int rate); // -10 to 10
    bool IsSpeaking { get; }
    bool IsPaused { get; }
    event EventHandler SpeechCompleted;
}
