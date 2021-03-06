﻿using urakawa.core.visitor;
using urakawa.core;
using urakawa.property.channel;
using System.Windows.Documents;
using urakawa.media;
using urakawa.property.xml;
using System.Windows.Controls;
using System.Windows;

namespace FlowDocumentXmlEditor.FlowDocumentExtraction
{
    
    public abstract class GenericExtractionVisitor : ITreeNodeVisitor
    {
        private Channel mTextChannel;
        public Channel TextChannel { get { return mTextChannel; } }

        protected GenericExtractionVisitor(Channel textCh)
        {
            mTextChannel = textCh;
        }
        public static bool USE_TEXT_BOX_UIELEMENT = true;

        public Inline GetTextInline(TreeNode node)
        {
            if (TextChannel != null)
            {
                ChannelsProperty chProp = node.GetProperty<ChannelsProperty>();
                if (chProp != null)
                {
                    TextMedia text = chProp.GetMedia(TextChannel) as TextMedia;
                    
                    if (text != null)
                    {
                        if (USE_TEXT_BOX_UIELEMENT)
                        {
                            TextBox tb = new TextBox();

                            TextMediaBinding binding = new TextMediaBinding();
                            binding.BoundTextMedia = text;
                            binding.Mode = System.Windows.Data.BindingMode.TwoWay;
                            tb.SetBinding(TextBox.TextProperty, binding);

                            //tb.Text = text.getText();

                            tb.BorderThickness = new Thickness(1);
                            tb.Focusable = true;
                            InlineUIContainer res = new InlineUIContainer(tb);
                            return res;
                        }
                        else {
                            TextMediaBindableRun run = new TextMediaBindableRun(text);

                            //run.Text = text.getText();
                            return run;
                        }
                    }
                }
            }
            return null;
        }

        #region ITreeNodeVisitor Members

        public void PostVisit(TreeNode node)
        {
            //Do nothing
        }

        private long mTotalNumberOfNodes = -1;
        private long mCurrentNodeIndex = -1;

        private void countTotalNodes(TreeNode node, long currentNumberOfNodes, out long totalNumberOfNodes)
        {
            totalNumberOfNodes = currentNumberOfNodes + 1;
            foreach (TreeNode child in node.ListOfChildren) {
                countTotalNodes(child, totalNumberOfNodes, out totalNumberOfNodes);
            }
        }
        public virtual bool PreVisit(TreeNode node)
        {
            if (mTotalNumberOfNodes == -1 && node.Parent == null)
            {
                countTotalNodes(node, mTotalNumberOfNodes, out mTotalNumberOfNodes);
            }
            
            mCurrentNodeIndex++;

            Inline nodeRun = GetTextInline(node);
            XmlProperty xmlProp = node.GetProperty<XmlProperty>();
            if (xmlProp!=null)
            {
                if (!HandleXmlElement(xmlProp, node, nodeRun)) return false;
            }
            if (nodeRun!=null) HandleNodeRun(nodeRun);
            return true;
        }

        #endregion

        protected abstract bool HandleXmlElement(XmlProperty xmlProp, TreeNode node, Inline nodeRun);

        protected abstract void HandleNodeRun(Inline nodeRun);
    }
}
