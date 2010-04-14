﻿using Tobi.Common;
using Tobi.Common.Validation;
using urakawa.core;

namespace Tobi.Plugin.Validator.MissingAudio
{
    public class MissingAudioValidationError : ValidationItem
    {
        public TreeNode Target { get; set;}
        
        public override string Message
        {
            get { return string.Format("Element <{0}> is missing audio", ValidatorUtilities.GetTreeNodeName(Target)); }
        }
        public override string CompleteSummary
        {
            get
            {
                return string.Format(@"Missing audio content
The element <{0}> has no associated audio content.
{1}", 
                        ValidatorUtilities.GetTreeNodeName(Target), 
                        ValidatorUtilities.GetNodeXml(Target, true)
                        );
            }
        }

        private IUrakawaSession m_UrakawaSession;
       
        public override void TakeAction()
        {
            m_UrakawaSession.PerformTreeNodeSelection(Target);
        }

        public MissingAudioValidationError(IUrakawaSession session)
        {
            m_UrakawaSession = session;
            Severity = ValidationSeverity.Error;
        }
    }
}