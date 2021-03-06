﻿using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Composite.Logging;
using Tobi.Common;
using Tobi.Common.UI;
using urakawa.core;

namespace Tobi.Plugin.NavigationPane
{
    /// <summary>
    /// Interaction logic for MarkersPanelView.xaml
    /// </summary>
    [Export(typeof(MarkersPanelView)), PartCreationPolicy(CreationPolicy.Shared)]
    public partial class MarkersPanelView : ITobiViewFocusable // : IMarkersPaneView, IActiveAware
    {
        //private bool _ignoreMarkersSelected = false;
        private bool _ignoreTreeNodeSelectedEvent = false;

        public MarkersPaneViewModel ViewModel
        {
            get; private set;
        }
        private readonly IEventAggregator m_EventAggregator;
        private readonly ILoggerFacade m_Logger;
        private readonly IUrakawaSession m_UrakawaSession;

        [ImportingConstructor]
        public MarkersPanelView(
            IEventAggregator eventAggregator,
            ILoggerFacade logger,
            [Import(typeof(IUrakawaSession), RequiredCreationPolicy = CreationPolicy.Shared, AllowDefault = false)]
            IUrakawaSession urakawaSession,
            [Import(typeof(MarkersPaneViewModel), RequiredCreationPolicy = CreationPolicy.Shared, AllowDefault = false)]
            MarkersPaneViewModel viewModel)
        {
            m_UrakawaSession = urakawaSession;
            m_EventAggregator = eventAggregator;
            m_Logger = logger;

            ViewModel = viewModel;
            DataContext = ViewModel;

            InitializeComponent();
            ViewModel.SetView(this);
        }
        private void onMarkersSelected(object sender, SelectionChangedEventArgs e)
        {
            // do nothing here (to avoid selecting in the document and audio views whilst navigating/exploring the page list).
        }
        //public void UpdateMarkersListSelection(TreeNode node)
        //{
        //    //if (_ignoreMarkersSelected)
        //    //{
        //    //    _ignoreMarkersSelected = false;
        //    //    return;
        //    //}
        //    if (_ignoreTreeNodeSelectedEvent)
        //    {
        //        _ignoreTreeNodeSelectedEvent = false;
        //        return;
        //    }
        //    MarkedTreeNode prevMarkers = null;
        //    foreach (MarkedTreeNode mnode in ViewModel.MarkersNavigator.MarkedTreeNodes)
        //    {
        //        TreeNode treeNode = mnode.TreeNode;
        //        if (treeNode != null && treeNode.IsAfter(node))
        //        {
        //            MarkedTreeNode toSelect = prevMarkers ?? mnode;
        //            if (toSelect != ListView.SelectedItem)
        //            {
        //                //_ignoreMarkersSelected = true;
        //                ListView.SelectedItem = toSelect;
        //                ListView.ScrollIntoView(toSelect);
        //            }
        //            return;
        //        }
        //        prevMarkers = mnode;
        //    }
        //}

        public string ViewName
        {
            get { return Tobi_Plugin_NavigationPane_Lang.Marks; }
        }

        public void LoadProject()
        {
            //m_LastListItemSelected = null;
        }
        public void UnloadProject()
        {
            //m_LastListItemSelected = null;
            SearchBox.Text = "";
        }
        private void OnSearchLostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(SearchBox.Text))
            {
                ViewModel.IsSearchVisible = false;
            }
        }

        private void OnMouseClickCheckBox(object sender, RoutedEventArgs e)
        {
            var item = FocusableItem;
            if (item != null)
            {
                FocusHelper.FocusBeginInvoke(item, DispatcherPriority.Background);
            }
            //((UIElement)sender).Dispatcher.BeginInvoke(
            //    new Action(() =>
            //    {
            //        UIElement ui = FocusableItem;
            //        if (ui != null && ui.Focusable)
            //        {
            //            FocusHelper.Focus(FocusableItem);
            //        }
            //    }),
            //    DispatcherPriority.Background);
        }

        public UIElement FocusableItem
        {
            get
            {
                if (ListView.Focusable) return ListView;

                if (ListView.SelectedIndex != -1)
                {
                    return ListView.ItemContainerGenerator.ContainerFromIndex(ListView.SelectedIndex) as ListViewItem;
                }

                if (ListView.Items.Count > 0)
                {
                    return ListView.ItemContainerGenerator.ContainerFromIndex(0) as ListViewItem;
                }

                return null;
            }
        }

        //public UIElement ViewControl
        //{
        //    get { return this; }
        //}
        //public UIElement ViewFocusStart
        //{
        //    get { return ListView; }
        //}


        private void handleListCurrentSelection()
        {
            MarkedTreeNode mnode = ListView.SelectedItem as MarkedTreeNode;
            if (mnode == null) return;
            TreeNode treeNode = mnode.TreeNode;

            if (treeNode == null) return;


            //m_Logger.Log("-- PublishEvent [TreeNodeSelectedEvent] MarkersPanelView.OnMarkersSelected", Category.Debug, Priority.Medium);

            if (!m_UrakawaSession.isAudioRecording)
            {
                _ignoreTreeNodeSelectedEvent = true;
                m_UrakawaSession.PerformTreeNodeSelection(treeNode);
            }
            //m_EventAggregator.GetEvent<TreeNodeSelectedEvent>().Publish(treeNode);
        }

        private void OnKeyUp_ListItem(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                handleListCurrentSelection();
            }
        }

        private void OnMouseDoubleClick_ListItem(object sender, MouseButtonEventArgs e)
        {
            handleListCurrentSelection();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ViewModel.MarkersNavigator == null) { return; }
            ViewModel.MarkersNavigator.SearchTerm = SearchBox.Text;
        }

        //internal ListViewItem m_LastListItemSelected;

        //private void OnSelected_ListItem(object sender, RoutedEventArgs e)
        //{
        //    DebugFix.Assert(sender == e.Source);
        //    m_LastListItemSelected = (ListViewItem)sender;
        //}

        private void OnSearchBoxKeyUp(object sender, KeyEventArgs e)
        {
            var key = (e.Key == Key.System ? e.SystemKey : (e.Key == Key.ImeProcessed ? e.ImeProcessedKey : e.Key));
            
            if (key == Key.Return && ViewModel.CommandFindNextMarkers.CanExecute())
            {
                ViewModel.CommandFindNextMarkers.Execute();
            }

            if (key == Key.Escape)
            {
                SearchBox.Text = "";

                var item = FocusableItem;
                if (item != null)
                {
                    FocusHelper.FocusBeginInvoke(item);
                }
            }
        }

        private void OnUILoaded(object sender, RoutedEventArgs e)
        {
            var item = FocusableItem;
            if (item != null)
            {
                FocusHelper.FocusBeginInvoke(item);
            }
        }

        private void OnClick_ButtonRemoveAll(object sender, RoutedEventArgs e)
        {
            ViewModel.CommandRemoveAllMarks.Execute();
        }
    }
}
