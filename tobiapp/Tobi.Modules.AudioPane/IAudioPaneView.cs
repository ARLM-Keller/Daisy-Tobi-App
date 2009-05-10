﻿namespace Tobi.Modules.AudioPane
{
    /// <summary>
    /// Minimal functionality exposed by the View, essentially to be consumed by the ViewModel
    /// in a technology-agnostic manner. In fact, the ViewModel could function without a real View
    /// (i.e the View can be mocked for testing purposes).
    /// </summary>
    public interface IAudioPaneView
    {
// ReSharper disable InconsistentNaming
        void RefreshUI_TimeMessageClear();
        void RefreshUI_TimeMessageInvalidate();
        void RefreshUI_TimeMessageInitiate();
        void RefreshUI_AllReset();
        void RefreshUI_LoadWaveForm();
        void RefreshUI_LoadingMessage(bool visible);
        void RefreshUI_PeakMeter();
        void RefreshUI_PeakMeterBlackout(bool black);
        void RefreshUI_WaveFormBackground();
        void RefreshUI_WaveFormPlayHead();
        void RefreshUI_WaveFormChunkMarkers(long bytesLeft, long bytesRight);
// ReSharper restore InconsistentNaming
        string OpenFileDialog();
        double BytesPerPixel { get; set; }
        void StopWaveFormTimer();
        void StartWaveFormTimer();
        void StopPeakMeterTimer();
        void StartPeakMeterTimer();
        void StartWaveFormLoadTimer(double delay, bool play);
        void ClearSelection();
        void ExpandSelection();
        void ZoomSelection();
        void ZoomFitFull();
        void Refresh();
        void InitGraphicalCommandBindings();
        void SetSelection(double begin, double end);
    }
}
