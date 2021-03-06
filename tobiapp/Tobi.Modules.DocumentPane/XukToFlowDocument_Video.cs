﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using AudioLib;
using Tobi.Common.UI.XAML;
using urakawa.core;
using urakawa.data;
using urakawa.media;
using urakawa.media.data.audio;
using urakawa.media.data.video;
using urakawa.media.timing;
using urakawa.property.channel;
using urakawa.property.xml;
using Colors = System.Windows.Media.Colors;

#if ENABLE_WPF_MEDIAKIT
using WPFMediaKit.DirectShow.Controls;
using MediaState = System.Windows.Controls.MediaState;
using WPFMediaKit.DirectShow.MediaPlayers;
#endif //ENABLE_WPF_MEDIAKIT

namespace Tobi.Plugin.DocumentPane
{
    public partial class XukToFlowDocument
    {
        public List<Action<object, RoutedEventArgs>> FlowDocumentLoadedEvents = new List<Action<object, RoutedEventArgs>>();
        public List<Action<object, RoutedEventArgs>> FlowDocumentUnLoadedEvents = new List<Action<object, RoutedEventArgs>>();

        private TextElement walkBookTreeAndGenerateFlowDocument_audio_video(TreeNode node, TextElement parent, string textMedia)
        {
            //            if (node.Children.Count != 0 || textMedia != null && !String.IsNullOrEmpty(textMedia))
            //            {
            //#if DEBUG
            //                Debugger.Break();
            //#endif
            //                throw new Exception("Node has children or text exists when processing video ??");
            //            }

            XmlProperty xmlProp = node.GetXmlProperty();

            bool isSource = "source".Equals(xmlProp.LocalName, StringComparison.OrdinalIgnoreCase);

            XmlProperty xmlProp_ = xmlProp;
            if (isSource && node.Parent != null)
            {
                xmlProp_ = node.Parent.GetXmlProperty();
            }


            AbstractVideoMedia videoMedia = node.GetVideoMedia();
            var videoMedia_ext = videoMedia as ExternalVideoMedia;
            var videoMedia_man = videoMedia as ManagedVideoMedia;


            Media audioMedia = node.GetMediaInChannel<AudioXChannel>(); // as AbstractAudioMedia;
            var audioMedia_ext = audioMedia as ExternalAudioMedia;
            var audioMedia_man = audioMedia as ManagedAudioMedia;



            string dirPath = Path.GetDirectoryName(m_TreeNode.Presentation.RootUri.LocalPath);

            string videoPath = null;

            if (videoMedia_ext != null)
            {

#if DEBUG
                Debugger.Break();
#endif //DEBUG

                videoPath = Path.Combine(dirPath, videoMedia_ext.Src);
            }
            else if (videoMedia_man != null)
            {
#if DEBUG
                XmlAttribute srcAttr = xmlProp.GetAttribute("src");

                DebugFix.Assert(videoMedia_man.VideoMediaData.OriginalRelativePath == FileDataProvider.UriDecode(srcAttr.Value));
#endif //DEBUG
                var fileDataProv = videoMedia_man.VideoMediaData.DataProvider as FileDataProvider;

                if (fileDataProv != null)
                {
                    videoPath = fileDataProv.DataFileFullPath;
                }
            }


            string audioPath = null;

            if (audioMedia_ext != null)
            {

#if DEBUG
                Debugger.Break();
#endif //DEBUG

                audioPath = Path.Combine(dirPath, audioMedia_ext.Src);
            }
            else if (audioMedia_man != null)
            {
#if DEBUG
                XmlAttribute srcAttr = xmlProp.GetAttribute("src");

                DebugFix.Assert(audioMedia_man.AudioMediaData.OriginalRelativePath == FileDataProvider.UriDecode(srcAttr.Value));
#endif //DEBUG
                var fileDataProv = audioMedia_man.AudioMediaData.DataProvider as FileDataProvider;

                if (fileDataProv != null)
                {
                    audioPath = fileDataProv.DataFileFullPath;
                }
            }

            if (
                (videoPath == null || FileDataProvider.isHTTPFile(videoPath))
                &&
                (audioPath == null || FileDataProvider.isHTTPFile(audioPath))
                )
            {
#if DEBUG
                Debugger.Break();
#endif //DEBUG

                return walkBookTreeAndGenerateFlowDocument_Section(node, parent, textMedia, null);
            }

            string path = string.IsNullOrEmpty(videoPath) ? audioPath : videoPath;

            var uri = new Uri(path, UriKind.Absolute);


            string videoAudioAlt = null;


            XmlAttribute altAttr = xmlProp_.GetAttribute("alt");
            if (altAttr != null)
            {
                videoAudioAlt = altAttr.Value;
            }



            bool parentHasBlocks = parent is TableCell
                                   || parent is Section
                                   || parent is Floater
                                   || parent is Figure
                                   || parent is ListItem;

            var videoAudioPanel = new StackPanel();
            videoAudioPanel.Orientation = Orientation.Vertical;

            //videoPanel.LastChildFill = true;
            if (!string.IsNullOrEmpty(videoAudioAlt))
            {
                var tb = new TextBlock(new Run(videoAudioAlt))
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap
                };
                videoAudioPanel.Children.Add(tb);
            }
            //videoPanel.Children.Add(mediaElement);


            var slider = new Slider();
            slider.Focusable = false;
            slider.TickPlacement = TickPlacement.None;
            slider.IsMoveToPointEnabled = true;
            slider.Minimum = 0;
            slider.Maximum = 100;
            slider.Visibility = Visibility.Hidden;

            videoAudioPanel.Children.Add(slider);


            var timeLabel = new TextBlock();
            timeLabel.Focusable = false;
            //timeLabel.IsEnabled = false;
            timeLabel.TextWrapping = TextWrapping.NoWrap;
            //timeLabel.TextTrimming = TextTrimming.None;
            timeLabel.HorizontalAlignment = HorizontalAlignment.Stretch;
            timeLabel.TextAlignment = TextAlignment.Center;
            timeLabel.Visibility = Visibility.Hidden;

            videoAudioPanel.Children.Add(timeLabel);

            var playPause = new Button()
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    FontWeight = FontWeights.Heavy,
                    Content = new Run("Play / Pause")
                };
            //var border_ = new Border()
            //    {
            //        BorderThickness = new Thickness(2.0),
            //        BorderBrush = ColorBrushCache.Get(Settings.Default.Document_Color_Font_NoAudio),
            //        Padding = new Thickness(4),
            //        Child = playPause
            //    };
            videoAudioPanel.Children.Add(playPause);

            videoAudioPanel.HorizontalAlignment = HorizontalAlignment.Stretch;
            videoAudioPanel.VerticalAlignment = VerticalAlignment.Top;

            var panelBorder = new Border();
            panelBorder.HorizontalAlignment = HorizontalAlignment.Stretch;
            panelBorder.VerticalAlignment = VerticalAlignment.Top;
            panelBorder.Child = videoAudioPanel;
            panelBorder.Padding = new Thickness(4);
            panelBorder.BorderBrush = ColorBrushCache.Get(Settings.Default.Document_Color_Font_Audio);
            panelBorder.BorderThickness = new Thickness(2.0);


            if (parentHasBlocks)
            {
                Block vidContainer = new BlockUIContainer(panelBorder);
                vidContainer.TextAlignment = TextAlignment.Center;

                setTag(vidContainer, node);

                addBlock(parent, vidContainer);
            }
            else
            {
                Inline vidContainer = new InlineUIContainer(panelBorder);

                setTag(vidContainer, node);

                addInline(parent, vidContainer);
            }


            MediaElement medElement_WINDOWS_MEDIA_PLAYER = null;
#if ENABLE_WPF_MEDIAKIT
            MediaUriElement medElement_MEDIAKIT_DIRECTSHOW = null;
#endif //ENABLE_WPF_MEDIAKIT

            var reh = (Action<object, RoutedEventArgs>)(
                (object obj, RoutedEventArgs rev) =>
                {
#if ENABLE_WPF_MEDIAKIT
                    if (Common.Settings.Default.EnableMediaKit)
                    {
                        medElement_MEDIAKIT_DIRECTSHOW = new MediaUriElement();
                    }
                    else
#endif //ENABLE_WPF_MEDIAKIT
                    {
                        medElement_WINDOWS_MEDIA_PLAYER = new MediaElement();
                    }





#if ENABLE_WPF_MEDIAKIT
                    DebugFix.Assert((medElement_WINDOWS_MEDIA_PLAYER == null) == (medElement_MEDIAKIT_DIRECTSHOW != null));
#else  // DISABLE_WPF_MEDIAKIT
            DebugFix.Assert(medElement_WINDOWS_MEDIA_PLAYER!=null);
#endif //ENABLE_WPF_MEDIAKIT




                    if (medElement_WINDOWS_MEDIA_PLAYER != null)
                    {
                        medElement_WINDOWS_MEDIA_PLAYER.Stretch = Stretch.Uniform;
                        medElement_WINDOWS_MEDIA_PLAYER.StretchDirection = StretchDirection.DownOnly;
                    }

#if ENABLE_WPF_MEDIAKIT
                    if (medElement_MEDIAKIT_DIRECTSHOW != null)
                    {
                        medElement_MEDIAKIT_DIRECTSHOW.Stretch = Stretch.Uniform;
                        medElement_MEDIAKIT_DIRECTSHOW.StretchDirection = StretchDirection.DownOnly;
                    }
#endif //ENABLE_WPF_MEDIAKIT



                    FrameworkElement mediaElement = null;
                    if (medElement_WINDOWS_MEDIA_PLAYER != null)
                    {
                        mediaElement = medElement_WINDOWS_MEDIA_PLAYER;
                    }
                    else
                    {
                        mediaElement = medElement_MEDIAKIT_DIRECTSHOW;
                    }

                    mediaElement.Focusable = false;

                    XmlAttribute srcW = xmlProp_.GetAttribute("width");
                    if (srcW != null)
                    {
                        var d = parseWidthHeight(srcW.Value);
                        if (d > 0)
                        {
                            mediaElement.Width = d;
                        }
                    }
                    XmlAttribute srcH = xmlProp_.GetAttribute("height");
                    if (srcH != null)
                    {
                        var d = parseWidthHeight(srcH.Value);
                        if (d > 0)
                        {
                            mediaElement.Height = d;
                        }
                    }

                    mediaElement.HorizontalAlignment = HorizontalAlignment.Center;
                    mediaElement.VerticalAlignment = VerticalAlignment.Top;

                    if (!string.IsNullOrEmpty(videoAudioAlt))
                    {
                        mediaElement.ToolTip = videoAudioAlt;
                    }

                    videoAudioPanel.Children.Insert(0, mediaElement);

                    var actionMediaFailed = new Action<string>(
                        (str) =>
                        {
                            m_DocumentPaneView.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                (Action)(() =>
                                {
                                    var label = new TextBlock(new Run(str));

                                    label.TextWrapping = TextWrapping.Wrap;
                                    //label.Height = 150;

                                    var border = new Border();
                                    border.Child = label;
                                    border.BorderBrush = ColorBrushCache.Get(Colors.Red);
                                    border.BorderThickness = new Thickness(2.0);

                                    videoAudioPanel.Children.Insert(0, border);

                                    slider.Visibility = Visibility.Hidden;
                                    timeLabel.Visibility = Visibility.Hidden;
                                }
                                ));
                        }
                        );


                    Action actionUpdateSliderFromVideoTime = null;
                    DispatcherTimer _timer = new DispatcherTimer();
                    _timer.Interval = new TimeSpan(0, 0, 0, 0, 500);
                    _timer.Stop();
                    _timer.Tick += (object oo, EventArgs ee) =>
                    {
                        actionUpdateSliderFromVideoTime.Invoke();
                    };



                    if (medElement_WINDOWS_MEDIA_PLAYER != null)
                    {
                        medElement_WINDOWS_MEDIA_PLAYER.ScrubbingEnabled = true;

                        medElement_WINDOWS_MEDIA_PLAYER.LoadedBehavior = MediaState.Manual;
                        medElement_WINDOWS_MEDIA_PLAYER.UnloadedBehavior = MediaState.Stop;


                        bool doNotUpdateVideoTimeWhenSliderChanges = false;
                        actionUpdateSliderFromVideoTime = new Action(() =>
                        {
                            if (medElement_WINDOWS_MEDIA_PLAYER == null)
                            {
#if DEBUG
                                    Debugger.Break();
#endif
                                return;
                            }

                            TimeSpan? timeSpan = medElement_WINDOWS_MEDIA_PLAYER.Clock.CurrentTime;
                            double timeMS = timeSpan != null ? timeSpan.Value.TotalMilliseconds : 0;

                            //Console.WriteLine("UPDATE: " + timeMS);

                            //if (medElement_WINDOWS_MEDIA_PLAYER.NaturalDuration.HasTimeSpan
                            //    && timeMS >= medElement_WINDOWS_MEDIA_PLAYER.NaturalDuration.TimeSpan.TotalMilliseconds - 50)
                            //{
                            //    medElement_WINDOWS_MEDIA_PLAYER.Clock.Controller.Stop();
                            //}

                            doNotUpdateVideoTimeWhenSliderChanges = true;
                            slider.Value = timeMS;
                        });

                        medElement_WINDOWS_MEDIA_PLAYER.MediaFailed += new EventHandler<ExceptionRoutedEventArgs>(
                            (oo, ee) =>
                            {
                                //#if DEBUG
                                //                                Debugger.Break();
                                //#endif //DEBUG
                                //medElement_WINDOWS_MEDIA_PLAYER.Source
                                actionMediaFailed.Invoke(uri.ToString()
                                    + " \n("
                                    + (ee.ErrorException != null ? ee.ErrorException.Message : "ERROR!")
                                    + ")");
                            }
                            );



                        medElement_WINDOWS_MEDIA_PLAYER.MediaOpened += new RoutedEventHandler(
                            (oo, ee) =>
                            {
                                if (medElement_WINDOWS_MEDIA_PLAYER == null)
                                {
#if DEBUG
                                    Debugger.Break();
#endif
                                    return;
                                }

                                slider.Visibility = Visibility.Visible;
                                timeLabel.Visibility = Visibility.Visible;

                                double durationMS = medElement_WINDOWS_MEDIA_PLAYER.NaturalDuration.TimeSpan.TotalMilliseconds;
                                timeLabel.Text = Time.Format_Standard(medElement_WINDOWS_MEDIA_PLAYER.NaturalDuration.TimeSpan);

                                slider.Maximum = durationMS;


                                // freeze frame (poster)
                                if (medElement_WINDOWS_MEDIA_PLAYER.LoadedBehavior == MediaState.Manual)
                                {
                                    medElement_WINDOWS_MEDIA_PLAYER.IsMuted = true;

                                    medElement_WINDOWS_MEDIA_PLAYER.Clock.Controller.Begin();
                                    medElement_WINDOWS_MEDIA_PLAYER.Clock.Controller.Pause();

                                    medElement_WINDOWS_MEDIA_PLAYER.IsMuted = false;

                                    slider.Value = 0.10;
                                }
                            }
                            );



                        medElement_WINDOWS_MEDIA_PLAYER.MediaEnded +=
                            new RoutedEventHandler(
                            (oo, ee) =>
                            {
                                if (medElement_WINDOWS_MEDIA_PLAYER == null)
                                {
#if DEBUG
                                    Debugger.Break();
#endif
                                    return;
                                }

                                _timer.Stop();

                                medElement_WINDOWS_MEDIA_PLAYER.Clock.Controller.Stop();

                                actionUpdateSliderFromVideoTime.Invoke();
                            }
                            );

                        var mouseButtonEventHandler_WINDOWS_MEDIA_PLAYER = new Action(
                                () =>
                                {
                                    if (medElement_WINDOWS_MEDIA_PLAYER == null)
                                    {
#if DEBUG
                                    Debugger.Break();
#endif
                                        return;
                                    }

                                    if (medElement_WINDOWS_MEDIA_PLAYER.LoadedBehavior != MediaState.Manual)
                                    {
                                        return;
                                    }

                                    bool wasPlaying = false;
                                    bool wasStopped = false;

                                    //Is Active
                                    if (medElement_WINDOWS_MEDIA_PLAYER.Clock.CurrentState == ClockState.Active)
                                    {
                                        //Is Paused
                                        if (medElement_WINDOWS_MEDIA_PLAYER.Clock.CurrentGlobalSpeed == 0.0)
                                        {
                                        }
                                        else //Is Playing
                                        {
                                            wasPlaying = true;
                                            medElement_WINDOWS_MEDIA_PLAYER.Clock.Controller.Pause();
                                        }
                                    }
                                    else if (medElement_WINDOWS_MEDIA_PLAYER.Clock.CurrentState == ClockState.Stopped)
                                    {
                                        wasStopped = true;
                                        //medElement_WINDOWS_MEDIA_PLAYER.Clock.Controller.Begin();
                                        //medElement_WINDOWS_MEDIA_PLAYER.Clock.Controller.Pause();
                                    }

                                    double durationMS = medElement_WINDOWS_MEDIA_PLAYER.NaturalDuration.TimeSpan.TotalMilliseconds;
                                    double timeMS =
                                        medElement_WINDOWS_MEDIA_PLAYER.Clock.CurrentTime == null
                                        || !medElement_WINDOWS_MEDIA_PLAYER.Clock.CurrentTime.HasValue
                                        ? -1.0
                                        : medElement_WINDOWS_MEDIA_PLAYER.Clock.CurrentTime.Value.TotalMilliseconds;

                                    if (timeMS == -1.0 || timeMS >= durationMS)
                                    {
                                        slider.Value = 0.100;
                                    }

                                    if (!wasPlaying)
                                    {
                                        _timer.Start();
                                        if (wasStopped)
                                        {
                                            medElement_WINDOWS_MEDIA_PLAYER.Clock.Controller.Begin();
                                        }
                                        else
                                        {
                                            medElement_WINDOWS_MEDIA_PLAYER.Clock.Controller.Resume();
                                        }
                                    }
                                    else
                                    {
                                        _timer.Stop();
                                        medElement_WINDOWS_MEDIA_PLAYER.Clock.Controller.Pause();
                                        actionUpdateSliderFromVideoTime.Invoke();
                                    }

                                    //if (ee.ChangedButton == MouseButton.Left)
                                    //{
                                    //}
                                    //else if (ee.ChangedButton == MouseButton.Right)
                                    //{
                                    //    _timer.Stop();

                                    //    //Is Active
                                    //    if (medElement_WINDOWS_MEDIA_PLAYER.Clock.CurrentState == ClockState.Active)
                                    //    {
                                    //        //Is Paused
                                    //        if (medElement_WINDOWS_MEDIA_PLAYER.Clock.CurrentGlobalSpeed == 0.0)
                                    //        {

                                    //        }
                                    //        else //Is Playing
                                    //        {
                                    //            medElement_WINDOWS_MEDIA_PLAYER.Clock.Controller.Pause();
                                    //        }
                                    //    }
                                    //    else if (medElement_WINDOWS_MEDIA_PLAYER.Clock.CurrentState == ClockState.Stopped)
                                    //    {
                                    //        medElement_WINDOWS_MEDIA_PLAYER.Clock.Controller.Begin();
                                    //        medElement_WINDOWS_MEDIA_PLAYER.Clock.Controller.Pause();
                                    //    }

                                    //    //actionRefreshTime.Invoke();
                                    //    slider.Value = 0;
                                    //}
                                }
                                );
                        medElement_WINDOWS_MEDIA_PLAYER.MouseDown +=
                            new MouseButtonEventHandler((oo, ee) => mouseButtonEventHandler_WINDOWS_MEDIA_PLAYER());
                        playPause.Click += new RoutedEventHandler((object sender, RoutedEventArgs e) => mouseButtonEventHandler_WINDOWS_MEDIA_PLAYER());

                        slider.ValueChanged += new RoutedPropertyChangedEventHandler<double>(
                            (oo, ee) =>
                            {
                                if (medElement_WINDOWS_MEDIA_PLAYER == null)
                                {
#if DEBUG
                                    Debugger.Break();
#endif
                                    return;
                                }

                                var timeSpan = new TimeSpan(0, 0, 0, 0, (int)Math.Round(slider.Value));

                                if (doNotUpdateVideoTimeWhenSliderChanges || !_timer.IsEnabled)
                                {
                                    double durationMS = medElement_WINDOWS_MEDIA_PLAYER.NaturalDuration.TimeSpan.TotalMilliseconds;

                                    timeLabel.Text = String.Format(
                                        "{0} / {1}",
                                        Time.Format_Standard(timeSpan),
                                        Time.Format_Standard(medElement_WINDOWS_MEDIA_PLAYER.NaturalDuration.TimeSpan)
                                         );
                                }

                                if (doNotUpdateVideoTimeWhenSliderChanges)
                                {
                                    doNotUpdateVideoTimeWhenSliderChanges = false;
                                    return;
                                }

                                bool wasPlaying = false;

                                //Is Active
                                if (medElement_WINDOWS_MEDIA_PLAYER.Clock.CurrentState == ClockState.Active)
                                {
                                    //Is Paused
                                    if (medElement_WINDOWS_MEDIA_PLAYER.Clock.CurrentGlobalSpeed == 0.0)
                                    {

                                    }
                                    else //Is Playing
                                    {
                                        wasPlaying = true;
                                        medElement_WINDOWS_MEDIA_PLAYER.Clock.Controller.Pause();
                                    }
                                }
                                else if (medElement_WINDOWS_MEDIA_PLAYER.Clock.CurrentState == ClockState.Stopped)
                                {
                                    medElement_WINDOWS_MEDIA_PLAYER.Clock.Controller.Begin();
                                    medElement_WINDOWS_MEDIA_PLAYER.Clock.Controller.Pause();
                                }

                                medElement_WINDOWS_MEDIA_PLAYER.Clock.Controller.Seek(timeSpan, TimeSeekOrigin.BeginTime);

                                if (wasPlaying)
                                {
                                    medElement_WINDOWS_MEDIA_PLAYER.Clock.Controller.Resume();
                                }
                            });

                        bool wasPlayingBeforeDrag = false;
                        slider.AddHandler(Thumb.DragStartedEvent,
                            new DragStartedEventHandler(
                            (Action<object, DragStartedEventArgs>)(
                            (oo, ee) =>
                            {
                                if (medElement_WINDOWS_MEDIA_PLAYER == null)
                                {
#if DEBUG
                                    Debugger.Break();
#endif
                                    return;
                                }

                                wasPlayingBeforeDrag = false;

                                //Is Active
                                if (medElement_WINDOWS_MEDIA_PLAYER.Clock.CurrentState == ClockState.Active)
                                {
                                    //Is Paused
                                    if (medElement_WINDOWS_MEDIA_PLAYER.Clock.CurrentGlobalSpeed == 0.0)
                                    {

                                    }
                                    else //Is Playing
                                    {
                                        wasPlayingBeforeDrag = true;
                                        medElement_WINDOWS_MEDIA_PLAYER.Clock.Controller.Pause();
                                    }
                                }
                                else if (medElement_WINDOWS_MEDIA_PLAYER.Clock.CurrentState == ClockState.Stopped)
                                {
                                    medElement_WINDOWS_MEDIA_PLAYER.Clock.Controller.Begin();
                                    medElement_WINDOWS_MEDIA_PLAYER.Clock.Controller.Pause();
                                }
                            })));

                        slider.AddHandler(Thumb.DragCompletedEvent,
                            new DragCompletedEventHandler(
                            (Action<object, DragCompletedEventArgs>)(
                            (oo, ee) =>
                            {
                                if (medElement_WINDOWS_MEDIA_PLAYER == null)
                                {
#if DEBUG
                                    Debugger.Break();
#endif
                                    return;
                                }

                                if (wasPlayingBeforeDrag)
                                {
                                    medElement_WINDOWS_MEDIA_PLAYER.Clock.Controller.Resume();
                                }
                                wasPlayingBeforeDrag = false;
                            })));
                    }












#if ENABLE_WPF_MEDIAKIT
                    if (medElement_MEDIAKIT_DIRECTSHOW != null)
                    {
                        bool doNotUpdateVideoTimeWhenSliderChanges = false;
                        actionUpdateSliderFromVideoTime = new Action(() =>
                        {
                            if (medElement_MEDIAKIT_DIRECTSHOW == null)
                            {
#if DEBUG
                                    Debugger.Break();
#endif
                                return;
                            }
                            long timeVideo = medElement_MEDIAKIT_DIRECTSHOW.MediaPosition;

                            //if (timeMS >= medElement_MEDIAKIT_DIRECTSHOW.MediaDuration - 50 * 10000.0)
                            //{
                            //    medElement_MEDIAKIT_DIRECTSHOW.Stop();
                            //}


                            double timeMS = timeVideo / 10000.0;

                            //Console.WriteLine("UPDATE: " + timeMS);

                            doNotUpdateVideoTimeWhenSliderChanges = true;
                            slider.Value = timeMS;
                        });


                        medElement_MEDIAKIT_DIRECTSHOW.MediaFailed += new EventHandler<WPFMediaKit.DirectShow.MediaPlayers.MediaFailedEventArgs>(
                            (oo, ee) =>
                            {
                                //#if DEBUG
                                //                        Debugger.Break();
                                //#endif //DEBUG

                                //medElement_MEDIAKIT_DIRECTSHOW.Source
                                actionMediaFailed.Invoke(uri.ToString()
                                    + " \n("
                                    + (ee.Exception != null ? ee.Exception.Message : ee.Message)
                                    + ")");
                            }
                                );



                        medElement_MEDIAKIT_DIRECTSHOW.MediaOpened += new RoutedEventHandler(
                            (oo, ee) =>
                            {
                                if (medElement_MEDIAKIT_DIRECTSHOW == null)
                                {
#if DEBUG
                                    Debugger.Break();
#endif
                                    return;
                                }

                                long durationVideo = medElement_MEDIAKIT_DIRECTSHOW.MediaDuration;
                                if (durationVideo == 0)
                                {
                                    return;
                                }

                                //MediaPositionFormat mpf = medElement.CurrentPositionFormat;
                                //MediaPositionFormat.MediaTime
                                double durationMS = durationVideo / 10000.0;


                                slider.Visibility = Visibility.Visible;
                                timeLabel.Visibility = Visibility.Visible;

                                slider.Maximum = durationMS;

                                var durationTimeSpan = new TimeSpan(0, 0, 0, 0, (int)Math.Round(durationMS));
                                timeLabel.Text = Time.Format_Standard(durationTimeSpan);


                                // freeze frame (poster)
                                if (medElement_MEDIAKIT_DIRECTSHOW.LoadedBehavior == WPFMediaKit.DirectShow.MediaPlayers.MediaState.Manual)
                                {
                                    if (false)
                                    {
                                        double volume = medElement_MEDIAKIT_DIRECTSHOW.Volume;
                                        medElement_MEDIAKIT_DIRECTSHOW.Volume = 0;

                                        medElement_MEDIAKIT_DIRECTSHOW.Play();
                                        slider.Value = 0.10;
                                        medElement_MEDIAKIT_DIRECTSHOW.Pause();

                                        medElement_MEDIAKIT_DIRECTSHOW.Volume = volume;
                                    }
                                    else
                                    {
                                        medElement_MEDIAKIT_DIRECTSHOW.Pause();
                                        slider.Value = 0.10;
                                    }
                                }
                            }
                            );



                        medElement_MEDIAKIT_DIRECTSHOW.MediaEnded +=
                            new RoutedEventHandler(
                            (oo, ee) =>
                            {
                                if (medElement_MEDIAKIT_DIRECTSHOW == null)
                                {
#if DEBUG
                                    Debugger.Break();
#endif
                                    return;
                                }

                                _timer.Stop();
                                medElement_MEDIAKIT_DIRECTSHOW.Pause();
                                actionUpdateSliderFromVideoTime.Invoke();

                                // TODO: BaseClasses.cs in WPF Media Kit,
                                // MediaPlayerBase.OnMediaEvent
                                // ==> remove StopGraphPollTimer();
                                // in case EventCode.Complete.


                                //m_DocumentPaneView.Dispatcher.BeginInvoke(
                                //    DispatcherPriority.Background,
                                //    (Action)(() =>
                                //    {
                                //        //medElement_MEDIAKIT_DIRECTSHOW.BeginInit();
                                //        medElement_MEDIAKIT_DIRECTSHOW.Source = uri;
                                //        //medElement_MEDIAKIT_DIRECTSHOW.EndInit();
                                //    })
                                //    );
                            }
                            );


                        medElement_MEDIAKIT_DIRECTSHOW.MediaClosed +=
                            new RoutedEventHandler(
                            (oo, ee) =>
                            {
                                int debug = 1;
                            }
                            );

                        var mouseButtonEventHandler_MEDIAKIT_DIRECTSHOW = new Action(
                                () =>
                                {
                                    if (medElement_MEDIAKIT_DIRECTSHOW == null)
                                    {
#if DEBUG
                                    Debugger.Break();
#endif
                                        return;
                                    }

                                    if (medElement_MEDIAKIT_DIRECTSHOW.LoadedBehavior != WPFMediaKit.DirectShow.MediaPlayers.MediaState.Manual)
                                    {
                                        return;
                                    }

                                    if (medElement_MEDIAKIT_DIRECTSHOW.IsPlaying)
                                    {
                                        _timer.Stop();
                                        medElement_MEDIAKIT_DIRECTSHOW.Pause();
                                        actionUpdateSliderFromVideoTime.Invoke();
                                    }
                                    else
                                    {
                                        _timer.Start();
                                        medElement_MEDIAKIT_DIRECTSHOW.Play();
                                    }


                                    double durationMS = medElement_MEDIAKIT_DIRECTSHOW.MediaDuration / 10000.0;
                                    double timeMS = medElement_MEDIAKIT_DIRECTSHOW.MediaPosition / 10000.0;

                                    if (timeMS >= durationMS)
                                    {
                                        slider.Value = 0.100;
                                    }

                                    //if (ee.ChangedButton == MouseButton.Left)
                                    //{
                                    //}
                                    //else if (ee.ChangedButton == MouseButton.Right)
                                    //{
                                    //    _timer.Stop();
                                    //    medElement_MEDIAKIT_DIRECTSHOW.Pause();
                                    //    //actionRefreshTime.Invoke();
                                    //    slider.Value = 0;
                                    //}
                                }
                                );
                        medElement_MEDIAKIT_DIRECTSHOW.MouseDown +=
                            new MouseButtonEventHandler((oo, ee) => mouseButtonEventHandler_MEDIAKIT_DIRECTSHOW());
                        playPause.Click += new RoutedEventHandler((object sender, RoutedEventArgs e) => mouseButtonEventHandler_MEDIAKIT_DIRECTSHOW());

                        slider.ValueChanged += new RoutedPropertyChangedEventHandler<double>(
                            (oo, ee) =>
                            {
                                if (medElement_MEDIAKIT_DIRECTSHOW == null)
                                {
#if DEBUG
                                    Debugger.Break();
#endif
                                    return;
                                }

                                double timeMs = slider.Value;

                                if (doNotUpdateVideoTimeWhenSliderChanges || !_timer.IsEnabled)
                                {
                                    var timeSpan = new TimeSpan(0, 0, 0, 0, (int)Math.Round(timeMs));

                                    double durationMS = medElement_MEDIAKIT_DIRECTSHOW.MediaDuration / 10000.0;

                                    //MediaPositionFormat.MediaTime
                                    //MediaPositionFormat mpf = medElement.CurrentPositionFormat;

                                    timeLabel.Text = String.Format(
                                        "{0} / {1}",
                                        Time.Format_Standard(timeSpan),
                                        Time.Format_Standard(new TimeSpan(0, 0, 0, 0, (int)Math.Round(durationMS)))
                                         );
                                }

                                if (doNotUpdateVideoTimeWhenSliderChanges)
                                {
                                    doNotUpdateVideoTimeWhenSliderChanges = false;
                                    return;
                                }

                                bool wasPlaying = medElement_MEDIAKIT_DIRECTSHOW.IsPlaying;

                                if (wasPlaying)
                                {
                                    medElement_MEDIAKIT_DIRECTSHOW.Pause();
                                }

                                long timeVideo = (long)Math.Round(timeMs * 10000.0);
                                medElement_MEDIAKIT_DIRECTSHOW.MediaPosition = timeVideo;

                                DebugFix.Assert(medElement_MEDIAKIT_DIRECTSHOW.MediaPosition == timeVideo);

                                if (wasPlaying)
                                {
                                    medElement_MEDIAKIT_DIRECTSHOW.Play();
                                }
                            });

                        bool wasPlayingBeforeDrag = false;
                        slider.AddHandler(Thumb.DragStartedEvent,
                            new DragStartedEventHandler(
                            (Action<object, DragStartedEventArgs>)(
                            (oo, ee) =>
                            {
                                if (medElement_MEDIAKIT_DIRECTSHOW == null)
                                {
#if DEBUG
                                    Debugger.Break();
#endif
                                    return;
                                }

                                wasPlayingBeforeDrag = medElement_MEDIAKIT_DIRECTSHOW.IsPlaying;

                                if (wasPlayingBeforeDrag)
                                {
                                    medElement_MEDIAKIT_DIRECTSHOW.Pause();
                                }
                            })));


                        slider.AddHandler(Thumb.DragCompletedEvent,
                            new DragCompletedEventHandler(
                            (Action<object, DragCompletedEventArgs>)(
                            (oo, ee) =>
                            {
                                if (medElement_MEDIAKIT_DIRECTSHOW == null)
                                {
#if DEBUG
                                    Debugger.Break();
#endif
                                    return;
                                }

                                if (wasPlayingBeforeDrag)
                                {
                                    medElement_MEDIAKIT_DIRECTSHOW.Play();
                                }
                                wasPlayingBeforeDrag = false;
                            })));

                        //DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(
                        //    MediaSeekingElement.MediaPositionProperty,
                        //    typeof(MediaSeekingElement));
                        //if (dpd != null)
                        //{
                        //    dpd.AddValueChanged(medElement_MEDIAKIT_DIRECTSHOW, new EventHandler((o, e) =>
                        //    {
                        //        //actionRefreshTime.Invoke();

                        //        //if (!_timer.IsEnabled)
                        //        //{
                        //        //    _timer.Start();
                        //        //}
                        //    }));
                        //}

                    }
#endif //ENABLE_WPF_MEDIAKIT


                    if (medElement_WINDOWS_MEDIA_PLAYER != null)
                    {
                        var timeline = new MediaTimeline();
                        timeline.Source = uri;

                        medElement_WINDOWS_MEDIA_PLAYER.Clock = timeline.CreateClock(true) as MediaClock;

                        medElement_WINDOWS_MEDIA_PLAYER.Clock.Controller.Stop();

                        //medElement_WINDOWS_MEDIA_PLAYER.Clock.CurrentTimeInvalidated += new EventHandler(
                        //(o, e) =>
                        //{
                        //    //actionRefreshTime.Invoke();
                        //    //if (!_timer.IsEnabled)
                        //    //{
                        //    //    _timer.Start();
                        //    //}
                        //});

                    }

#if ENABLE_WPF_MEDIAKIT
                    if (medElement_MEDIAKIT_DIRECTSHOW != null)
                    {
                        medElement_MEDIAKIT_DIRECTSHOW.BeginInit();

                        medElement_MEDIAKIT_DIRECTSHOW.Loop = false;
                        medElement_MEDIAKIT_DIRECTSHOW.VideoRenderer = VideoRendererType.VideoMixingRenderer9;

                        // seems to be a multiplicator of 10,000 to get milliseconds
                        medElement_MEDIAKIT_DIRECTSHOW.PreferedPositionFormat = MediaPositionFormat.MediaTime;


                        medElement_MEDIAKIT_DIRECTSHOW.LoadedBehavior = WPFMediaKit.DirectShow.MediaPlayers.MediaState.Manual;
                        medElement_MEDIAKIT_DIRECTSHOW.UnloadedBehavior = WPFMediaKit.DirectShow.MediaPlayers.MediaState.Stop;

                        try
                        {
                            medElement_MEDIAKIT_DIRECTSHOW.Source = uri;

                            medElement_MEDIAKIT_DIRECTSHOW.EndInit();
                        }
                        catch (Exception ex)
                        {
#if DEBUG
                            Debugger.Break();
#endif //DEBUG
                            ; // swallow (reported in MediaFailed)
                        }
                    }
#endif //ENABLE_WPF_MEDIAKIT
                });

            FlowDocumentLoadedEvents.Add(reh);
            m_FlowDoc.Loaded += new RoutedEventHandler(reh);




            var reh2 = (Action<object, RoutedEventArgs>)(
                (object o, RoutedEventArgs e) =>
                {
                    bool thereWasOne = false;
                    if (medElement_WINDOWS_MEDIA_PLAYER != null)
                    {
                        thereWasOne = true;
                        medElement_WINDOWS_MEDIA_PLAYER.Close();
                        medElement_WINDOWS_MEDIA_PLAYER = null;
                    }

#if ENABLE_WPF_MEDIAKIT
                    if (medElement_MEDIAKIT_DIRECTSHOW != null)
                    {
                        thereWasOne = true;
                        medElement_MEDIAKIT_DIRECTSHOW.Close();
                        medElement_MEDIAKIT_DIRECTSHOW = null;
                    }
#endif //ENABLE_WPF_MEDIAKIT

                    if (thereWasOne)
                    {
                        videoAudioPanel.Children.RemoveAt(0);
                    }
                });

            FlowDocumentUnLoadedEvents.Add(reh2);
            m_FlowDoc.Unloaded += new RoutedEventHandler(reh2);


            return parent;
        }
    }
}
