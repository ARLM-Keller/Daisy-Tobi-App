﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using System.Windows.Threading;
using AudioLib;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Composite.Logging;
using Tobi.Common;
using Tobi.Common.MVVM;
using Tobi.Common.MVVM.Command;
using urakawa;
using urakawa.command;
using urakawa.core;
using urakawa.daisy.import;
using urakawa.data;
using urakawa.media;
using urakawa.media.data.audio;
using urakawa.media.data.audio.codec;
using urakawa.media.timing;

namespace Tobi.Plugin.AudioPane
{
    ///<summary>
    /// Single shared instance (singleton) of the audio view
    ///</summary>
    [Export(typeof(AudioPaneViewModel)), PartCreationPolicy(CreationPolicy.Shared)]
    public partial class AudioPaneViewModel : ViewModelBase, IPartImportsSatisfiedNotification
    {
#pragma warning disable 1591 // non-documented method
        public void OnImportsSatisfied()
#pragma warning restore 1591
        {
            //#if DEBUG
            //            Debugger.Break();
            //#endif
        }

        #region Construction


        private readonly ILoggerFacade Logger;
        private readonly IEventAggregator EventAggregator;

        private readonly IShellView m_ShellView;
        internal readonly IUrakawaSession m_UrakawaSession;


        ///<summary>
        /// We inject a few dependencies in this constructor.
        /// The Initialize method is then normally called by the bootstrapper of the plugin framework.
        ///</summary>
        ///<param name="logger">normally obtained from the Unity container, it's a built-in CAG service</param>
        ///<param name="eventAggregator">normally obtained from the Unity container, it's a built-in CAG service</param>
        ///<param name="shellView">normally obtained from the Unity container, it's a Tobi built-in type</param>
        ///<param name="session">normally obtained from the Unity container, it's a Tobi built-in type</param>
        [ImportingConstructor]
        public AudioPaneViewModel(
            ILoggerFacade logger,
            IEventAggregator eventAggregator,
            [Import(typeof(IShellView), RequiredCreationPolicy = CreationPolicy.Shared, AllowDefault = false)]
            IShellView shellView,
            [Import(typeof(IUrakawaSession), RequiredCreationPolicy = CreationPolicy.Shared, AllowDefault = false)]
            IUrakawaSession session
            )
        {
            EventAggregator = eventAggregator;
            Logger = logger;

            m_ShellView = shellView;
            m_UrakawaSession = session;

            Initialize();

            m_AudioStreamProvider_File = () =>
            {
                if (State.Audio.PlayStream == null)
                {
                    if (String.IsNullOrEmpty(State.FilePath))
                    {
                        State.ResetAll();
                        return null;
                    }
                    if (!File.Exists(State.FilePath))
                    {
                        State.ResetAll();
                        return null;
                    }
                    try
                    {
                        State.Audio.SetPlayStream_FromFile(File.Open(State.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read), State.FilePath);
                    }
                    catch (Exception ex)
                    {
                        State.ResetAll();

                        m_LastPlayHeadTime = -1;
                        //IsWaveFormLoading = false;
                        return null;
                    }
                }
                return State.Audio.PlayStream;
            };

            m_AudioStreamProvider_TreeNode = () =>
            {
                Tuple<TreeNode, TreeNode> treeNodeSelection = m_UrakawaSession.GetTreeNodeSelection();

                if (treeNodeSelection.Item1 == null) return null;

                if (State.Audio.PlayStream == null)
                {
                    Debug.Assert(State.Audio.PlayStreamMarkers == null);

                    StreamWithMarkers? sm = treeNodeSelection.Item1.OpenPcmInputStreamOfManagedAudioMediaFlattened();

                    if (sm != null)
                    {
                        //    TreeNode ancerstor = State.CurrentTreeNode.GetFirstAncestorWithManagedAudio();
                        //    if (ancerstor != null)
                        //    {
                        //        StreamWithMarkers? sma = ancerstor.OpenPcmInputStreamOfManagedAudioMedia();
                        //        if (sma != null)
                        //        {
                        //            TreeNode theCurrentSubTreeNode = State.CurrentTreeNode;
                        //            State.CurrentTreeNode = ancerstor;

                        //            State.Audio.SetPlayStream_FromTreeNode(sma.GetValueOrDefault().m_Stream);
                        //            State.Audio.PlayStreamMarkers = sma.GetValueOrDefault().m_SubStreamMarkers;

                        //            Logger.Log("-- PublishEvent [TreeNodeSelectedEvent] AudioPaneViewModel.m_CurrentAudioStreamProvider", Category.Debug, Priority.Medium);

                        //            EventAggregator.GetEvent<TreeNodeSelectedEvent>().Publish(State.CurrentTreeNode);

                        //            Logger.Log("-- PublishEvent [SubTreeNodeSelectedEvent] AudioPaneViewModel.m_CurrentAudioStreamProvider", Category.Debug, Priority.Medium);

                        //            EventAggregator.GetEvent<SubTreeNodeSelectedEvent>().Publish(theCurrentSubTreeNode);
                        //        }
                        //    }
                        //}
                        //else
                        //{
                        State.Audio.SetPlayStream_FromTreeNode(sm.GetValueOrDefault().m_Stream);
                        State.Audio.PlayStreamMarkers = sm.GetValueOrDefault().m_SubStreamMarkers;
                    }

                    //if (State.Audio.PlayStream == null)
                    //{
                    //    //State.ResetAll();
                    //    //m_LastPlayHeadTime = -1;
                    //    //IsWaveFormLoading = false;
                    //    return null;
                    //}

                    //if (State.Audio.PlayStreamMarkers.Count == 1)
                    //{
                    //    State.CurrentSubTreeNode = State.CurrentTreeNode;
                    //}
                }

                return State.Audio.PlayStream;
            };

            EventAggregator.GetEvent<StatusBarMessageUpdateEvent>().Subscribe(str => StatusBarMessage = str, StatusBarMessageUpdateEvent.THREAD_OPTION);

            Settings.Default.PropertyChanged += SettingsPropertyChanged;
        }

        private void SettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!e.PropertyName.StartsWith(@"AudioWaveForm_")) return;

            if (e.PropertyName == GetMemberName(() => Settings.Default.AudioWaveForm_Resolution))
            {
                WaveStepX = Settings.Default.AudioWaveForm_Resolution;
            }
            else if (e.PropertyName == GetMemberName(() => Settings.Default.AudioWaveForm_IsFilled))
            {
                IsEnvelopeFilled = Settings.Default.AudioWaveForm_IsFilled;
            }
            else if (e.PropertyName == GetMemberName(() => Settings.Default.AudioWaveForm_IsBordered))
            {
                IsEnvelopeVisible = Settings.Default.AudioWaveForm_IsBordered;
            }
            else if (e.PropertyName == GetMemberName(() => Settings.Default.AudioWaveForm_IsStroked))
            {
                IsWaveFillVisible = Settings.Default.AudioWaveForm_IsStroked;
            }
            else if (e.PropertyName == GetMemberName(() => Settings.Default.AudioWaveForm_Color_Stroke))
            {
                ColorWaveBars = Settings.Default.AudioWaveForm_Color_Stroke;
            }
            else if (e.PropertyName == GetMemberName(() => Settings.Default.AudioWaveForm_Color_Border))
            {
                ColorEnvelopeOutline = Settings.Default.AudioWaveForm_Color_Border;
            }
            else if (e.PropertyName == GetMemberName(() => Settings.Default.AudioWaveForm_Color_Fill))
            {
                ColorEnvelopeFill = Settings.Default.AudioWaveForm_Color_Fill;
            }
            else if (e.PropertyName == GetMemberName(() => Settings.Default.AudioWaveForm_Color_CursorBorder))
            {
                ColorPlayhead = Settings.Default.AudioWaveForm_Color_CursorBorder;
            }
            else if (e.PropertyName == GetMemberName(() => Settings.Default.AudioWaveForm_Color_CursorFill))
            {
                ColorPlayheadFill = Settings.Default.AudioWaveForm_Color_CursorFill;
            }
            else if (e.PropertyName == GetMemberName(() => Settings.Default.AudioWaveForm_Color_Phrases))
            {
                ColorMarkers = Settings.Default.AudioWaveForm_Color_Phrases;
            }
            else if (e.PropertyName == GetMemberName(() => Settings.Default.AudioWaveForm_Color_Selection))
            {
                ColorTimeSelection = Settings.Default.AudioWaveForm_Color_Selection;
            }
            else if (e.PropertyName == GetMemberName(() => Settings.Default.AudioWaveForm_Color_TimeText))
            {
                ColorTimeInfoText = Settings.Default.AudioWaveForm_Color_TimeText;
            }
        }

        //StatusBarMessageUpdateEvent
        private string m_StatusBarMessage;
        public string StatusBarMessage
        {
            private set
            {
                if (m_StatusBarMessage == value)
                {
                    return;
                }
                m_StatusBarMessage = value;

                RaisePropertyChanged(() => StatusBarMessage);
            }
            get { return m_StatusBarMessage; }
        }

        public IInputBindingManager InputBindingManager
        {
            get { return m_ShellView; }
        }

        ~AudioPaneViewModel()
        {
#if DEBUG
            Logger.Log("AudioPaneViewModel garbage collected.", Category.Debug, Priority.Medium);
#endif
            Settings.Default.PropertyChanged -= SettingsPropertyChanged;
        }

        #endregion Construction

        #region Initialization

        protected IAudioPaneView View { get; private set; }
        public void SetView(IAudioPaneView view)
        {
            View = view;
        }

        protected void Initialize()
        {
            State = new StateData(this, this);

            initializeCommands();

            // Removed because called too many times.
            //PropertyChanged += OnViewModelPropertyChanged;

            initializeAudioStuff();

            State.ResetAll();

            m_LastPlayHeadTime = -1;
            //IsWaveFormLoading = false;

            //EventAggregator.GetEvent<UserInterfaceScaledEvent>().Subscribe(OnUserInterfaceScaled, ThreadOption.UIThread);

            EventAggregator.GetEvent<ProjectLoadedEvent>().Subscribe(OnProjectLoaded, ProjectLoadedEvent.THREAD_OPTION);
            EventAggregator.GetEvent<ProjectUnLoadedEvent>().Subscribe(OnProjectUnLoaded, ProjectUnLoadedEvent.THREAD_OPTION);

            //EventAggregator.GetEvent<TreeNodeSelectedEvent>().Subscribe(OnTreeNodeSelected, TreeNodeSelectedEvent.THREAD_OPTION);
            //EventAggregator.GetEvent<SubTreeNodeSelectedEvent>().Subscribe(OnSubTreeNodeSelected, SubTreeNodeSelectedEvent.THREAD_OPTION);

            EventAggregator.GetEvent<TreeNodeSelectionChangedEvent>().Subscribe(OnTreeNodeSelectionChanged, TreeNodeSelectionChangedEvent.THREAD_OPTION);

            EventAggregator.GetEvent<EscapeEvent>().Subscribe(OnEscape, EscapeEvent.THREAD_OPTION);
        }

        private void OnEscape(object obj)
        {
            if (!Dispatcher.CheckAccess())
            {
#if DEBUG
                Debugger.Break();
#endif
                Dispatcher.Invoke(DispatcherPriority.Normal, (Action<object>)OnEscape, obj);
                return;
            }

            if (View != null)
            {
                View.CancelWaveFormLoad(false);
            }
            if (IsPlaying)
            {
                CommandPause.Execute();
                return;
            }
            if (IsMonitoring)
            {
                CommandStopMonitor.Execute();
                return;
            }
            if (IsRecording)
            {
                CommandStopRecord.Execute();
                return;
            }
        }

        private void initializeAudioStuff()
        {
            Logger.Log("AudioPaneViewModel.initializeAudioStuff", Category.Debug, Priority.Medium);

            m_Player = new AudioPlayer(AudioPlaybackStreamKeepAlive);
            m_Player.SetOutputDevice(GetWindowsFormsHookControl(), @"fakename");
            m_Player.StateChanged += OnStateChanged_Player;
            m_Player.AudioPlaybackFinished += OnAudioPlaybackFinished;

            //m_Player.ResetVuMeter += OnPlayerResetVuMeter;

            m_Recorder = new AudioRecorder();
            m_Recorder.SetDevice(@"fakename");
            m_Recorder.StateChanged += OnStateChanged_Recorder;
            m_Recorder.AudioRecordingFinished += OnAudioRecordingFinished;
            m_Recorder.RecordingDirectory = AudioFormatConvertorSession.TEMP_AUDIO_DIRECTORY; // Directory.GetCurrentDirectory();

            //m_Recorder.ResetVuMeter += OnRecorderResetVuMeter;

            m_VuMeter = new VuMeter(m_Player, m_Recorder);
            m_VuMeter.PeakMeterUpdated += OnPeakMeterUpdated;
            m_VuMeter.PeakMeterOverloaded += OnPeakMeterOverloaded;

            PeakMeterBarDataCh1 = new PeakMeterBarData();
            PeakMeterBarDataCh2 = new PeakMeterBarData();
            PeakMeterBarDataCh1.ValueDb = Double.NegativeInfinity;
            PeakMeterBarDataCh2.ValueDb = Double.NegativeInfinity;
            m_PeakMeterValues = new double[2];

            m_AudioFormatConvertorSession_NoProject =
                new AudioFormatConvertorSession(AudioFormatConvertorSession.TEMP_AUDIO_DIRECTORY, null);
        }

        #endregion Initialization

        #region WindowsFormsHookControl (required by DirectSound)

        // ReSharper disable RedundantDefaultFieldInitializer
        private System.Windows.Forms.Control m_WindowsFormsHookControl = null;
        // ReSharper restore RedundantDefaultFieldInitializer
        public System.Windows.Forms.Control GetWindowsFormsHookControl()
        {
            if (m_WindowsFormsHookControl == null)
            {
                Logger.Log("AudioPaneViewModel.GetWindowsFormsHookControl", Category.Debug, Priority.Medium);

                m_WindowsFormsHookControl = new System.Windows.Forms.Control(@"Dummy visual needed by DirectSound");
            }

            return m_WindowsFormsHookControl;
        }

        #endregion WindowsFormsHookControl (required by DirectSound)

        #region Event / Callbacks

        //private bool m_SkipTreeNodeSelectedEvent = false;

        //private void OnSubTreeNodeSelected(TreeNode node)
        //{
        //    if (!Dispatcher.CheckAccess())
        //    {
        //        //Dispatcher.Invoke(DispatcherPriority.Normal, new ThreadStart(RefreshUI_WaveFormChunkMarkers));
        //        Dispatcher.BeginInvoke(DispatcherPriority.Normal,
        //            (Action<TreeNode>)OnSubTreeNodeSelected, node);
        //        return;
        //    }
        //    //Logger.Log("AudioPaneViewModel.OnSubTreeNodeSelected", Category.Debug, Priority.Medium);


        //    if (View != null)
        //    {
        //        View.CancelWaveFormLoad();
        //    }

        //    if (node == null || State.CurrentTreeNode == null)
        //    {
        //        return;
        //    }
        //    if (State.CurrentSubTreeNode == node)
        //    {
        //        return;
        //    }
        //    if (!IsAudioLoadedWithSubTreeNodes)
        //    {
        //        return;
        //    }
        //    if (!node.IsDescendantOf(State.CurrentTreeNode))
        //    {
        //        return;
        //    }

        //    if (m_Player.CurrentState != AudioPlayer.State.NotReady && m_Player.CurrentState != AudioPlayer.State.Stopped)
        //    {
        //        m_Player.Stop();
        //    }

        //    State.CurrentSubTreeNode = node;

        //    RefreshWaveFormChunkMarkersForCurrentSubTreeNode(true);
        //}

        private void RefreshWaveFormChunkMarkersForCurrentSubTreeNode(bool placePlayHead)
        {
            Tuple<TreeNode, TreeNode> treeNodeSelection = m_UrakawaSession.GetTreeNodeSelection();

            TreeNode treeNodeToMatch = treeNodeSelection.Item2 ?? treeNodeSelection.Item1;

            if (!State.Audio.HasContent || treeNodeToMatch == null)
            {
                return;
            }

            long bytesRight;
            long bytesLeft;
            int index;
            bool match = State.Audio.FindInPlayStreamMarkers(treeNodeToMatch, out index, out bytesLeft, out bytesRight);

            if (match)
            {
                if (View != null)
                {
                    View.RefreshUI_WaveFormChunkMarkers(bytesLeft, bytesRight);
                }
                if (placePlayHead)
                {
                    long bytes = State.Audio.ConvertMillisecondsToBytes(LastPlayHeadTime);
                    if (!(
                        bytes >= bytesLeft &&
                        (bytes < bytesRight || index == State.Audio.PlayStreamMarkers.Count - 1 && bytes >= bytesRight)
                        )
                        )
                    {
                        LastPlayHeadTime = State.Audio.ConvertBytesToMilliseconds(bytesLeft);
                    }
                }
            }
        }


        private void OnTreeNodeSelectionChanged(Tuple<Tuple<TreeNode, TreeNode>, Tuple<TreeNode, TreeNode>> oldAndNewTreeNodeSelection)
        {
            //Tuple<TreeNode, TreeNode> treeNodeSelection = m_UrakawaSession.GetTreeNodeSelection();

            if (!Dispatcher.CheckAccess())
            {
#if DEBUG
                Debugger.Break();
#endif
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action<Tuple<Tuple<TreeNode, TreeNode>, Tuple<TreeNode, TreeNode>>>)OnTreeNodeSelectionChanged, oldAndNewTreeNodeSelection);
                return;
            }

            Tuple<TreeNode, TreeNode> oldTreeNodeSelection = oldAndNewTreeNodeSelection.Item1;
            Tuple<TreeNode, TreeNode> newTreeNodeSelection = oldAndNewTreeNodeSelection.Item2;

            bool mustLoadWaveForm = false;

            if (State.Audio.PlayStreamMarkers == null // audio file may have been previously loaded, no relation to the document. 
                || oldTreeNodeSelection.Item1 != newTreeNodeSelection.Item1)
            {
                if (m_Player.CurrentState != AudioPlayer.State.NotReady && m_Player.CurrentState != AudioPlayer.State.Stopped)
                {
                    m_Player.Stop();
                }

                if (View != null)
                {
                    View.CancelWaveFormLoad(true);
                }

                if (AudioPlaybackStreamKeepAlive)
                {
                    ensurePlaybackStreamIsDead();
                }

                State.ResetAll();

                m_LastPlayHeadTime = -1;

                m_CurrentAudioStreamProvider = m_AudioStreamProvider_TreeNode;

                if (m_CurrentAudioStreamProvider() == null)
                {
                    CommandManager.InvalidateRequerySuggested();
                    return;
                }

                m_LastPlayHeadTime = 0;

                mustLoadWaveForm = true;
            }

            if (IsAudioLoadedWithSubTreeNodes
                && oldTreeNodeSelection.Item2 != newTreeNodeSelection.Item2)
            {
                RefreshWaveFormChunkMarkersForCurrentSubTreeNode(true);
            }

            //Logger.Log("AudioPaneViewModel.OnTreeNodeSelected", Category.Debug, Priority.Medium);


            /*if (m_SkipTreeNodeSelectedEvent)
            {
                m_SkipTreeNodeSelectedEvent = false;
                return;
            }*/

            //IsWaveFormLoading = false;

            CommandManager.InvalidateRequerySuggested();

            if (mustLoadWaveForm)
            {
                loadAndPlay();
            }
            else
            {
                checkAndDoAutoPlay();
                //bool wasPlaying = (m_Player.CurrentState == AudioPlayer.State.Playing);
                //AudioPlayer_PlayAfterWaveFormLoaded(wasPlaying, IsAutoPlay);
            }
        }

        //private void OnTreeNodeSelected(TreeNode node)
        //{
        //    if (node == null)
        //    {
        //        return;
        //    }

        //    if (!Dispatcher.CheckAccess())
        //    {
        //        //Dispatcher.Invoke(DispatcherPriority.Normal, new ThreadStart(RefreshUI_WaveFormChunkMarkers));
        //        Dispatcher.BeginInvoke(DispatcherPriority.Normal,
        //            (Action<TreeNode>)OnTreeNodeSelected, node);
        //        return;
        //    }

        //    //Logger.Log("AudioPaneViewModel.OnTreeNodeSelected", Category.Debug, Priority.Medium);


        //    if (View != null)
        //    {
        //        View.CancelWaveFormLoad();
        //    }

        //    if (State.CurrentTreeNode == node)
        //    {
        //        return;
        //    }

        //    /*if (m_SkipTreeNodeSelectedEvent)
        //    {
        //        m_SkipTreeNodeSelectedEvent = false;
        //        return;
        //    }*/

        //    if (m_Player.CurrentState != AudioPlayer.State.NotReady && m_Player.CurrentState != AudioPlayer.State.Stopped)
        //    {
        //        m_Player.Stop();
        //    }

        //    if (AudioPlaybackStreamKeepAlive)
        //    {
        //        ensurePlaybackStreamIsDead();
        //    }

        //    State.ResetAll();

        //    m_LastPlayHeadTime = -1;
        //    IsWaveFormLoading = false;

        //    State.CurrentTreeNode = node;

        //    m_CurrentAudioStreamProvider = m_AudioStreamProvider_TreeNode;

        //    if (m_CurrentAudioStreamProvider() == null)
        //    {
        //        return;
        //    }

        //    //m_LastPlayHeadTime = 0; Set after waveform loaded

        //    loadAndPlay();
        //}


        //private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        //{
        //    if (e.PropertyName.StartsWith("Can"))
        //    {
        //        Logger.Log("@@ AudioPaneViewModel.OnViewModelPropertyChanged: [" + e.PropertyName + "]", Category.Debug, Priority.High);

        //        CommandManager.InvalidateRequerySuggested();
        //    }
        //}

        private AudioFormatConvertorSession m_AudioFormatConvertorSession;
        private AudioFormatConvertorSession m_AudioFormatConvertorSession_NoProject;

        private void OnProjectUnLoaded(Project project)
        {
            if (!Dispatcher.CheckAccess())
            {
#if DEBUG
                Debugger.Break();
#endif
                Dispatcher.Invoke(DispatcherPriority.Normal, (Action<Project>)OnProjectUnLoaded, project);
                return;
            }

            project.Presentations.Get(0).UndoRedoManager.CommandDone -= OnUndoRedoManagerChanged;
            project.Presentations.Get(0).UndoRedoManager.CommandReDone -= OnUndoRedoManagerChanged;
            project.Presentations.Get(0).UndoRedoManager.CommandUnDone -= OnUndoRedoManagerChanged;

            OnProjectLoaded(null);
        }

        private void OnProjectLoaded(Project project)
        {
            if (!Dispatcher.CheckAccess())
            {
#if DEBUG
                Debugger.Break();
#endif
                Dispatcher.Invoke(DispatcherPriority.Normal, (Action<Project>)OnProjectLoaded, project);
                return;
            }

            if (View != null)
            {
                View.CancelWaveFormLoad(true);
            }

            if (AudioPlaybackStreamKeepAlive)
            {
                ensurePlaybackStreamIsDead();
            }

            State.ResetAll();

            AudioClipboard = null;
            m_LastPlayHeadTime = -1;
            //IsWaveFormLoading = false;

            //var shell = Container.Resolve<IShellView>();
            //shell.DocumentProject
            if (project != null)
            {
                m_AudioFormatConvertorSession =
                    new AudioFormatConvertorSession(
                    //AudioFormatConvertorSession.TEMP_AUDIO_DIRECTORY,
                    project.Presentations.Get(0).DataProviderManager.DataFileDirectoryFullPath,
                project.Presentations.Get(0).MediaDataManager.DefaultPCMFormat);

                project.Presentations.Get(0).UndoRedoManager.CommandDone += OnUndoRedoManagerChanged;
                project.Presentations.Get(0).UndoRedoManager.CommandReDone += OnUndoRedoManagerChanged;
                project.Presentations.Get(0).UndoRedoManager.CommandUnDone += OnUndoRedoManagerChanged;
                m_Recorder.RecordingDirectory = project.Presentations.Get(0).DataProviderManager.DataFileDirectoryFullPath; //AudioFormatConvertorSession.TEMP_AUDIO_DIRECTORY

                EventAggregator.GetEvent<StatusBarMessageUpdateEvent>().Publish(Tobi_Plugin_AudioPane_Lang.Ready);
            }
            else
            {
                m_AudioFormatConvertorSession = null;

                m_Recorder.RecordingDirectory = AudioFormatConvertorSession.TEMP_AUDIO_DIRECTORY; // Directory.GetCurrentDirectory();

                EventAggregator.GetEvent<StatusBarMessageUpdateEvent>().Publish("No document."); // TODO Localize NoDocument
            }
        }

        #endregion Event / Callbacks

        #region Private Class Attributes

        private const bool AudioPlaybackStreamKeepAlive = true;

        private double getTimeOffset(TreeNode treeNode, ManagedAudioMedia managedMedia)
        {
            //if (!State.IsTreeNodeShownInAudioWaveForm(treeNode))
            //{
            //    return 0;
            //}

            double timeOffset = 0;

            //Tuple<TreeNode, TreeNode> treeNodeSelection = m_UrakawaSession.GetTreeNodeSelection();

            if (State.Audio.PlayStreamMarkers != null)
            {
                long bytesRight;
                long bytesLeft;
                int index;
                bool match = State.Audio.FindInPlayStreamMarkers(treeNode, out index, out bytesLeft, out bytesRight);

                if (match)
                {
                    timeOffset = State.Audio.ConvertBytesToMilliseconds(bytesLeft);
                }
                else return 0;
            }

            if (managedMedia == null)
            {
                return timeOffset;
            }

            SequenceMedia seqManAudioMedia = treeNode.GetManagedAudioSequenceMedia();
            if (seqManAudioMedia != null)
            {
                Debug.Fail("SequenceMedia is normally removed at import time...have you tried re-importing the DAISY book ?");

                foreach (Media media in seqManAudioMedia.ChildMedias.ContentsAs_YieldEnumerable)
                {
                    var manMedia = (ManagedAudioMedia)media;
                    if (media == managedMedia)
                    {
                        break;
                    }
                    timeOffset += manMedia.Duration.AsMilliseconds;
                }
            }

            return timeOffset;
        }

        #endregion Private Class Attributes

        #region Public Properties

        public bool ResizeDrag
        {
            get
            {
                return m_ShellView.SplitterDrag;
            }
        }

        [NotifyDependsOn("IsAudioLoaded")]
        [NotifyDependsOn("IsSelectionSet")]
        public bool TimeStringSelectionVisible
        {
            get
            {
                return IsAudioLoaded && IsSelectionSet;
            }
        }

        [NotifyDependsOn("TimeStringSelectionVisible")]
        public string TimeStringSelectionStart
        {
            get
            {
                if (!TimeStringSelectionVisible)
                {
                    return "";
                }

                return FormatTimeSpan_Units(State.Selection.SelectionBegin);
            }
        }

        [NotifyDependsOn("TimeStringSelectionVisible")]
        public string TimeStringSelectionEnd
        {
            get
            {
                if (!TimeStringSelectionVisible)
                {
                    return "";
                }

                return FormatTimeSpan_Units(State.Selection.SelectionEnd);
            }
        }

        [NotifyDependsOn("TimeStringSelectionVisible")]
        public string TimeStringSelectionDur
        {
            get
            {
                if (!TimeStringSelectionVisible)
                {
                    return "";
                }

                return FormatTimeSpan_Units(State.Selection.SelectionEnd - State.Selection.SelectionBegin);
            }
        }

        [NotifyDependsOn("IsAudioLoaded")]
        [NotifyDependsOnEx("DataLength", typeof(StreamStateData))]
        public string TimeStringTotalWaveform
        {
            get
            {
                if (!IsAudioLoaded)
                {
                    return "";
                }

                return FormatTimeSpan_Units(State.Audio.ConvertBytesToMilliseconds(State.Audio.DataLength));
            }
        }

        private static readonly PropertyChangedEventArgs m_RecorderCurrentDurationArgs
            = new PropertyChangedEventArgs("RecorderCurrentDuration");
        public double RecorderCurrentDuration
        {
            get
            {
                return m_Recorder.CurrentDuration;
            }
        }

        public static string FormatTimeSpan_Units(double ms)
        {
            TimeSpan time = TimeSpan.FromTicks((long)(ms * TimeSpan.TicksPerMillisecond));

            if (Settings.Default.UseFriendlyTimeFormat)
                return Time.Format_H_MN_S_MS(time);

            return Time.Format_Standard(time);
        }


        [NotifyDependsOn("TimeStringSelectionEnd")]
        [NotifyDependsOn("TimeStringSelectionStart")]
        [NotifyDependsOn("TimeStringSelectionDur")]
        public String TimeStringSelection
        {
            get
            {
                return "Selection: " + TimeStringSelectionStart + " - " + TimeStringSelectionEnd + " (" + TimeStringSelectionDur + ")";
            }
        }

        [NotifyDependsOn("IsMonitoring")]
        [NotifyDependsOn("IsRecording")]
        [NotifyDependsOn("IsAudioLoaded")]
        public bool TimeStringCurrentVisible
        {
            get
            {
                return IsAudioLoaded || IsRecording || IsMonitoring;
            }
        }

        private static readonly PropertyChangedEventArgs m_TimeStringCurrentArgs
        = new PropertyChangedEventArgs("TimeStringCurrent");
        [NotifyDependsOn("IsPlaying")]
        [NotifyDependsOn("TimeStringCurrentVisible")]
        [NotifyDependsOn("TimeStringTotalWaveform")]
        [NotifyDependsOnFast("m_LastPlayHeadTimeArgs", "m_TimeStringCurrentArgs")]
        [NotifyDependsOnFast("m_RecorderCurrentDurationArgs", "m_TimeStringCurrentArgs")]
        public String TimeStringCurrent
        {
            get
            {
                if (IsRecording || IsMonitoring)
                {
                    return FormatTimeSpan_Units(RecorderCurrentDuration);
                }

                string strToDisplay = null;


                if (!IsAudioLoaded)
                {
                    return "";
                }
                else if (IsPlaying)
                {
                    strToDisplay = FormatTimeSpan_Units(m_Player.CurrentTime);
                }
                else if (LastPlayHeadTime >= 0 && (
                                                 m_Player.CurrentState == AudioPlayer.State.Paused ||
                                                 m_Player.CurrentState == AudioPlayer.State.Stopped
                                             ))
                {
                    strToDisplay = FormatTimeSpan_Units(LastPlayHeadTime);
                }

                if (!String.IsNullOrEmpty(strToDisplay))
                {
                    return "Time: " + strToDisplay + " / " + TimeStringTotalWaveform;
                }

                return "";
            }
        }

        private static readonly PropertyChangedEventArgs m_LastPlayHeadTimeArgs
            = new PropertyChangedEventArgs("LastPlayHeadTime");
        private double m_LastPlayHeadTime;
        public double LastPlayHeadTime
        {
            get
            {
                return m_LastPlayHeadTime;
            }
            set
            {
                if (m_LastPlayHeadTime == value)
                {
                    if (View != null)
                    {
                        View.RefreshUI_WaveFormPlayHead();
                    }
                    RefreshWaveFormChunkMarkersForCurrentSubTreeNode(false);
                    return;
                }

                m_LastPlayHeadTime = value;

                if (m_LastPlayHeadTime < 0)
                {
                    Debug.Fail(String.Format("m_LastPlayHeadTime < 0 ?? {0}", m_LastPlayHeadTime));
                    m_LastPlayHeadTime = 0;
                }

                if (State.Audio.HasContent)
                {
                    double time = State.Audio.ConvertBytesToMilliseconds(State.Audio.DataLength);
                    //double time = PcmFormat.GetDuration(DataLength).AsMilliseconds;
                    if (m_LastPlayHeadTime > time)
                    {
                        Debug.Fail(String.Format("m_LastPlayHeadTime > DataLength ?? {0}", m_LastPlayHeadTime));
                        m_LastPlayHeadTime = time;
                    }
                }

                RaisePropertyChanged(m_LastPlayHeadTimeArgs);
                //RaisePropertyChanged(() => LastPlayHeadTime);

                if (View != null)
                {
                    View.RefreshUI_WaveFormPlayHead();
                }

                if (!State.Audio.HasContent)
                {
                    return;
                }
                Tuple<TreeNode, TreeNode> treeNodeSelection = m_UrakawaSession.GetTreeNodeSelection();

                if (treeNodeSelection.Item1 == null)
                {
                    checkAndDoAutoPlay();
                    return;
                }

                //long byteOffset = PcmFormat.GetByteForTime(new Time(LastPlayHeadTime));
                long byteOffset = State.Audio.ConvertMillisecondsToBytes(m_LastPlayHeadTime);

                long bytesRight;
                long bytesLeft;
                int index;
                TreeNode treeNode;
                bool match = State.Audio.FindInPlayStreamMarkers(byteOffset, out treeNode, out index, out bytesLeft, out bytesRight);

                TreeNode treeNodeToMatch = treeNodeSelection.Item2 ?? treeNodeSelection.Item1;


                if (match)
                {
                    if (treeNodeToMatch == treeNode || treeNodeToMatch.IsDescendantOf(treeNode))
                    {
                        checkAndDoAutoPlay();
                        return;
                    }

                    if (View != null)
                    {
                        View.RefreshUI_WaveFormChunkMarkers(bytesLeft, bytesRight);
                    }

                    Logger.Log("-- PublishEvent [SubTreeNodeSelectedEvent] AudioPaneViewModel.updateWaveFormPlayHead",
                                   Category.Debug, Priority.Medium);

                    //EventAggregator.GetEvent<SubTreeNodeSelectedEvent>().Publish(State.CurrentSubTreeNode);
                    m_UrakawaSession.PerformTreeNodeSelection(treeNode);
                }
                else
                {
                    Debug.Fail("audio chunk not found ??");
                    return;
                }

                //State.CurrentSubTreeNode = subTreeNode;

                //if (State.CurrentSubTreeNode != State.CurrentTreeNode)
                //{
                //    Logger.Log("-- PublishEvent [SubTreeNodeSelectedEvent] AudioPaneViewModel.updateWaveFormPlayHead",
                //               Category.Debug, Priority.Medium);

                //    EventAggregator.GetEvent<SubTreeNodeSelectedEvent>().Publish(State.CurrentSubTreeNode);
                //}
                //else
                //{
                //    checkAndDoAutoPlay();
                //}
            }
        }

        #endregion Public Properties

        #region VuMeter / PeakMeter

        private VuMeter m_VuMeter;

        public PeakMeterBarData PeakMeterBarDataCh1 { get; set; }
        public PeakMeterBarData PeakMeterBarDataCh2 { get; set; }

        private double[] m_PeakMeterValues;

        public int PeakOverloadCountCh1
        {
            get
            {
                return PeakMeterBarDataCh1.PeakOverloadCount;
            }
            set
            {
                if (PeakMeterBarDataCh1.PeakOverloadCount == value) return;
                PeakMeterBarDataCh1.PeakOverloadCount = value;
                RaisePropertyChanged(() => PeakOverloadCountCh1);
            }
        }

        public int PeakOverloadCountCh2
        {
            get
            {
                return PeakMeterBarDataCh2.PeakOverloadCount;
            }
            set
            {
                if (PeakMeterBarDataCh2.PeakOverloadCount == value) return;
                PeakMeterBarDataCh2.PeakOverloadCount = value;
                RaisePropertyChanged(() => PeakOverloadCountCh2);
            }
        }

        public void UpdatePeakMeter()
        {
            if (m_Player.CurrentState != AudioPlayer.State.Playing
                && (m_Recorder.CurrentState != AudioRecorder.State.Recording && m_Recorder.CurrentState != AudioRecorder.State.Monitoring))
            {
                if (View != null)
                {
                    View.RefreshUI_PeakMeterBlackout(true);
                }
                return;
            }
            PeakMeterBarDataCh1.ValueDb = m_PeakMeterValues[0];

            PCMFormatInfo pcmInfo = State.Audio.GetCurrentPcmFormat();
            if (pcmInfo.Data.NumberOfChannels > 1)
            {
                PeakMeterBarDataCh2.ValueDb = m_PeakMeterValues[1];
            }

            if (View != null)
            {
                View.RefreshUI_PeakMeterBlackout(false);

                View.RefreshUI_PeakMeter();
            }
        }

        private void resetPeakMeter()
        {
            //Logger.Log("AudioPaneViewModel.resetPeakMeter", Category.Debug, Priority.Medium);

            PeakMeterBarDataCh1.ValueDb = Double.NegativeInfinity;
            //PeakMeterBarDataCh1.ForceFullFallback();

            PeakMeterBarDataCh2.ValueDb = Double.NegativeInfinity;
            //PeakMeterBarDataCh2.ForceFullFallback();

            m_PeakMeterValues[0] = PeakMeterBarDataCh1.ValueDb;
            m_PeakMeterValues[1] = PeakMeterBarDataCh2.ValueDb;

            UpdatePeakMeter();

            bool oldVal = IsAutoPlay;
            IsAutoPlay = false;
            AudioPlayer_UpdateWaveFormPlayHead();
            IsAutoPlay = oldVal;
        }

        #region Event / Callbacks

        private void OnPeakMeterUpdated(object sender, VuMeter.PeakMeterUpdateEventArgs e)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                  (Action<object, VuMeter.PeakMeterUpdateEventArgs>)OnPeakMeterUpdated_, sender, e);
                return;
            }
#if DEBUG
            Debugger.Break();
#endif
        }
        private void OnPeakMeterUpdated_(object sender, VuMeter.PeakMeterUpdateEventArgs e)
        {
            if (IsRecording || IsMonitoring)
            {
                if (View != null)
                {
                    View.TimeMessageRefresh();
                }

                RaisePropertyChanged(m_RecorderCurrentDurationArgs);
                //RaisePropertyChanged(() => RecorderCurrentDuration);
            }

            if (e.PeakDb != null && e.PeakDb.Length > 0)
            {
                m_PeakMeterValues[0] = e.PeakDb[0];

                PCMFormatInfo pcmInfo = State.Audio.GetCurrentPcmFormat();

                if (pcmInfo.Data.NumberOfChannels > 1)
                {
                    m_PeakMeterValues[1] = e.PeakDb[1];
                }
                UpdatePeakMeter();
            }
        }

        private void OnPeakMeterOverloaded(object sender, VuMeter.PeakOverloadEventArgs e)
        {
            if (!Dispatcher.CheckAccess())
            {

                Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                  (Action<object, VuMeter.PeakOverloadEventArgs>)OnPeakMeterOverloaded_, sender, e);
                return;
            }
#if DEBUG
            Debugger.Break();
#endif
        }
        private void OnPeakMeterOverloaded_(object sender, VuMeter.PeakOverloadEventArgs e)
        {
            if (e != null)
            {
                AudioCues.PlayHi();

                if (e.Channel == 1)
                {
                    PeakOverloadCountCh1++;
                }
                else if (e.Channel == 2)
                {
                    PeakOverloadCountCh2++;
                }
            }
        }

        #endregion Event / Callbacks

        #endregion VuMeter / PeakMeter

        private List<TreeNodeAndStreamSelection> getAudioSelectionData()
        {
            long byteSelectionLeft = State.Audio.ConvertMillisecondsToBytes(State.Selection.SelectionBegin);
            long byteSelectionRight = State.Audio.ConvertMillisecondsToBytes(State.Selection.SelectionEnd);

            //long byteLastPlayHeadTime = State.Audio.ConvertMillisecondsToBytes(LastPlayHeadTime);

            var listOfTreeNodeAndStreamSelection = new List<TreeNodeAndStreamSelection>();

            long bytesToMatch = byteSelectionLeft;

            State.Audio.FindInPlayStreamMarkersAndDo(bytesToMatch,
               (bytesLeft, bytesRight, markerTreeNode, index)
               =>
               {
                   if (listOfTreeNodeAndStreamSelection.Count == 0)
                   {
                       bool rightBoundaryIsAlsoHere = (byteSelectionRight < bytesRight
                                                       ||
                                                       index == (State.Audio.PlayStreamMarkers.Count - 1) &&
                                                       byteSelectionRight >= bytesRight);

                       TreeNodeAndStreamSelection data = new TreeNodeAndStreamSelection()
                       {
                           m_TreeNode = markerTreeNode,
                           m_LocalStreamLeftMark = byteSelectionLeft - bytesLeft,
                           m_LocalStreamRightMark = (rightBoundaryIsAlsoHere ? byteSelectionRight - bytesLeft : -1)
                       };
                       listOfTreeNodeAndStreamSelection.Add(data);

                       if (rightBoundaryIsAlsoHere)
                       {
                           return -1; // break;
                       }
                       return byteSelectionRight; // continue with new bytesToMatch
                   }
                   else
                   {
                       TreeNodeAndStreamSelection data = new TreeNodeAndStreamSelection()
                       {
                           m_TreeNode = markerTreeNode,
                           m_LocalStreamLeftMark = -1,
                           m_LocalStreamRightMark = byteSelectionRight - bytesLeft
                       };

                       if (data.m_LocalStreamRightMark > 0)
                           listOfTreeNodeAndStreamSelection.Add(data);

                       return -1; // break;
                   }
               }
                ,
               (bytesToMatch_, bytesLeft, bytesRight, markerTreeNode)
               =>
               {
                   if (listOfTreeNodeAndStreamSelection.Count > 0)
                   {
                       TreeNodeAndStreamSelection data = new TreeNodeAndStreamSelection()
                       {
                           m_TreeNode = markerTreeNode,
                           m_LocalStreamLeftMark = -1,
                           m_LocalStreamRightMark = -1
                       };
                       listOfTreeNodeAndStreamSelection.Add(data);
                   }

                   return bytesToMatch_; // continue with same bytesToMatch
               }
                );

            return listOfTreeNodeAndStreamSelection;
        }
    }
}