using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Grafiton.Services;

namespace Grafiton.ViewModels;

public partial class TextToSpeechViewModel : ObservableObject
{
    private readonly ITextToSpeechService _ttsService;

    [ObservableProperty]
    private string _textToSpeak = string.Empty;

    [ObservableProperty]
    private bool _isPlaying;

    [ObservableProperty]
    private int _speechRate = 0; // -10 to 10

    public TextToSpeechViewModel(ITextToSpeechService ttsService)
    {
        _ttsService = ttsService;
        _ttsService.SpeechCompleted += (s, e) => IsPlaying = false;
    }

    [RelayCommand]
    private void PlayPause()
    {
        if (_ttsService.IsSpeaking && !_ttsService.IsPaused)
        {
            _ttsService.Pause();
            IsPlaying = false;
        }
        else if (_ttsService.IsPaused)
        {
            _ttsService.Resume();
            IsPlaying = true;
        }
        else
        {
            _ttsService.SetRate(SpeechRate);
            _ttsService.Speak(TextToSpeak);
            IsPlaying = true;
        }
    }

    [RelayCommand]
    private void Stop()
    {
        _ttsService.Stop();
        IsPlaying = false;
    }
}
