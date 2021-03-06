﻿using System;
using System.ComponentModel.Composition;
using System.Windows.Input;
using Microsoft.Practices.Composite.Logging;
using Microsoft.Practices.Unity;
using Tobi.Common;
using Tobi.Common.MVVM;
using Tobi.Common.MVVM.Command;
using Tobi.Common.UI;

namespace Tobi.Plugin.Settings
{
    ///<summary>
    /// The application settings / configuration / options includes a top-level UI to enable user edits
    ///</summary>
    public sealed class SettingsPlugin : AbstractTobiPlugin
    {
        private readonly IUnityContainer m_Container;
        private readonly IShellView m_ShellView;

        //private readonly SettingsView m_SettingsView;

        private readonly IUrakawaSession m_UrakawaSession;
        public readonly ISettingsAggregator m_SettingsAggregator;

        private readonly ILoggerFacade m_Logger;

        ///<summary>
        /// We inject a few dependencies in this constructor.
        /// The Initialize method is then normally called by the bootstrapper of the plugin framework.
        ///</summary>
        ///<param name="logger">normally obtained from the Unity dependency injection container, it's a built-in CAG service</param>
        ///<param name="container">normally obtained from the Unity dependency injection container, it's a built-in CAG service</param>
        ///<param name="shellView">normally obtained from the MEF composition container, it's a Tobi-specific service</param>
        ///<param name="settingsAggregator">normally obtained from the MEF composition container, it's a Tobi-specific service</param>
        [ImportingConstructor]
        public SettingsPlugin(
            ILoggerFacade logger,
            IUnityContainer container,
            [Import(typeof(IShellView), RequiredCreationPolicy = CreationPolicy.Shared, AllowDefault = false)]
            IShellView shellView,
            //[Import(typeof(SettingsView), RequiredCreationPolicy = CreationPolicy.Shared, AllowDefault = false)]
            //SettingsView view,
            [Import(typeof(IUrakawaSession), RequiredCreationPolicy = CreationPolicy.Shared, AllowDefault = false)]
            IUrakawaSession session,
            [Import(typeof(ISettingsAggregator), RequiredCreationPolicy = CreationPolicy.Shared, AllowRecomposition = false)]
            ISettingsAggregator settingsAggregator)
        {
            m_Logger = logger;
            m_Container = container;
            m_ShellView = shellView;

            //m_SettingsView = view;
            m_UrakawaSession = session;

            m_SettingsAggregator = settingsAggregator;

            CommandShowSettings = new RichDelegateCommand(
                Tobi_Plugin_Settings_Lang.Cmd_ApplicationPref,
                Tobi_Plugin_Settings_Lang.Cmd_DisplayEditor,
                null, // KeyGesture obtained from settings (see last parameters below)
                m_ShellView.LoadTangoIcon(@"preferences-system"),
                ShowDialog,
                CanShowDialog,
                Settings_KeyGestures.Default,
                PropertyChangedNotifyBase.GetMemberName(() => Settings_KeyGestures.Default.Keyboard_EditPreferences));

            m_ShellView.RegisterRichCommand(CommandShowSettings);

            //m_Logger.Log(@"SettingsPlugin init", Category.Debug, Priority.Medium);
        }
    
        private readonly RichDelegateCommand CommandShowSettings;

        private int m_ToolBarId_1;
        protected override void OnToolBarReady()
        {
            m_ToolBarId_1 = m_ToolBarsView.AddToolBarGroup(new[] { CommandShowSettings }, PreferredPosition.Last);

            m_Logger.Log(@"SettingsPlugin commands pushed to toolbar", Category.Debug, Priority.Medium);
        }

        private int m_MenuBarId_1;
        protected override void OnMenuBarReady()
        {
            m_MenuBarId_1 = m_MenuBarView.AddMenuBarGroup(
                Tobi_Common_Lang.Menu_Tools, PreferredPosition.Last, true,
                null, PreferredPosition.Any, true,
                new[] { CommandShowSettings });

            m_Logger.Log(@"SettingsPlugin commands pushed to menubar", Category.Debug, Priority.Medium);
        }

        public override void Dispose()
        {
            if (m_ToolBarsView != null)
            {
                m_ToolBarsView.RemoveToolBarGroup(m_ToolBarId_1);

                m_Logger.Log(@"SettingsPlugin commands removed from toolbar", Category.Debug, Priority.Medium);
            }

            if (m_MenuBarView != null)
            {
                m_MenuBarView.RemoveMenuBarGroup(Tobi_Common_Lang.Menu_Tools, m_MenuBarId_1);

                m_Logger.Log(@"SettingsPlugin commands removed from menubar", Category.Debug, Priority.Medium);
            }
        }

        public override string Name
        {
            get { return Tobi_Plugin_Settings_Lang.SettingsPlugin_Name; }
        }

        public override string Description
        {
            get { return Tobi_Plugin_Settings_Lang.SettingsPlugin_Description; }
        }

        private bool m_DialogIsShowing;

        private bool CanShowDialog()
        {
            return !m_DialogIsShowing && !m_UrakawaSession.isAudioRecording;
        }

        private void ShowDialog()
        {
            m_Logger.Log("SettingsPlugin.ShowDialog", Category.Debug, Priority.Medium);

            var view = m_Container.Resolve<SettingsView>();
            
            var windowPopup = new PopupModalWindow(m_ShellView,
                                                   UserInterfaceStrings.EscapeMnemonic(Tobi_Plugin_Settings_Lang.Preferences),
                                                   view,
                                                   PopupModalWindow.DialogButtonsSet.OkCancel,
                                                   PopupModalWindow.DialogButton.Cancel,
                                                   true, 800, 500, null, 0,null);
            windowPopup.IgnoreEscape = true;

            //view.OwnerWindow = windowPopup;

            m_SettingsAggregator.SaveAll(); // Not strictly necessary..but just to make double-sure we've got the current settings in persistent storage.

            windowPopup.ShowFloating(null);

            windowPopup.Closed += (o, args) =>
                                      {
                                          m_DialogIsShowing = false;

                                          // This line is not strictly necessary, but this way we make sure the CanShowDialog method (CanExecute) is called to refresh the command visual enabled/disabled status.
                                          CommandManager.InvalidateRequerySuggested();

                                          if (windowPopup.ClickedDialogButton == PopupModalWindow.DialogButton.Ok)
                                          {
                                              m_SettingsAggregator.SaveAll();
                                          }
                                          else
                                          {
                                              m_SettingsAggregator.ReloadAll();
                                          }

                                          view = null;

                                          // TODO: view is not collected ! (at least in VS debugger)
                                          // despite PartCreationPolicy(CreationPolicy.NonShared)
                                          //GC.Collect();
                                          //GC.WaitForFullGCComplete();
                                      };

            m_DialogIsShowing = true;
        }
    }
}
