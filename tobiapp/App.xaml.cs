﻿using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Navigation;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using Sid.Windows.Controls;
using Tobi.Infrastructure;
using Tobi.Infrastructure.UI;

namespace Tobi
{
    /// <summary>
    /// The application delegates to the Composite WPF <see cref="Bootstrapper"/>
    /// </summary>
    public partial class App
    {
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ((TextBox)sender).SelectAll();
        }

        public SplashScreen SplashScreen
        {
            get; set;
        }

        ///<summary>
        /// Implements 2 runtimes: DEBUG and RELEASE
        ///</summary>
        ///<param name="e"></param>
        protected override void OnStartup(StartupEventArgs e)
        {
            PresentationTraceSources.ResourceDictionarySource.Listeners.Add(new ConsoleTraceListener());
            PresentationTraceSources.ResourceDictionarySource.Switch.Level = SourceLevels.All;
            PresentationTraceSources.DataBindingSource.Listeners.Add(new ConsoleTraceListener());
            PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Error;
            PresentationTraceSources.DependencyPropertySource.Listeners.Add(new ConsoleTraceListener());
            PresentationTraceSources.DependencyPropertySource.Switch.Level = SourceLevels.All;
            PresentationTraceSources.DocumentsSource.Listeners.Add(new ConsoleTraceListener());
            PresentationTraceSources.DocumentsSource.Switch.Level = SourceLevels.All;
            PresentationTraceSources.MarkupSource.Listeners.Add(new ConsoleTraceListener());
            PresentationTraceSources.MarkupSource.Switch.Level = SourceLevels.All;
            PresentationTraceSources.NameScopeSource.Listeners.Add(new ConsoleTraceListener());
            PresentationTraceSources.NameScopeSource.Switch.Level = SourceLevels.All;

            SplashScreen = new SplashScreen("TobiSplashScreen.png");
            SplashScreen.Show(false);

            base.OnStartup(e);

            ShutdownMode = ShutdownMode.OnMainWindowClose;

            FrameworkElement.LanguageProperty.OverrideMetadata(
                  typeof(FrameworkElement),
                  new FrameworkPropertyMetadata(
                     XmlLanguage.GetLanguage(
                     CultureInfo.CurrentCulture.IetfLanguageTag)));
            
            EventManager.RegisterClassHandler(typeof(TextBox),
                UIElement.GotFocusEvent,
                new RoutedEventHandler(TextBox_GotFocus));

#if (DEBUG)
            runInDebugMode();
#else
            runInReleaseMode();
#endif
        }

        private static void runInDebugMode()
        {
            var bootstrapper = new Bootstrapper();
            bootstrapper.Run();
        }

        private static void runInReleaseMode()
        {
            AppDomain.CurrentDomain.UnhandledException += appDomainUnhandledException;
            try
            {
                var bootstrapper = new Bootstrapper();
                bootstrapper.Run();
            }
            catch (Exception ex)
            {
                handleException(ex);
            }
        }

        private static void appDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            handleException(e.ExceptionObject as Exception);
        }

        public static void handleException(Exception ex)
        {
            if (ex == null)
                return;

            //ExceptionPolicy.HandleException(ex, "Default Policy");
            //MessageBox.Show(UserInterfaceStrings.UnhandledException);
            //TaskDialog.ShowException("Unhandled Exception !", UserInterfaceStrings.UnhandledException, ex);


            var margin = new Thickness(10, 10, 10, 0);

            var panel = new DockPanel {LastChildFill = true};

            string logPath = Directory.GetCurrentDirectory() + @"\Tobi.log";

            var labelMsg = new TextBox
                            {
                                FontWeight = FontWeights.ExtraBlack,
                                Text = UserInterfaceStrings.UnhandledException + String.Format("\n[{0}]", logPath),
                                Margin = margin,
                                HorizontalAlignment = HorizontalAlignment.Stretch,
                                VerticalAlignment = VerticalAlignment.Top,
                                TextWrapping = TextWrapping.Wrap,
                                IsReadOnly = true
                            };
            labelMsg.SetValue(DockPanel.DockProperty, Dock.Top);
            panel.Children.Add(labelMsg);

            var labelSummary = new TextBox
            {
                Text = ex.Message,
                Margin = margin,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                TextWrapping = TextWrapping.Wrap,
                IsReadOnly = true
            };
            labelSummary.SetValue(DockPanel.DockProperty, Dock.Top);
            panel.Children.Add(labelSummary);

            var stackTrace = new TextBox
                                 {
                                     Text = ex.StackTrace,
                                     HorizontalAlignment = HorizontalAlignment.Stretch,
                                     VerticalAlignment = VerticalAlignment.Stretch,
                                     TextWrapping = TextWrapping.Wrap,
                                     IsReadOnly = true,
                                 };

            var scroll = new ScrollViewer
                             {
                                 Content = stackTrace,
                                 Margin = margin,
                                 HorizontalAlignment = HorizontalAlignment.Stretch,
                                 VerticalAlignment = VerticalAlignment.Stretch,
                             };
            panel.Children.Add(scroll);

            /*
            var logStr = String.Format("CANNOT OPEN [{0}] !", logPath);

            var logFile = File.Open(logPath, FileMode.Open, FileAccess.Read);
            if (logFile.CanRead)
            {
                logFile.Close();
                //logFile.Read(bytes, int, int)
                logStr = File.ReadAllText(logPath);
            }

            var log = new TextBlock
            {
                Text = logStr,
                Margin = new Thickness(15),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                TextWrapping = TextWrapping.Wrap,
            };

            var scroll2 = new ScrollViewer
            {
                Content = log,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            panel.Children.Add(scroll2);
             * */

            var windowPopup = new PopupModalWindow(Current.MainWindow,
                                                   UserInterfaceStrings.EscapeMnemonic(
                                                       UserInterfaceStrings.Exit),
                                                   panel,
                                                   PopupModalWindow.DialogButtonsSet.Ok,
                                                   PopupModalWindow.DialogButton.Ok,
                                                   false)
            {
                Height = 300,
                Width = 500
            };

            SystemSounds.Exclamation.Play();
            windowPopup.Show();

            if (windowPopup.ClickedDialogButton == PopupModalWindow.DialogButton.Ok)
            {
                Environment.Exit(1);
            }
        }

        private void OnApplicationLoadCompleted(object sender, NavigationEventArgs e)
        {
            bool debug = true;
        }

        private void OnApplicationStartup(object sender, StartupEventArgs e)
        {
            bool debug = true;
        }
    }
}