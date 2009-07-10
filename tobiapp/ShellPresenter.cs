﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Composite.Logging;
using Microsoft.Practices.Composite.Regions;
using Microsoft.Practices.Unity;
using Tobi.Infrastructure;
using Tobi.Infrastructure.Commanding;
using Tobi.Infrastructure.UI;

namespace Tobi
{
    public class ShellPresenter : IShellPresenter
    {
        private void playAudioCue(string audioClipName)
        {
            string audioClipPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                               audioClipName);
            if (File.Exists(audioClipPath))
            {
                new SoundPlayer(audioClipPath).Play();
            }
        }

        public void PlayAudioCueTock()
        {
            playAudioCue("tock.wav");
        }

        public void PlayAudioCueTockTock()
        {
            playAudioCue("tocktock.wav");
        }

        // To avoid the shutting-down loop in OnShellWindowClosing()
        private bool m_Exiting;

        public RichDelegateCommand<object> ExitCommand { get; private set; }

        public RichDelegateCommand<object> MagnifyUiIncreaseCommand { get; private set; }
        public RichDelegateCommand<object> MagnifyUiDecreaseCommand { get; private set; }

        public RichDelegateCommand<object> ManageShortcutsCommand { get; private set; }

        public RichDelegateCommand<object> UndoCommand { get; private set; }
        public RichDelegateCommand<object> RedoCommand { get; private set; }

        public RichDelegateCommand<object> CopyCommand { get; private set; }
        public RichDelegateCommand<object> CutCommand { get; private set; }
        public RichDelegateCommand<object> PasteCommand { get; private set; }

        public RichDelegateCommand<object> HelpCommand { get; private set; }
        public RichDelegateCommand<object> PreferencesCommand { get; private set; }
        public RichDelegateCommand<object> WebHomeCommand { get; private set; }

        public RichDelegateCommand<object> NavNextCommand { get; private set; }
        public RichDelegateCommand<object> NavPreviousCommand { get; private set; }

        public IShellView View { get; private set; }
        protected ILoggerFacade Logger { get; private set; }
        protected IRegionManager RegionManager { get; private set; }

        protected IUnityContainer Container { get; private set; }
        protected IEventAggregator EventAggregator { get; private set; }

        ///<summary>
        /// Default constructor
        ///</summary>
        ///<param name="view"></param>
        public ShellPresenter(IShellView view, ILoggerFacade logger,
                            IRegionManager regionManager, IUnityContainer container,
                            IEventAggregator eventAggregator
                            )
        {
            m_Exiting = false;

            View = view;
            Logger = logger;
            Container = container;
            RegionManager = regionManager;
            EventAggregator = eventAggregator;

            Logger.Log("ShellPresenter.ctor", Category.Debug, Priority.Medium);

            App.LOGGER = Logger;

            initCommands();
        }


        private void initCommands()
        {
            Logger.Log("ShellPresenter.initCommands", Category.Debug, Priority.Medium);

            //
            ExitCommand = new RichDelegateCommand<object>(UserInterfaceStrings.Menu_Exit,
                                                                      UserInterfaceStrings.Menu_Exit_,
                                                                      UserInterfaceStrings.Menu_Exit_KEYS,
                                                                      RichDelegateCommand<object>.ConvertIconFormat((DrawingImage)Application.Current.FindResource("Horizon_Image_Exit")),
                //(VisualBrush)Application.Current.FindResource("document-save"),
                                                            ExitCommand_Executed, obj => true);
            RegisterRichCommand(ExitCommand);
            //

            MagnifyUiIncreaseCommand = new RichDelegateCommand<object>(UserInterfaceStrings.UI_IncreaseMagnification,
                                                                       UserInterfaceStrings.UI_IncreaseMagnification_,
                                                                      UserInterfaceStrings.UI_IncreaseMagnification_KEYS,
                                                                      RichDelegateCommand<object>.ConvertIconFormat((DrawingImage)Application.Current.FindResource("Horizon_Image_Zoom_In")),
                                                            obj => MagnifyUi(0.15), obj => true);
            RegisterRichCommand(MagnifyUiIncreaseCommand);
            //

            MagnifyUiDecreaseCommand = new RichDelegateCommand<object>(UserInterfaceStrings.UI_DecreaseMagnification,
                                                                      UserInterfaceStrings.UI_DecreaseMagnification_,
                                                                      UserInterfaceStrings.UI_DecreaseMagnification_KEYS,
                                                                      RichDelegateCommand<object>.ConvertIconFormat((DrawingImage)Application.Current.FindResource("Horizon_Image_Zoom_out")),
                                                            obj => MagnifyUi(-0.15), obj => true);
            RegisterRichCommand(MagnifyUiDecreaseCommand);
            //

            ManageShortcutsCommand = new RichDelegateCommand<object>(UserInterfaceStrings.ManageShortcuts,
                                                                      UserInterfaceStrings.ManageShortcuts_,
                                                                      UserInterfaceStrings.ManageShortcuts_KEYS,
                                                                      (VisualBrush)Application.Current.FindResource("preferences-desktop-keyboard-shortcuts"),
                                                            obj => manageShortcuts(), obj => true);
            RegisterRichCommand(ManageShortcutsCommand);
            //
            UndoCommand = new RichDelegateCommand<object>(UserInterfaceStrings.Undo,
                UserInterfaceStrings.Undo_,
                UserInterfaceStrings.Undo_KEYS,
                (VisualBrush)Application.Current.FindResource("edit-undo"),
                obj => { throw new NotImplementedException("Functionality not implemented, sorry :("); }, obj => true);

            RegisterRichCommand(UndoCommand);
            //
            RedoCommand = new RichDelegateCommand<object>(UserInterfaceStrings.Redo,
                UserInterfaceStrings.Redo_,
                UserInterfaceStrings.Redo_KEYS,
                (VisualBrush)Application.Current.FindResource("edit-redo"),
                obj => { throw new NotImplementedException("Functionality not implemented, sorry :("); }, obj => true);

            RegisterRichCommand(RedoCommand);
            //
            CutCommand = new RichDelegateCommand<object>(UserInterfaceStrings.Cut,
                UserInterfaceStrings.Cut_,
                UserInterfaceStrings.Cut_KEYS,
                (VisualBrush)Application.Current.FindResource("edit-cut"),
                obj => { throw new NotImplementedException("Functionality not implemented, sorry :("); }, obj => true);

            RegisterRichCommand(CutCommand);
            //
            CopyCommand = new RichDelegateCommand<object>(UserInterfaceStrings.Copy,
                UserInterfaceStrings.Copy_,
                UserInterfaceStrings.Copy_KEYS,
                (VisualBrush)Application.Current.FindResource("edit-copy"),
                obj => { throw new NotImplementedException("Functionality not implemented, sorry :("); }, obj => true);

            RegisterRichCommand(CopyCommand);
            //
            PasteCommand = new RichDelegateCommand<object>(UserInterfaceStrings.Paste,
                UserInterfaceStrings.Paste_,
                UserInterfaceStrings.Paste_KEYS,
                (VisualBrush)Application.Current.FindResource("edit-paste"),
                obj => { throw new NotImplementedException("Functionality not implemented, sorry :("); }, obj => true);

            RegisterRichCommand(PasteCommand);
            //
            HelpCommand = new RichDelegateCommand<object>(UserInterfaceStrings.Help,
                UserInterfaceStrings.Help_,
                UserInterfaceStrings.Help_KEYS,
                (VisualBrush)Application.Current.FindResource("help-browser"),
                obj => { throw new NotImplementedException("Functionality not implemented, sorry :("); }, obj => true);

            RegisterRichCommand(HelpCommand);
            //
            PreferencesCommand = new RichDelegateCommand<object>(UserInterfaceStrings.Preferences,
                UserInterfaceStrings.Preferences_,
                UserInterfaceStrings.Preferences_KEYS,
                (VisualBrush)Application.Current.FindResource("preferences-system"),
                obj => { throw new NotImplementedException("Functionality not implemented, sorry :("); }, obj => true);

            RegisterRichCommand(PreferencesCommand);
            //
            WebHomeCommand = new RichDelegateCommand<object>(UserInterfaceStrings.WebHome,
                UserInterfaceStrings.WebHome_,
                UserInterfaceStrings.WebHome_KEYS,
                RichDelegateCommand<object>.ConvertIconFormat((DrawingImage)Application.Current.FindResource("Home_icon")),
                //(VisualBrush)Application.Current.FindResource("go-home"),
                obj => { throw new NotImplementedException("Functionality not implemented, sorry :("); }, obj => true);

            RegisterRichCommand(WebHomeCommand);
            //
            NavNextCommand = new RichDelegateCommand<object>(UserInterfaceStrings.NavNext,
                UserInterfaceStrings.NavNext_,
                UserInterfaceStrings.NavNext_KEYS,
                RichDelegateCommand<object>.ConvertIconFormat((DrawingImage)Application.Current.FindResource("Horizon_Image_Forward")),
                obj => { throw new NotImplementedException("Functionality not implemented, sorry :("); }, obj => true);

            RegisterRichCommand(NavNextCommand);
            //
            NavPreviousCommand = new RichDelegateCommand<object>(UserInterfaceStrings.NavPrevious,
                UserInterfaceStrings.NavPrevious_,
                UserInterfaceStrings.NavPrevious_KEYS,
                RichDelegateCommand<object>.ConvertIconFormat((DrawingImage)Application.Current.FindResource("Horizon_Image_Back")),
                obj => { throw new NotImplementedException("Functionality not implemented, sorry :("); }, obj => true);

            RegisterRichCommand(NavPreviousCommand);
            //
        }

        private void manageShortcuts()
        {
            Logger.Log("ShellPresenter.manageShortcuts", Category.Debug, Priority.Medium);

            var window = View as Window;

            var windowPopup = new PopupModalWindow(window ?? Application.Current.MainWindow,
                                                   UserInterfaceStrings.EscapeMnemonic(
                                                       UserInterfaceStrings.ManageShortcuts),
                                                   new KeyboardShortcuts(this),
                                                   PopupModalWindow.DialogButtonsSet.Ok,
                                                   PopupModalWindow.DialogButton.Ok,
                                                   true, 500, 600);
            windowPopup.Show();

            /*
            var windowPopup = new Window()
            {
                Owner = (window ?? Application.Current.MainWindow),
                ShowInTaskbar = false,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Title = UserInterfaceStrings.EscapeMnemonic(UserInterfaceStrings.ManageShortcuts),
                Height = 600,
                Width = 500,
                Content = new KeyboardShortcuts(this)
            };
            windowPopup.ShowDialog();
             * */

            /*
        if (window != null)
        {
            var dialog = new TaskDialog();
            dialog.MaxWidth = 600;
            dialog.MaxHeight = 500;
            dialog.TopMost = InteropWindowZOrder.TopMost;
            dialog.TaskDialogWindow.Title = UserInterfaceStrings.EscapeMnemonic(UserInterfaceStrings.ManageShortcuts);
            dialog.TaskDialogButton = TaskDialogButton.Custom;
            dialog.Button1Text = "_Ok";
            dialog.DefaultResult = TaskDialogResult.Button1;
            dialog.IsButton1Cancel = true;
            dialog.Content = new KeyboardShortcuts(this);
            dialog.Show();
                
        } * */

        }

        private void MagnifyUi(double value)
        {
            View.MagnificationLevel += value;
        }

        private void exit()
        {
            Logger.Log("ShellPresenter.exit", Category.Debug, Priority.Medium);

            //MessageBox.Show("Good bye !", "Tobi says:");
            /*TaskDialog.Show("Tobi is exiting.",
                "Just saying goodbye...",
                "The Tobi application is now closing.",
                TaskDialogIcon.Information);*/
            m_Exiting = true;
            Application.Current.Shutdown();
        }

        public bool OnShellWindowClosing()
        {
            Logger.Log("ShellPresenter.OnShellWindowClosing", Category.Debug, Priority.Medium);

            if (m_Exiting) return true;

            if (ExitCommand.CanExecute(null))
            {
                if (askUserConfirmExit())
                {
                    exit();
                    return true;
                }
            }

            return false;
        }

        private bool askUserConfirmExit()
        {
            Logger.Log("ShellPresenter.askUserConfirmExit", Category.Debug, Priority.Medium);

            /*
            try
            {
                throw new ArgumentException("Opps !", new ArgumentOutOfRangeException("Oops 2 !!"));
            }
            catch (Exception ex)
            {
                App.handleException(ex);
            }*/

            var window = View as Window;

            var label = new TextBlock
                            {
                                Text = UserInterfaceStrings.ExitConfirm,
                                Margin = new Thickness(8, 0, 8, 0),
                                HorizontalAlignment = HorizontalAlignment.Center,
                                VerticalAlignment = VerticalAlignment.Center,
                                Focusable = false,
                            };

            var fakeCommand = new RichDelegateCommand<object>(null,
                null,
                null,
                (VisualBrush)Application.Current.FindResource("help-browser"),
                null, obj => true);

            var panel = new StackPanel
                            {
                                Orientation = Orientation.Horizontal,
                                HorizontalAlignment = HorizontalAlignment.Center,
                                VerticalAlignment = VerticalAlignment.Stretch,
                            };
            panel.Children.Add(fakeCommand.IconLarge);
            panel.Children.Add(label);
            //panel.Margin = new Thickness(8, 8, 8, 0);

            var details = new TextBox
            {
                Text = "Any unsaved changes in your document will be lost !",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                TextWrapping = TextWrapping.Wrap,
                IsReadOnly = true,
                Background = SystemColors.ControlLightLightBrush,
                BorderBrush = SystemColors.ControlDarkDarkBrush,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(6),
                SnapsToDevicePixels = true
            };

            var windowPopup = new PopupModalWindow(window ?? Application.Current.MainWindow,
                                                   UserInterfaceStrings.EscapeMnemonic(
                                                       UserInterfaceStrings.Exit),
                                                   panel,
                                                   PopupModalWindow.DialogButtonsSet.YesNo,
                                                   PopupModalWindow.DialogButton.No,
                                                   true, 300, 160, details, 40);

            fakeCommand.IconDrawScale = View.MagnificationLevel;

            windowPopup.Show();

            if (windowPopup.ClickedDialogButton == PopupModalWindow.DialogButton.Yes)
            {
                return true;
            }

            return false;


            if (window != null)
            {
                /*MessageBoxResult result = MessageBox.Show(window, "Confirm quit ?", "Tobi asks:",
                                                          MessageBoxButton.OKCancel);
                if (result != MessageBoxResult.OK)
                {
                    return false;
                }*/

                /*
                TaskDialogResult result = TaskDialog.Show(window, "Exit Tobi ?",
                    "Are you sure you want to exit Tobi ?",
                    "Press OK to exit, CANCEL to return to the application.",
                    "You can use the ESCAPE, ENTER or 'C' shortcut keys to cancel,\nor the 'O' shortcut key to confirm.",
                    "Please note that any unsaved work will be lost.",
                TaskDialogButton.OkCancel,
                TaskDialogResult.Cancel,
                TaskDialogIcon.Question,
                TaskDialogIcon.Warning,
                Brushes.White,
                Brushes.Navy);

                if (result != TaskDialogResult.Ok)
                {
                    return false;
                }*/
            }
            return true;
        }

        private void ExitCommand_Executed(object parameter)
        {
            if (askUserConfirmExit())
            {
                exit();
            }
        }

        private readonly List<RichDelegateCommand<object>> m_listOfRegisteredRichCommands =
            new List<RichDelegateCommand<object>>();

        public List<RichDelegateCommand<object>> RegisteredRichCommands
        {
            get
            {
                return m_listOfRegisteredRichCommands;
            }
        }

        public void SetZoomValue(double value)
        {
            /*if (EventAggregator == null)
            {
                return;
            }
            EventAggregator.GetEvent<UserInterfaceScaledEvent>().Publish(value);
             */

            foreach (var command in m_listOfRegisteredRichCommands)
            {
                command.IconDrawScale = value;
            }
        }

        public void RegisterRichCommand(RichDelegateCommand<object> command)
        {
            m_listOfRegisteredRichCommands.Add(command);
            AddInputBinding(command.KeyBinding);
        }

        public void UnRegisterRichCommand(RichDelegateCommand<object> command)
        {
            m_listOfRegisteredRichCommands.Remove(command);
            RemoveInputBinding(command.KeyBinding);
        }

        public bool AddInputBinding(InputBinding inputBinding)
        {
            Logger.Log("ShellPresenter.AddInputBinding", Category.Debug, Priority.Medium);

            var window = View as Window;
            if (window != null && inputBinding != null)
            {
                logInputBinding(inputBinding);
                window.InputBindings.Add(inputBinding);
                return true;
            }

            return false;
        }

        public void RemoveInputBinding(InputBinding inputBinding)
        {
            Logger.Log("ShellPresenter.RemoveInputBinding", Category.Debug, Priority.Medium);

            var window = View as Window;
            if (window != null && inputBinding != null)
            {
                logInputBinding(inputBinding);
                window.InputBindings.Remove(inputBinding);
            }
        }

        private void logInputBinding(InputBinding inputBinding)
        {
            if (inputBinding.Gesture is KeyGesture)
            {
                Logger.Log(
                    "KeyBinding (" +
                    ((KeyGesture)(inputBinding.Gesture)).GetDisplayStringForCulture(CultureInfo.CurrentCulture) + ")",
                    Category.Debug, Priority.Medium);
            }
            else
            {
                Logger.Log(
                       "InputBinding (" +
                       inputBinding.Gesture + ")",
                       Category.Debug, Priority.Medium);
            }
        }
    }
}

/*
 
        public void ToggleView(bool? show, IToggableView view)
        {
            Logger.Log("ShellPresenter.ToggleView", Category.Debug, Priority.Medium);

            var region = RegionManager.Regions[view.RegionName];
            var isVisible = region.ActiveViews.Contains(view);

            var makeVisible = true;
            switch (show)
            {
                case null:
                    {
                        makeVisible = !isVisible;
                    }
                    break;
                default:
                    {
                        makeVisible = (bool)show;
                    }
                    break;
            }
            if (makeVisible)
            {
                if (!isVisible)
                {
                    region.Add(view);
                    region.Activate(view);
                }
                view.FocusControl();
            }
            else if (isVisible)
            {
                region.Deactivate(view);
                region.Remove(view);
            }

            var menuView = Container.Resolve<MenuBarView>();
            menuView.EnsureViewMenuCheckState(view.RegionName, makeVisible);
        }

 */