﻿using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Practices.Composite.Logging;
using Tobi.Common;
using Tobi.Common.MVVM;
using Tobi.Common.MVVM.Command;
using urakawa.core;

namespace Tobi.Plugin.AudioPane
{
    public partial class AudioPaneViewModel
    {
        public RichDelegateCommand CommandOpenFile { get; private set; }
        public RichDelegateCommand CommandInsertFile { get; private set; }
        public RichDelegateCommand CommandDeleteAudioSelection { get; private set; }

        private void initializeCommands_Edit()
        {
            CommandOpenFile = new RichDelegateCommand(
                UserInterfaceStrings.Audio_OpenFile,
                UserInterfaceStrings.Audio_OpenFile_,
                null, // KeyGesture obtained from settings (see last parameters below)
                m_ShellView.LoadTangoIcon("document-open"),
                () =>
                {
                    Logger.Log("AudioPaneViewModel.CommandOpenFile", Category.Debug, Priority.Medium);

                    State.Audio.PcmFormatAlt = null;
                    openFile(null, false, false);
                },
                () => !IsWaveFormLoading && !IsMonitoring && !IsRecording,
                Settings_KeyGestures.Default,
                PropertyChangedNotifyBase.GetMemberName(() => Settings_KeyGestures.Default.Keyboard_Audio_OpenFile));

            m_ShellView.RegisterRichCommand(CommandOpenFile);
            //
            CommandInsertFile = new RichDelegateCommand(
                UserInterfaceStrings.Audio_InsertFile,
                UserInterfaceStrings.Audio_InsertFile_,
                null, // KeyGesture obtained from settings (see last parameters below)
                m_ShellView.LoadGnomeNeuIcon("Neu_go-jump"),
                () =>
                {
                    Logger.Log("AudioPaneViewModel.CommandInsertFile", Category.Debug, Priority.Medium);

                    State.Audio.PcmFormatAlt = null;
                    openFile(null, true, false);
                },
                () =>
                {
                    return !IsWaveFormLoading && !IsPlaying && !IsMonitoring && !IsRecording
                        && m_UrakawaSession.DocumentProject != null && State.CurrentTreeNode != null
                        && IsAudioLoaded;
                },
                Settings_KeyGestures.Default,
                PropertyChangedNotifyBase.GetMemberName(() => Settings_KeyGestures.Default.Keyboard_Audio_InsertFile));

            m_ShellView.RegisterRichCommand(CommandInsertFile);
            //
            CommandDeleteAudioSelection = new RichDelegateCommand(
                UserInterfaceStrings.Audio_Delete,
                UserInterfaceStrings.Audio_Delete_,
                null, // KeyGesture obtained from settings (see last parameters below)
                m_ShellView.LoadGnomeNeuIcon("Neu_dialog-cancel"),
                () =>
                {
                    Logger.Log("AudioPaneViewModel.CommandDeleteAudioSelection", Category.Debug, Priority.Medium);

                    long byteSelectionLeft = State.Audio.ConvertMillisecondsToBytes(State.Selection.SelectionBegin);
                    long byteSelectionRight = State.Audio.ConvertMillisecondsToBytes(State.Selection.SelectionEnd);

                    //long byteLastPlayHeadTime = State.Audio.ConvertMillisecondsToBytes(LastPlayHeadTime);

                    var listOfTreeNodeAndStreamSelection = new List<TreeNodeAndStreamSelection>();


                    long bytesToMatch = byteSelectionLeft;
                    long bytesRight = 0;
                    long bytesLeft = 0;
                    int index = -1;
                    foreach (TreeNodeAndStreamDataLength marker in State.Audio.PlayStreamMarkers)
                    {
                        index++;
                        bytesRight += marker.m_LocalStreamDataLength;
                        if (bytesToMatch < bytesRight
                        || index == (State.Audio.PlayStreamMarkers.Count - 1) && bytesToMatch >= bytesRight)
                        {
                            if (listOfTreeNodeAndStreamSelection.Count == 0)
                            {
                                bool rightBoundaryIsAlsoHere = (byteSelectionRight < bytesRight
                                                                ||
                                                                index == (State.Audio.PlayStreamMarkers.Count - 1) &&
                                                                byteSelectionRight >= bytesRight);

                                TreeNodeAndStreamSelection data = new TreeNodeAndStreamSelection()
                                {
                                    m_TreeNode = marker.m_TreeNode,
                                    m_LocalStreamLeftMark = byteSelectionLeft - bytesLeft,
                                    m_LocalStreamRightMark = (rightBoundaryIsAlsoHere ? byteSelectionRight - bytesLeft : -1)
                                };
                                listOfTreeNodeAndStreamSelection.Add(data);

                                if (rightBoundaryIsAlsoHere)
                                {
                                    break;
                                }
                                else
                                {
                                    bytesToMatch = byteSelectionRight;
                                }
                            }
                            else
                            {
                                TreeNodeAndStreamSelection data = new TreeNodeAndStreamSelection()
                                {
                                    m_TreeNode = marker.m_TreeNode,
                                    m_LocalStreamLeftMark = -1,
                                    m_LocalStreamRightMark = byteSelectionRight - bytesLeft
                                };
                                listOfTreeNodeAndStreamSelection.Add(data);

                                break;
                            }
                        }
                        else if (listOfTreeNodeAndStreamSelection.Count > 0)
                        {
                            TreeNodeAndStreamSelection data = new TreeNodeAndStreamSelection()
                            {
                                m_TreeNode = marker.m_TreeNode,
                                m_LocalStreamLeftMark = -1,
                                m_LocalStreamRightMark = -1
                            };
                            listOfTreeNodeAndStreamSelection.Add(data);
                        }

                        bytesLeft = bytesRight;
                    }

                    if (listOfTreeNodeAndStreamSelection.Count == 0)
                    {
                        Debug.Fail("This should never happen !");
                        return;
                    }

                    if (listOfTreeNodeAndStreamSelection.Count == 1)
                    {
                        var command = m_UrakawaSession.DocumentProject.Presentations.Get(0).CommandFactory.
                                    CreateTreeNodeAudioStreamDeleteCommand(listOfTreeNodeAndStreamSelection[0]);

                        m_UrakawaSession.DocumentProject.Presentations.Get(0).UndoRedoManager.Execute(command);
                    }
                    else
                    {
                        m_UrakawaSession.DocumentProject.Presentations.Get(0).UndoRedoManager.StartTransaction("Delete spanning audio portion", "Delete a portion of audio that spans across several treenodes");

                        foreach (TreeNodeAndStreamSelection selection in listOfTreeNodeAndStreamSelection)
                        {
                            var command = m_UrakawaSession.DocumentProject.Presentations.Get(0).CommandFactory.
                                        CreateTreeNodeAudioStreamDeleteCommand(selection);

                            m_UrakawaSession.DocumentProject.Presentations.Get(0).UndoRedoManager.Execute(command);
                        }

                        m_UrakawaSession.DocumentProject.Presentations.Get(0).UndoRedoManager.EndTransaction();
                    }
                },
                () =>
                {
                    return !IsWaveFormLoading
                        && !IsPlaying && !IsMonitoring && !IsRecording
                        && m_UrakawaSession.DocumentProject != null
                        && State.CurrentTreeNode != null
                        && IsAudioLoaded && IsSelectionSet;
                },
                Settings_KeyGestures.Default,
                PropertyChangedNotifyBase.GetMemberName(() => Settings_KeyGestures.Default.Keyboard_Audio_Delete));

            m_ShellView.RegisterRichCommand(CommandDeleteAudioSelection);
            //
        }
    }
}
