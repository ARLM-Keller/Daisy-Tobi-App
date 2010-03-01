﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using AudioLib;
using Microsoft.Practices.Composite.Logging;
using Tobi.Common;
using Tobi.Common.MVVM;
using Tobi.Common.MVVM.Command;
using Tobi.Common.UI;
using urakawa.core;
using urakawa.media.data.audio;
using System.Diagnostics;

namespace Tobi.Plugin.AudioPane
{
    public partial class AudioPaneViewModel
    {
        #region Audio Player


        public RichDelegateCommand CommandAutoPlay { get; private set; }

        public RichDelegateCommand CommandPlay { get; private set; }
        public RichDelegateCommand CommandPlayPreviewLeft { get; private set; }
        public RichDelegateCommand CommandPlayPreviewRight { get; private set; }
        public RichDelegateCommand CommandPause { get; private set; }

        public RichDelegateCommand CommandPlaybackRateDown { get; private set; }
        public RichDelegateCommand CommandPlaybackRateUp { get; private set; }
        public RichDelegateCommand CommandPlaybackRateReset { get; private set; }

        public int AudioPlayer_RefreshInterval
        {
            get { return m_Player.RefreshInterval; }
        }

        private const float PLAYBACK_RATE_MIN = 1f;
        private const float PLAYBACK_RATE_MAX = 2.5f;
        private const float PLAYBACK_RATE_STEP = 0.25f;

        private void initializeCommands_Player()
        {
            CommandPlaybackRateReset = new RichDelegateCommand(
                  Tobi_Plugin_AudioPane_Lang.Audio_PlayRateReset,
                  Tobi_Plugin_AudioPane_Lang.Audio_PlayRateReset_,
                  null, // KeyGesture obtained from settings (see last parameters below)
                  m_ShellView.LoadGnomeGionIcon("Gion_go-previous"),
                  () =>
                  {
                      Logger.Log("AudioPaneViewModel.CommandPlaybackRateReset", Category.Debug, Priority.Medium);

                      PlaybackRate = PLAYBACK_RATE_MIN;
                  },
                  () => !IsWaveFormLoading,
                   Settings_KeyGestures.Default,
                   PropertyChangedNotifyBase.GetMemberName(() => Settings_KeyGestures.Default.Keyboard_Audio_PlaybackRateReset));

            m_ShellView.RegisterRichCommand(CommandPlaybackRateReset);
            //
            CommandPlaybackRateDown = new RichDelegateCommand(
               Tobi_Plugin_AudioPane_Lang.Audio_PlayRateDown,
               Tobi_Plugin_AudioPane_Lang.Audio_PlayRateDown_,
               null, // KeyGesture obtained from settings (see last parameters below)
               m_ShellView.LoadGnomeGionIcon("Gion_go-down"),
               () =>
               {
                   Logger.Log("AudioPaneViewModel.CommandPlaybackRateDown", Category.Debug, Priority.Medium);

                   if ((PlaybackRate - PLAYBACK_RATE_STEP) >= PLAYBACK_RATE_MIN)
                       PlaybackRate -= PLAYBACK_RATE_STEP;
                   else
                   {
                       PlaybackRate = PLAYBACK_RATE_MIN;
                       Debug.Fail("This should never happen !");
                   }
               },
               () => !IsWaveFormLoading && (PlaybackRate - PLAYBACK_RATE_STEP) >= PLAYBACK_RATE_MIN,
                Settings_KeyGestures.Default,
                PropertyChangedNotifyBase.GetMemberName(() => Settings_KeyGestures.Default.Keyboard_Audio_PlaybackRateDown));

            m_ShellView.RegisterRichCommand(CommandPlaybackRateDown);
            //
            CommandPlaybackRateUp = new RichDelegateCommand(
               Tobi_Plugin_AudioPane_Lang.Audio_PlayRateUp,
               Tobi_Plugin_AudioPane_Lang.Audio_PlayRateUp_,
               null, // KeyGesture obtained from settings (see last parameters below)
               m_ShellView.LoadGnomeGionIcon("Gion_go-up"),
               () =>
               {
                   Logger.Log("AudioPaneViewModel.CommandPlaybackRateUp", Category.Debug, Priority.Medium);

                   if ((PlaybackRate + PLAYBACK_RATE_STEP) <= PLAYBACK_RATE_MAX)
                       PlaybackRate += PLAYBACK_RATE_STEP;
                   else
                   {
                       PlaybackRate = PLAYBACK_RATE_MAX;
                       Debug.Fail("This should never happen !");
                   }
               },
               () => !IsWaveFormLoading && (PlaybackRate + PLAYBACK_RATE_STEP) <= PLAYBACK_RATE_MAX,
                Settings_KeyGestures.Default,
                PropertyChangedNotifyBase.GetMemberName(() => Settings_KeyGestures.Default.Keyboard_Audio_PlaybackRateUp));

            m_ShellView.RegisterRichCommand(CommandPlaybackRateUp);
            //
            CommandAutoPlay = new RichDelegateCommand(
               Tobi_Plugin_AudioPane_Lang.Audio_AutoPlay,
               Tobi_Plugin_AudioPane_Lang.Audio_AutoPlay_,
               null, // KeyGesture obtained from settings (see last parameters below)
               m_ShellView.LoadTangoIcon("applications-multimedia"),
               () =>
               {
                   Logger.Log("AudioPaneViewModel.CommandAutoPlay", Category.Debug, Priority.Medium);

                   IsAutoPlay = !IsAutoPlay;
               },
               () => !IsWaveFormLoading,
                Settings_KeyGestures.Default,
                PropertyChangedNotifyBase.GetMemberName(() => Settings_KeyGestures.Default.Keyboard_Audio_AutoPlay));

            m_ShellView.RegisterRichCommand(CommandAutoPlay);
            //
            //
            CommandPause = new RichDelegateCommand(
                Tobi_Plugin_AudioPane_Lang.Audio_Pause,
                Tobi_Plugin_AudioPane_Lang.Audio_Pause_,
                null, // KeyGesture obtained from settings (see last parameters below)
                m_ShellView.LoadTangoIcon("media-playback-pause"),
                () =>
                {
                    Logger.Log("AudioPaneViewModel.CommandPause", Category.Debug, Priority.Medium);

                    AudioPlayer_Stop();
                },
                () => !IsWaveFormLoading && IsAudioLoaded && IsPlaying,
                Settings_KeyGestures.Default,
                PropertyChangedNotifyBase.GetMemberName(() => Settings_KeyGestures.Default.Keyboard_Audio_PlayPause));

            m_ShellView.RegisterRichCommand(CommandPause);
            //
            CommandPlay = new RichDelegateCommand(
                Tobi_Plugin_AudioPane_Lang.Audio_Play,
                Tobi_Plugin_AudioPane_Lang.Audio_Play_,
                null, // KeyGesture obtained from settings (see last parameters below)
                m_ShellView.LoadTangoIcon("media-playback-start"),
                () =>
                {
                    Logger.Log("AudioPaneViewModel.CommandPlay", Category.Debug, Priority.Medium);

                    AudioPlayer_Stop();

                    if (!IsSelectionSet)
                    {
                        if (LastPlayHeadTime >=
                                State.Audio.ConvertBytesToMilliseconds(
                                                    State.Audio.DataLength))
                        {
                            //LastPlayHeadTime = 0; infinite loop !
                            AudioPlayer_PlayFromTo(0, -1);
                        }
                        else
                        {
                            AudioPlayer_PlayFromTo(State.Audio.ConvertMillisecondsToBytes(LastPlayHeadTime), -1);
                        }
                    }
                    else
                    {
                        long byteSelectionLeft = State.Audio.ConvertMillisecondsToBytes(State.Selection.SelectionBegin);
                        long byteSelectionRight = State.Audio.ConvertMillisecondsToBytes(State.Selection.SelectionEnd);

                        long byteLastPlayHeadTime = State.Audio.ConvertMillisecondsToBytes(LastPlayHeadTime);

                        if (byteLastPlayHeadTime >= byteSelectionLeft
                                && byteLastPlayHeadTime < byteSelectionRight)
                        {
                            //if (verifyBeginEndPlayerValues(byteLastPlayHeadTime, byteSelectionRight))
                            //{
                            //}
                            AudioPlayer_PlayFromTo(byteLastPlayHeadTime, byteSelectionRight);
                        }
                        else
                        {
                            //if (verifyBeginEndPlayerValues(byteSelectionLeft, byteSelectionRight))
                            //{
                            //}
                            AudioPlayer_PlayFromTo(byteSelectionLeft, byteSelectionRight);
                        }
                    }
                },
                () => !IsWaveFormLoading && IsAudioLoaded && !IsPlaying && !IsMonitoring && !IsRecording,
                Settings_KeyGestures.Default,
                PropertyChangedNotifyBase.GetMemberName(() => Settings_KeyGestures.Default.Keyboard_Audio_PlayPause));

            m_ShellView.RegisterRichCommand(CommandPlay);
            //
            CommandPlayPreviewLeft = new RichDelegateCommand(
                Tobi_Plugin_AudioPane_Lang.Audio_PlayPreviewLeft,
                Tobi_Plugin_AudioPane_Lang.Audio_PlayPreviewLeft_,
                null, // KeyGesture obtained from settings (see last parameters below)
                ScalableGreyableImageProvider.ConvertIconFormat((DrawingImage)Application.Current.FindResource("Horizon_Image_Left")),
                () => PlayPreviewLeftRight(true),
                () => CommandPlay.CanExecute(),
                Settings_KeyGestures.Default,
                PropertyChangedNotifyBase.GetMemberName(() => Settings_KeyGestures.Default.Keyboard_Audio_PlayPreviewLeft));

            m_ShellView.RegisterRichCommand(CommandPlayPreviewLeft);
            //
            CommandPlayPreviewRight = new RichDelegateCommand(
                Tobi_Plugin_AudioPane_Lang.Audio_PlayPreviewRight,
                Tobi_Plugin_AudioPane_Lang.Audio_PlayPreviewRight_,
                null, // KeyGesture obtained from settings (see last parameters below)
                ScalableGreyableImageProvider.ConvertIconFormat((DrawingImage)Application.Current.FindResource("Horizon_Image_Right")),
                () => PlayPreviewLeftRight(false),
                () => CommandPlay.CanExecute(),
                Settings_KeyGestures.Default,
                PropertyChangedNotifyBase.GetMemberName(() => Settings_KeyGestures.Default.Keyboard_Audio_PlayPreviewRight));

            m_ShellView.RegisterRichCommand(CommandPlayPreviewRight);
            //
        }

        private void PlayPreviewLeftRight(bool left)
        {
            Logger.Log(String.Format("AudioPaneViewModel.CommandPlayPreview ({0})",
                              (left ? "before" : "after")), Category.Debug, Priority.Medium);

            AudioPlayer_Stop();

            double from = 0;
            double to = 0;
            if (left)
            {
                from = Math.Max(0, LastPlayHeadTime - m_TimePreviewPlay);
                to = LastPlayHeadTime;
            }
            else
            {
                from = LastPlayHeadTime;
                to = Math.Min(State.Audio.ConvertBytesToMilliseconds(State.Audio.DataLength), LastPlayHeadTime + m_TimePreviewPlay);
            }

            long byteLeft = State.Audio.ConvertMillisecondsToBytes(from);
            long byteRight = State.Audio.ConvertMillisecondsToBytes(to);

            if (byteRight == byteLeft)
            {
                return;
            }

            //if (verifyBeginEndPlayerValues(byteLeft, byteRight))
            //{
            //}
            AudioPlayer_PlayFromTo(byteLeft, byteRight);

            State.Audio.EndOffsetOfPlayStream = left ? byteRight : byteLeft;
        }

        //private long m_StreamRiffHeaderEndPos;

        private AudioPlayer m_Player;

        // A pointer to a function that returns a stream of PCM data,
        // not including the PCM format RIFF header.
        // The function also calculates the initial StateData
        private AudioPlayer.StreamProviderDelegate m_CurrentAudioStreamProvider;
        private AudioPlayer.StreamProviderDelegate m_AudioStreamProvider_TreeNode;
        private AudioPlayer.StreamProviderDelegate m_AudioStreamProvider_File;

        [NotifyDependsOn("IsRecording")]
        [NotifyDependsOn("IsMonitoring")]
        public bool CanSwapOutputDevice
        {
            get
            {
                return !IsMonitoring && !IsRecording;
            }
        }

        [NotifyDependsOn("IsRecording")]
        [NotifyDependsOn("IsMonitoring")]
        public bool CanSwapInputDevice
        {
            get
            {
                return !IsMonitoring && !IsRecording;
            }
        }

        public List<OutputDevice> OutputDevices
        {
            get
            {
                return m_Player.OutputDevices;
            }
        }
        public OutputDevice OutputDevice
        {
            get
            {
                return m_Player.OutputDevice;
            }
            set
            {
                if (value != null && m_Player.OutputDevice != value)
                {
                    double time = -1;
                    if (m_Player.CurrentState == AudioPlayer.State.Playing)
                    {
                        time = m_Player.CurrentTime;
                        AudioPlayer_Stop();
                    }

                    m_Player.SetOutputDevice(GetWindowsFormsHookControl(), value);
                    //m_Player.OutputDevice = value;

                    if (time >= 0 && State.Audio.HasContent)
                    {
                        AudioPlayer_PlayFromTo(State.Audio.ConvertMillisecondsToBytes(time), -1);
                    }
                }
            }
        }

        private void checkAndDoAutoPlay()
        {
            if (IsAutoPlay
                && (m_Player.CurrentState == AudioPlayer.State.Paused
                || m_Player.CurrentState == AudioPlayer.State.Stopped))
            {
                Logger.Log("AudioPaneViewModel.checkAndDoAutoPlay", Category.Debug, Priority.Medium);

                CommandPlay.Execute();
            }
        }

        [NotifyDependsOn("PlaybackRate")]
        public string PlaybackRateString
        {
            get
            {
                if (IsMonitoring || IsRecording) return "";
                return "Playback x" + PlaybackRate;       // TODO Localize  PlaybackX
            }
        }

        public float PlaybackRate
        {
            get
            {
                return m_Player.FastPlayFactor;
            }
            set
            {
                if (m_Player.FastPlayFactor == value) return;
                m_Player.FastPlayFactor = value;

                RaisePropertyChanged(() => PlaybackRate);
            }
        }

        // ReSharper disable RedundantDefaultFieldInitializer
        private bool m_IsAutoPlay = false;
        // ReSharper restore RedundantDefaultFieldInitializer
        public bool IsAutoPlay
        {
            get
            {
                return m_IsAutoPlay;
            }
            set
            {
                if (m_IsAutoPlay == value) return;
                m_IsAutoPlay = value;

                RaisePropertyChanged(() => IsAutoPlay);
            }
        }

        public bool IsPlaying
        {
            get
            {
                return (m_Player.CurrentState == AudioPlayer.State.Playing);
            }
        }

        [NotifyDependsOn("IsAudioLoaded")]
        [NotifyDependsOnEx("CurrentTreeNode", typeof(StateData))]
        [NotifyDependsOnEx("PlayStream", typeof(StreamStateData))]
        public bool IsAudioLoadedWithSubTreeNodes
        {
            get
            {
                return (IsAudioLoaded
                    && State.CurrentTreeNode != null
                    && State.Audio.PlayStreamMarkers.Count > 1);
            }
        }

        [NotifyDependsOnEx("PlayStream", typeof(StreamStateData))]
        public bool IsAudioLoaded
        {
            get
            {
                return State.Audio.HasContent; // && (View == null || View.BytesPerPixel > 0);
            }
        }

        private void loadAndPlay()
        {
            //Logger.Log("AudioPaneViewModel.loadAndPlay", Category.Debug, Priority.Medium);

            if (View != null)
            {
                View.RefreshUI_PeakMeterBlackout(false);
            }

            PeakOverloadCountCh1 = 0;
            PeakOverloadCountCh2 = 0;

            //StartWaveFormLoadTimer(0, IsAutoPlay);

            m_ForcePlayAfterWaveFormLoaded = IsAutoPlay;
            AudioPlayer_LoadWaveForm(m_ForcePlayAfterWaveFormLoaded);
        }

        public void RefreshWaveFormChunkMarkers()
        {
            if (State.CurrentTreeNode == null || !State.Audio.HasContent)
            {
                return;
            }

            //Logger.Log("AudioPaneViewModel.RefreshWaveFormChunkMarkers", Category.Debug, Priority.Medium);

            long byteOffset = State.Audio.ConvertMillisecondsToBytes(LastPlayHeadTime);

            long bytesRight = 0;
            long bytesLeft = 0;
            int index = -1;
            TreeNode subTreeNode = null;
            foreach (TreeNodeAndStreamDataLength marker in State.Audio.PlayStreamMarkers)
            {
                index++;
                bytesRight += marker.m_LocalStreamDataLength;
                if (byteOffset < bytesRight
                    || index == (State.Audio.PlayStreamMarkers.Count - 1) && byteOffset >= bytesRight)
                {
                    subTreeNode = marker.m_TreeNode;
                    break;
                }
                bytesLeft = bytesRight;
            }

            if (View != null) // && subTreeNode != CurrentSubTreeNode
            {
                View.RefreshUI_WaveFormChunkMarkers(bytesLeft, bytesRight);
            }
        }

        public void AudioPlayer_UpdateWaveFormPlayHead()
        {
            if (!State.Audio.HasContent)
            {
                if (View != null)
                {
                    View.RefreshUI_WaveFormPlayHead();
                }
                return;
            }

            double time = LastPlayHeadTime;
            if (m_Player.CurrentState == AudioPlayer.State.Playing
                || m_Player.CurrentState == AudioPlayer.State.Paused)
            {
                time = m_Player.CurrentTime;
            }
            else if (m_Player.CurrentState == AudioPlayer.State.Stopped && time < 0)
            {
                time = 0;
            }

            if (time == LastPlayHeadTime)
            {
                if (View != null)
                {
                    View.RefreshUI_WaveFormPlayHead();
                }
                RefreshWaveFormChunkMarkersForCurrentSubTreeNode(false);
            }
            else if (time >= 0)
            {
                LastPlayHeadTime = time;
            }
        }

        private bool m_IsWaveFormLoading;
        public bool IsWaveFormLoading
        {
            get
            {
                return m_IsWaveFormLoading;
            }
            set
            {
                if (value != m_IsWaveFormLoading)
                {
                    m_IsWaveFormLoading = value;

                    if (m_IsWaveFormLoading)
                    {
                        EventAggregator.GetEvent<StatusBarMessageUpdateEvent>().Publish("Loading waveform..."); // TODO Localize LoadingWaveform
                    }
                    else
                    {
                        EventAggregator.GetEvent<StatusBarMessageUpdateEvent>().Publish("Waveform loaded.");  // TODO Localize WaveformLoaded
                    }

                    RaisePropertyChanged(() => IsWaveFormLoading);

                    // Manually forcing the commands to refresh their "canExecute" state
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }


        private DispatcherTimer m_WaveFormLoadTimer;

        // ReSharper disable RedundantDefaultFieldInitializer
        private bool m_ForcePlayAfterWaveFormLoaded = false;
        // ReSharper restore RedundantDefaultFieldInitializer

        private static readonly Object LOCK = new Object();

        private void StartWaveFormLoadTimer(double delay, bool play)
        {
            if (IsWaveFormLoading)
            {
                return;
            }

            lock (LOCK)
            {
                m_ForcePlayAfterWaveFormLoaded = play;

                if (View != null)
                {
                    View.ShowHideWaveFormLoadingMessage(true);
                }

                if (m_WaveFormLoadTimer == null)
                {
                    m_WaveFormLoadTimer = new DispatcherTimer(DispatcherPriority.Normal);
                    m_WaveFormLoadTimer.Tick += OnWaveFormLoadTimerTick;
                    // ReSharper disable ConvertIfStatementToConditionalTernaryExpression
                    if (delay == 0)
                    // ReSharper restore ConvertIfStatementToConditionalTernaryExpression
                    {
                        m_WaveFormLoadTimer.Interval = TimeSpan.FromMilliseconds(0);
                        //TODO: does this work ?? (immediate dispatch)
                    }
                    else
                    {
                        m_WaveFormLoadTimer.Interval = TimeSpan.FromMilliseconds(delay);
                    }
                }
                else if (m_WaveFormLoadTimer.IsEnabled)
                {
                    //Logger.Log("m_WaveFormLoadTimer.Stop()", Category.Debug, Priority.Medium);

                    m_WaveFormLoadTimer.Stop();
                }

                //Logger.Log("m_WaveFormLoadTimer.Start()", Category.Debug, Priority.Medium);

                m_WaveFormLoadTimer.Start();
            }
        }

        private void OnWaveFormLoadTimerTick(object sender, EventArgs e)
        {
            //Logger.Log("m_WaveFormLoadTimer.Stop()", Category.Debug, Priority.Medium);

            m_WaveFormLoadTimer.Stop();

            if (IsWaveFormLoading)
            {
                return;
            }

            AudioPlayer_LoadWaveForm(m_ForcePlayAfterWaveFormLoaded);
        }

        public void AudioPlayer_LoadWaveForm(bool play)
        {
            //Logger.Log("AudioPaneViewModel.AudioPlayer_LoadWaveForm", Category.Debug, Priority.Medium);

            if (String.IsNullOrEmpty(State.FilePath) && State.CurrentTreeNode == null)
            {
                if (View != null)
                {
                    View.ShowHideWaveFormLoadingMessage(false);
                }
                return;
            }

            bool wasPlaying = (m_Player.CurrentState == AudioPlayer.State.Playing);

            if (wasPlaying)
            {
                m_Player.Pause();
            }

            if (m_CurrentAudioStreamProvider() == null)
            {
                if (View != null)
                {
                    View.ShowHideWaveFormLoadingMessage(false);
                }
                return;
            }
            // else: the stream is now open

            if (View != null)
            {
                View.ShowHideWaveFormLoadingMessage(true);
            }

            if (View != null)
            {
                View.RefreshUI_LoadWaveForm(wasPlaying, play);
            }
            else
            {
                AudioPlayer_PlayAfterWaveFormLoaded(wasPlaying, play);
            }
        }

        struct StateToRestore
        {
            public double SelectionBegin;
            public double SelectionEnd;
            public double LastPlayHeadTime;
        }
        private StateToRestore? m_StateToRestore = null;

        public void AudioPlayer_PlayAfterWaveFormLoaded(bool wasPlaying, bool play)
        {
            if (m_StateToRestore != null)
            {
                LastPlayHeadTime = m_StateToRestore.GetValueOrDefault().LastPlayHeadTime;

                double begin = m_StateToRestore.GetValueOrDefault().SelectionBegin;
                double end = m_StateToRestore.GetValueOrDefault().SelectionEnd;

                if (begin >= 0 && end >= 0)
                {
                    State.Selection.SetSelectionTime(begin, end);
                }

                m_StateToRestore = null;
            }

            // ensure the stream is closed before we resume the player
            //m_PlayStream.Close();
            //m_PlayStream = null;

            if (wasPlaying)
            {
                m_Player.Resume();
                return;

                /*
                if (!play)
                {
                    m_Player.Resume();
                    return;
                }
                m_Player.Stop();
                 * */
            }

            if (play)
            {
                m_Player.PlayBytes(m_CurrentAudioStreamProvider,
                              State.Audio.DataLength,
                              State.Audio.PcmFormat.Copy().Data,
                              -1, -1);
            }
            else
            {
                AudioPlayer_UpdateWaveFormPlayHead();
            }
        }

        /// <summary>
        /// If player exists and is playing, then pause. Otherwise if paused or stopped, then plays.
        /// </summary>
        public void AudioPlayer_TogglePlayPause()
        {
            //Logger.Log("AudioPaneViewModel.AudioPlayer_TogglePlayPause", Category.Debug, Priority.Medium);

            if (!State.Audio.HasContent)
            {
                return;
            }

            if (m_Player.CurrentState == AudioPlayer.State.Playing)
            {
                m_Player.Pause();
            }
            else if (m_Player.CurrentState == AudioPlayer.State.Paused || m_Player.CurrentState == AudioPlayer.State.Stopped)
            {
                m_Player.Resume();
            }
        }

        /// <summary>
        /// If player exists, resumes or start playing at the given byte offset in the audio stream,
        /// and ends playback at the specified offset.
        /// </summary>
        /// <param name="bytesStart"></param>
        /// <param name="bytesEnd"></param>
        public void AudioPlayer_PlayFromTo(long bytesStart, long bytesEnd)
        {
            //Logger.Log("AudioPaneViewModel.AudioPlayer_PlayFromTo", Category.Debug, Priority.Medium);

            if (!State.Audio.HasContent)
            {
                return;
            }

            State.Audio.EndOffsetOfPlayStream = State.Audio.DataLength;

            if (m_Player.CurrentState == AudioPlayer.State.Paused)
            {
                m_Player.Stop();
            }

            if (bytesEnd < 0)
            {
                if (m_Player.CurrentState == AudioPlayer.State.Playing)
                {
                    m_Player.Stop();
                }

                if (m_Player.CurrentState == AudioPlayer.State.Stopped)
                {
                    if (m_CurrentAudioStreamProvider() == null)
                    {
                        return;
                    }
                    // else: the stream is now open

                    State.Audio.EndOffsetOfPlayStream = State.Audio.DataLength;
                    
                    m_Player.PlayBytes(m_CurrentAudioStreamProvider,
                                  State.Audio.DataLength,
                                  State.Audio.PcmFormat.Copy().Data,
                                  bytesStart,
                                  -1
                        );
                }
                //else if (m_Player.CurrentState == AudioPlayer.State.Playing)
                //{
                //    m_Player.CurrentTime = State.Audio.ConvertBytesToMilliseconds(bytesStart);
                //}
            }
            else
            {
                State.Audio.EndOffsetOfPlayStream = bytesEnd;

                if (m_Player.CurrentState == AudioPlayer.State.Playing)
                {
                    m_Player.Stop();
                }

                if (m_CurrentAudioStreamProvider() == null)
                {
                    return;
                }
                // else: the stream is now open

                m_Player.PlayBytes(m_CurrentAudioStreamProvider,
                              State.Audio.DataLength,
                              State.Audio.PcmFormat.Copy().Data,
                              bytesStart,
                              bytesEnd
                    );
            }

            EventAggregator.GetEvent<StatusBarMessageUpdateEvent>().Publish("Playing..."); // TODO Localize Playing

            //AudioPlayer_UpdateWaveFormPlayHead(); rounding problems between player.currentTime and playheadtime => let's let the vumeter callback do the refresh.
        }

        //private bool verifyBeginEndPlayerValues(double begin, double end)
        //{
        //    double from = State.Audio.ConvertBytesToMilliseconds(begin);
        //    double to = State.Audio.ConvertBytesToMilliseconds(end);

        //    PCMFormatInfo pcmInfo = State.Audio.PcmFormat;
        //    if (pcmInfo == null)
        //    {
        //        pcmInfo = m_PcmFormatOfAudioToInsert;
        //    }

        //    long startPosition = 0;
        //    if (from > 0)
        //    {
        //        startPosition = CalculationFunctions.ConvertTimeToByte(from, (int)pcmInfo.SampleRate, pcmInfo.BlockAlign);
        //        startPosition = CalculationFunctions.AdaptToFrame(startPosition, pcmInfo.BlockAlign);
        //    }
        //    long endPosition = 0;
        //    if (to > 0)
        //    {
        //        endPosition = CalculationFunctions.ConvertTimeToByte(to, (int)pcmInfo.SampleRate, pcmInfo.BlockAlign);
        //        endPosition = CalculationFunctions.AdaptToFrame(endPosition, pcmInfo.BlockAlign);
        //    }
        //    double time = State.Audio.ConvertBytesToMilliseconds(State.Audio.DataLength);
        //    TimeDelta timeD = new TimeDelta(time);
        //    //TimeDelta timeD = pcmInfo.GetDuration(DataLength);
        //    if (startPosition >= 0 &&
        //        (endPosition == 0 || startPosition < endPosition) &&
        //        endPosition <= pcmInfo.GetDataLength(timeD))
        //    {
        //        return true;
        //    }

        //    Debug.Fail("Oops, this should have never happened !");
        //    return false;
        //}

        /// <summary>
        /// If player exists and is ready but is not stopped, then stops.
        /// </summary>
        public void AudioPlayer_Stop()
        {
            //Logger.Log("AudioPaneViewModel.AudioPlayer_Stop", Category.Debug, Priority.Medium);

            if (!State.Audio.HasContent)
            {
                return;
            }

            if (m_Player.CurrentState != AudioPlayer.State.NotReady && m_Player.CurrentState != AudioPlayer.State.Stopped)
            {
                m_Player.Stop();

                EventAggregator.GetEvent<StatusBarMessageUpdateEvent>().Publish("Playback stopped."); // TODO Localize PlaybackStopped
            }
        }

        private double m_TimeStepForwardRewind = 500; // 500ms
        private double m_TimePreviewPlay = 1500; // 1.5s

        private void ensurePlaybackStreamIsDead()
        {
            m_Player.EnsurePlaybackStreamIsDead();
            State.Audio.ResetAll();
        }

        public void AudioPlayer_LoadAndPlayFromFile(string path)
        {
            Logger.Log("AudioPaneViewModel.AudioPlayer_LoadAndPlayFromFile", Category.Debug, Priority.Medium);

            if (m_Player.CurrentState != AudioPlayer.State.NotReady && m_Player.CurrentState != AudioPlayer.State.Stopped)
            {
                m_Player.Stop();
                if (AudioPlaybackStreamKeepAlive)
                {
                    ensurePlaybackStreamIsDead();
                }
            }

            State.ResetAll();

            m_LastPlayHeadTime = -1;
            IsWaveFormLoading = false;

            State.FilePath = path;

            m_CurrentAudioStreamProvider = m_AudioStreamProvider_File;

            if (m_CurrentAudioStreamProvider() == null)
            {
                State.ResetAll();

                m_LastPlayHeadTime = -1;
                IsWaveFormLoading = false;
                return;
            }

            //m_LastPlayHeadTime = 0; Set after waveform loaded

            loadAndPlay();
        }

        //[NotifyDependsOn("IsRecording")]
        //[NotifyDependsOn("IsMonitoring")]
        [NotifyDependsOnEx("PcmFormatRecordingMonitoring", typeof(StreamStateData))]
        [NotifyDependsOnEx("PcmFormat", typeof(StreamStateData))]
        public bool PcmFormatStringVisible
        {
            get
            {
                PCMFormatInfo pcmInfo = State.Audio.GetCurrentPcmFormat();
                return pcmInfo != null;
            }
        }

        //[NotifyDependsOn("IsRecording")]
        //[NotifyDependsOn("IsMonitoring")]
        [NotifyDependsOn("PcmFormatStringVisible")]
        public String PcmFormatString
        {
            get
            {
                PCMFormatInfo pcmInfo = State.Audio.GetCurrentPcmFormat();
                return (pcmInfo != null ? pcmInfo.ToString() : "");
            }
        }

        public Stream AudioPlayer_GetPlayStream()
        {
            return m_CurrentAudioStreamProvider(); // m_PlayStream;
        }

        /*
        public void AudioPlayer_ResetPlayStreamPosition()
        {
            m_PlayStream.Position = m_StreamRiffHeaderEndPos;
            m_PlayStream.Seek(m_StreamRiffHeaderEndPos, SeekOrigin.Begin);
        }*/

        #region Event / Callbacks

        private void OnAudioPlaybackFinished(object sender, AudioPlayer.AudioPlaybackFinishEventArgs e)
        {
            //Logger.Log("AudioPaneViewModel.OnAudioPlaybackFinished", Category.Debug, Priority.Medium);

            RaisePropertyChanged(() => IsPlaying);

            EventAggregator.GetEvent<StatusBarMessageUpdateEvent>().Publish("Playback ended."); // TODO Localize PlaybackEnded

            if (State.Audio.HasContent)
            {
                //double time = m_PcmFormat.GetDuration(m_DataLength).TimeDeltaAsMillisecondDouble;
                //long bytes = (long) m_Player.CurrentTime;

                double time = State.Audio.ConvertBytesToMilliseconds(State.Audio.EndOffsetOfPlayStream);
                //double time = PcmFormat.GetDuration(m_EndOffsetOfPlayStream).TimeDeltaAsMillisecondDouble;

                bool oldVal = IsAutoPlay;
                IsAutoPlay = false;
                LastPlayHeadTime = time;
                IsAutoPlay = oldVal;

                //updateWaveFormPlayHead(time);
            }

            CommandManager.InvalidateRequerySuggested();

            UpdatePeakMeter();

            if (State.CurrentTreeNode != null && State.Audio.EndOffsetOfPlayStream == State.Audio.DataLength && IsAutoPlay)
            {
                TreeNode nextNode = State.CurrentTreeNode.GetNextSiblingWithManagedAudio();
                if (nextNode != null)
                {
                    Logger.Log("-- PublishEvent [TreeNodeSelectedEvent] AudioPaneViewModel.OnAudioPlaybackFinished", Category.Debug, Priority.Medium);

                    EventAggregator.GetEvent<TreeNodeSelectedEvent>().Publish(nextNode);
                }
            }
        }

        private void OnStateChanged_Player(object sender, AudioPlayer.StateChangedEventArgs e)
        {
            //Logger.Log("AudioPaneViewModel.OnStateChanged_Player", Category.Debug, Priority.Medium);

            resetPeakMeter();

            RaisePropertyChanged(() => IsPlaying);

            if (e.OldState == AudioPlayer.State.Playing
                && (m_Player.CurrentState == AudioPlayer.State.Paused
                    || m_Player.CurrentState == AudioPlayer.State.Stopped))
            {
                UpdatePeakMeter();

                if (!AudioPlaybackStreamKeepAlive && m_Player.CurrentState == AudioPlayer.State.Stopped)
                {
                    // stream has been closed already on AudioPlayer side, we're just making sure to reset our cached pointer value.
                    ensurePlaybackStreamIsDead();
                }

                if (View != null)
                {
                    View.StopWaveFormTimer();
                    //View.StopPeakMeterTimer();
                }
            }

            if (m_Player.CurrentState == AudioPlayer.State.Playing)
            {
                if (e.OldState == AudioPlayer.State.Stopped)
                {
                    PeakOverloadCountCh1 = 0;
                    PeakOverloadCountCh2 = 0;
                }
                UpdatePeakMeter();
                if (View != null)
                {
                    View.StartWaveFormTimer();
                    //View.StartPeakMeterTimer();
                }
            }
        }

        //private void OnPlayerResetVuMeter(object sender, UpdateVuMeterEventArgs e)
        //{
        //    resetPeakMeter();
        //}

        #endregion Event / Callbacks

        #endregion Audio Player
    }
}
