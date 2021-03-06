﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Composite.Logging;
using Tobi.Common;
using Tobi.Common.Validation;
using System.ComponentModel.Composition;
using urakawa;
using urakawa.command;
using urakawa.commands;
using urakawa.events.undo;
using urakawa.metadata.daisy;
using urakawa.metadata;
using System;
#if DEBUG
using AudioLib;
using urakawa.undo;
#endif
using urakawa.undo;

namespace Tobi.Plugin.Validator.Metadata
{
    /// <summary>
    /// The main validator class
    /// </summary>
    [Export(typeof(MetadataValidator)), PartCreationPolicy(CreationPolicy.Shared)]
    public class MetadataValidator : AbstractValidator, IPartImportsSatisfiedNotification, UndoRedoManager.Hooker.Host
    {
#pragma warning disable 1591 // non-documented method
        public void OnImportsSatisfied()
#pragma warning restore 1591
        {
            //#if DEBUG
            //            Debugger.Break();
            //#endif
        }

        private readonly ILoggerFacade m_Logger;
        protected readonly IUrakawaSession m_Session;

        private ResourceDictionary m_ValidationItemTemplate;

        ///<summary>
        /// We inject a few dependencies in this constructor.
        /// The Initialize method is then normally called by the bootstrapper of the plugin framework.
        ///</summary>
        ///<param name="logger">normally obtained from the Unity dependency injection container, it's a built-in CAG service</param>
        ///<param name="session">normally obtained from the MEF composition container, it's a Tobi-specific service</param>
        [ImportingConstructor]
        public MetadataValidator(
            ILoggerFacade logger,
            IEventAggregator eventAggregator,
            [Import(typeof(IUrakawaSession), RequiredCreationPolicy = CreationPolicy.Shared, AllowRecomposition = false, AllowDefault = false)]
            IUrakawaSession session)
            : base(eventAggregator)
        {
            m_Logger = logger;
            m_Session = session;

            m_DataTypeValidator = new MetadataDataTypeValidator(this, m_EventAggregator);
            m_OccurrenceValidator = new MetadataOccurrenceValidator(this, m_EventAggregator);
            m_ValidationItemTemplate = new MetadataValidationItemTemplate();

            m_Logger.Log(@"MetadataValidator initialized", Category.Debug, Priority.Medium);
        }

        public void OnUndoRedoManagerChanged(UndoRedoManagerEventArgs eventt, bool done, Command command, bool isTransactionEndEvent, bool isNoTransactionOrTrailingEdge)
        {
            if (!Dispatcher.CurrentDispatcher.CheckAccess())
            {
#if DEBUG
                Debugger.Break();
#endif
                
#if NET40x
                Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Normal,
                    (Action<UndoRedoManagerEventArgs, bool, Command, bool, bool>)OnUndoRedoManagerChanged,
                    eventt, done, command, isTransactionEndEvent, isNoTransactionOrTrailingEdge);
#else
                Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Normal,
                    (Action)(() => OnUndoRedoManagerChanged(eventt, done, command, isTransactionEndEvent, isNoTransactionOrTrailingEdge))
                    );
#endif
                return;
            }

            //if (isTransactionEndEvent)
            //{
            //    return;
            //}

            if (command is CompositeCommand)
            {
#if DEBUG
                Debugger.Break();
#endif
            }
            else if (command is MetadataCommand)
            {
                if (isNoTransactionOrTrailingEdge)
                {
                    Validate();
                }
            }
        }

        private UndoRedoManager.Hooker m_UndoRedoManagerHooker = null;

        protected override void OnProjectLoaded(Project project)
        {
            if (m_Session.IsXukSpine)
            {
                return;
            }

            base.OnProjectLoaded(project);

            m_UndoRedoManagerHooker = project.Presentations.Get(0).UndoRedoManager.Hook(this);

            Validate();
        }

        protected override void OnProjectUnLoaded(Project project)
        {
            base.OnProjectUnLoaded(project);

            if (m_UndoRedoManagerHooker != null) m_UndoRedoManagerHooker.UnHook();
            m_UndoRedoManagerHooker = null;
        }

        public override string Name
        {
            get { return Tobi_Plugin_Validator_Metadata_Lang.MetadataValidator_Name; }
        }

        public override string Description
        {
            get { return Tobi_Plugin_Validator_Metadata_Lang.MetadataValidator_Description; }
        }

        public override bool Validate()
        {
            if (m_Session.IsXukSpine)
            {
                return true;
            }

            bool isValid = true;

            if (m_Session.DocumentProject != null)
            {
                resetToValid();
                isValid = _validate();
            }

            return isValid;
        }

        //TODO: un-hardcode this
        public MetadataDefinitionSet MetadataDefinitions = SupportedMetadata_Z39862005.DefinitionSet;

        private MetadataDataTypeValidator m_DataTypeValidator;
        private MetadataOccurrenceValidator m_OccurrenceValidator;


        private bool _validate() //IEnumerable<urakawa.metadata.Metadata> metadatas)
        {
            bool isValid = true;

            string name = m_Session.DocumentProject.Presentations.Get(0).RootNode.GetXmlElementLocalName();
            bool isHTML = @"body".EndsWith(name, StringComparison.OrdinalIgnoreCase);

#if DEBUG
            bool isXukSpine = @"spine".EndsWith(name, StringComparison.OrdinalIgnoreCase);
            DebugFix.Assert(isXukSpine == m_Session.IsXukSpine);

            if (isHTML)
            {
                //DebugFix.Assert(m_Session.HasXukSpine); too early
            }
#endif

            if (!isHTML && !m_Session.IsXukSpine)
            {
                //validate each item by itself
                foreach (
                    urakawa.metadata.Metadata metadata in
                        m_Session.DocumentProject.Presentations.Get(0).Metadatas.ContentsAs_Enumerable)
                {
                    if (!_validateItem(metadata))
                        isValid = false;
                }
                bool val = _validateAsSet();
                isValid = isValid && val; //metadatas);
            }

            return isValid;
        }


        internal void ReportError(ValidationItem error)
        {
            //prevent duplicate errors: check that the items aren't identical
            //and check that the definitions don't match and the error types don't match
            //theoretically, there should only be one error type per definition (e.g. format, duplicate, etc)

            bool foundDuplicate = false;

            foreach (ValidationItem e in ValidationItems)
            {
                if (e == null) continue;

                bool sameItem = false;
                bool sameDef = sameDef = (e as IMetadataValidationError).Definition == (error as IMetadataValidationError).Definition;
                bool sameType = e.GetType() == error.GetType();

                if (sameType)
                {
                    //does this error's target metadata item already have an error
                    //of this type associated with it?
                    if (e is AbstractMetadataValidationErrorWithTarget &&
                        error is AbstractMetadataValidationErrorWithTarget)
                    {
                        AbstractMetadataValidationErrorWithTarget eWithTarget =
                            e as AbstractMetadataValidationErrorWithTarget;
                        AbstractMetadataValidationErrorWithTarget errorWithTarget =
                            error as AbstractMetadataValidationErrorWithTarget;

                        if (sameType
                            &&
                            eWithTarget.Target == errorWithTarget.Target
                            &&
                            eWithTarget.Target != null)
                        {
                            sameItem = true;
                        }
                    }
                }
                if (error is AbstractMetadataValidationErrorWithTarget && sameItem == true)
                {
                    foundDuplicate = true;
                    break;
                }
                else if (!(error is AbstractMetadataValidationErrorWithTarget) && sameDef && sameType)
                {
                    foundDuplicate = true;
                    break;
                }
            }

            if (!foundDuplicate)
            {
                addValidationItem(error);
            }
        }

        private bool _validateItem(urakawa.metadata.Metadata metadata)
        {
            MetadataDefinition metadataDefinition =
                MetadataDefinitions.GetMetadataDefinition(metadata.NameContentAttribute.Name, true);

            if (metadataDefinition == null)
            {
                metadataDefinition = MetadataDefinitions.UnrecognizedItemFallbackDefinition;
            }

            //check the occurrence requirement
            bool meetsOccurrenceRequirement = m_OccurrenceValidator.Validate(metadata, metadataDefinition);
            //check the data type
            bool meetsDataTypeRequirement = m_DataTypeValidator.Validate(metadata, metadataDefinition);

            return meetsOccurrenceRequirement && meetsDataTypeRequirement;
        }

        private bool _validateAsSet() //IEnumerable<urakawa.metadata.Metadata> metadatas)
        {
            bool isValid = true;
            //make sure all the required items are there
            foreach (MetadataDefinition metadataDefinition in MetadataDefinitions.Definitions)
            {
                if (!metadataDefinition.IsReadOnly
                    && metadataDefinition.Occurrence == MetadataOccurrence.Required)
                {
                    //string name = metadataDefinition.Name.ToLower();

                    bool found = false;
                    foreach (urakawa.metadata.Metadata item in m_Session.DocumentProject.Presentations.Get(0).Metadatas.ContentsAs_Enumerable)
                    {
                        if (item.NameContentAttribute.Name.Equals(metadataDefinition.Name, StringComparison.Ordinal)) //OrdinalIgnoreCase
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        MetadataMissingItemValidationError err =
                            new MetadataMissingItemValidationError(metadataDefinition, m_EventAggregator);
                        ReportError(err);
                        isValid = false;
                    }
                }
            }

            //make sure repetitions are ok

            List<urakawa.metadata.Metadata> list = new List<urakawa.metadata.Metadata>();
            foreach (urakawa.metadata.Metadata metadata in m_Session.DocumentProject.Presentations.Get(0).Metadatas.ContentsAs_Enumerable)
            {
                MetadataDefinition metadataDefinition =
                    MetadataDefinitions.GetMetadataDefinition(metadata.NameContentAttribute.Name);

                if (metadataDefinition != null && !metadataDefinition.IsRepeatable)
                {
                    //string name = metadata.NameContentAttribute.Name.ToLower();
                    list.Clear();
                    foreach (urakawa.metadata.Metadata item in m_Session.DocumentProject.Presentations.Get(0).Metadatas.ContentsAs_Enumerable)
                    {
                        if (item.NameContentAttribute.Name.Equals(metadata.NameContentAttribute.Name, StringComparison.Ordinal)) //OrdinalIgnoreCase
                        {
                            list.Add(item);
                        }
                    }

                    if (list.Count > 1)
                    {
                        isValid = false;
                        foreach (urakawa.metadata.Metadata item in list)
                        {
                            MetadataDuplicateItemValidationError err =
                                new MetadataDuplicateItemValidationError(item, metadataDefinition, m_EventAggregator);
                            ReportError(err);
                        }
                    }
                }
            }
            return isValid;
        }
    }

    public class MetadataDataTypeValidator
    {
        private MetadataValidator m_ParentValidator;
        //These hints describe what the data must be formatted as.
        //Complete sentences purposefully left out.
        private string m_DateHint = Tobi_Plugin_Validator_Metadata_Lang.DateHint;
        private string m_NumericHint = Tobi_Plugin_Validator_Metadata_Lang.NumericValueHint;
        private string m_LanguageHint = Tobi_Plugin_Validator_Metadata_Lang.LanguageValueHint;

        private readonly IEventAggregator m_EventAggregator;

        public MetadataDataTypeValidator(MetadataValidator parentValidator, IEventAggregator eventAggregator)
        {
            m_ParentValidator = parentValidator;
            m_EventAggregator = eventAggregator;
        }
        public bool Validate(urakawa.metadata.Metadata metadata, MetadataDefinition definition)
        {
            switch (definition.DataType)
            {
                case MetadataDataType.ClockValue:
                    return _validateClockValue(metadata, definition);
                case MetadataDataType.Date:
                    return _validateDate(metadata, definition);
                case MetadataDataType.FileUri:
                    return _validateFileUri(metadata, definition);
                case MetadataDataType.Integer:
                    return _validateInteger(metadata, definition);
                case MetadataDataType.Double:
                    return _validateDouble(metadata, definition);
                case MetadataDataType.Number:
                    return _validateNumber(metadata, definition);
                case MetadataDataType.LanguageCode:
                    return _validateLanguageCode(metadata, definition);
                case MetadataDataType.String:
                    return _validateString(metadata, definition);
            }
            return true;
        }

        private bool _validateClockValue(urakawa.metadata.Metadata metadata, MetadataDefinition definition)
        {
            return true;
        }
        private bool _validateDate(urakawa.metadata.Metadata metadata, MetadataDefinition definition)
        {
            MetadataFormatValidationError err =
                new MetadataFormatValidationError(metadata, definition, m_EventAggregator);
            err.Hint = m_DateHint;

            string date = metadata.NameContentAttribute.Value;
            //Require at least the year field
            //The max length of the entire datestring is 10
            if (date.Length < 4 || date.Length > 10)
            {
                m_ParentValidator.ReportError(err);
                return false;
            }

            string[] dateArray = date.Split('-');

            //the year has to be 4 digits
            if (dateArray[0].Length != 4)
            {
                m_ParentValidator.ReportError(err);
                return false;
            }


            //the year has to be digits
            try
            {
                int year = Convert.ToInt32(dateArray[0]);
            }
            catch
            {
                m_ParentValidator.ReportError(err);
                return false;
            }

            //check for a month value (it's optional)
            if (dateArray.Length >= 2)
            {
                //the month has to be numeric
                int month = 0;
                try
                {
                    month = Convert.ToInt32(dateArray[1]);
                }
                catch
                {
                    m_ParentValidator.ReportError(err);
                    return false;
                }
                //the month has to be in this range
                if (month < 1 || month > 12)
                {
                    m_ParentValidator.ReportError(err);
                    return false;
                }
            }
            //check for a day value (it's optional but only if a month is specified)
            if (dateArray.Length == 3)
            {
                //the day has to be a number
                int day = 0;
                try
                {
                    day = Convert.ToInt32(dateArray[2]);
                }
                catch
                {
                    m_ParentValidator.ReportError(err);
                    return false;
                }
                //it has to be in this range
                if (day < 1 || day > 31)
                {
                    m_ParentValidator.ReportError(err);
                    return false;
                }
            }

            return true;
        }
        private bool _validateFileUri(urakawa.metadata.Metadata metadata, MetadataDefinition definition)
        {
            return true;
        }
        private bool _validateInteger(urakawa.metadata.Metadata metadata, MetadataDefinition definition)
        {
            try
            {
                int x = Convert.ToInt32(metadata.NameContentAttribute.Value);
            }
            catch (Exception)
            {
                MetadataFormatValidationError err =
                    new MetadataFormatValidationError(metadata, definition, m_EventAggregator);
                err.Hint = m_NumericHint;

                m_ParentValidator.ReportError(err);
                return false;
            }
            return true;
        }
        private bool _validateDouble(urakawa.metadata.Metadata metadata, MetadataDefinition definition)
        {
            try
            {
                double x = Convert.ToDouble(metadata.NameContentAttribute.Value);
            }
            catch (Exception)
            {
                MetadataFormatValidationError err =
                    new MetadataFormatValidationError(metadata, definition, m_EventAggregator);
                err.Hint = m_NumericHint;

                m_ParentValidator.ReportError(err);
                return false;
            }
            return true;
        }
        //works for both double and int
        private bool _validateNumber(urakawa.metadata.Metadata metadata, MetadataDefinition definition)
        {
            //bool res = _validateInteger(metadata, definition);
            //if (!res) return false;

            bool res = _validateDouble(metadata, definition);
            if (!res) return false;

            return true;
        }

        // Cache is necessary because the exception raised by
        // CultureInfo.GetCultureInfo is very expensive! (computationally speaking)
        private static List<string> m_InvalidLanguageCodes = new List<string>();

        private bool _validateLanguageCode(urakawa.metadata.Metadata metadata, MetadataDefinition definition)
        {
            string lang = metadata.NameContentAttribute.Value;
            bool valid = true;
            if (m_InvalidLanguageCodes.Contains(lang))
            {
                valid = false;
            }
            else
            {
                try
                {
                    CultureInfo info = CultureInfo.GetCultureInfo(lang);
                    if (info != null)
                    {
                        return true;
                    }
                }
                catch
                {
                    valid = false;

                    if (!m_InvalidLanguageCodes.Contains(lang))
                    {
                        m_InvalidLanguageCodes.Add(lang);
                    }
                }
            }
            if (valid)
            {
                return true;
            }
            MetadataFormatValidationError err =
                new MetadataFormatValidationError(metadata, definition, m_EventAggregator);
            err.Hint = m_LanguageHint;

            m_ParentValidator.ReportError(err);

            return false;
        }
        private bool _validateString(urakawa.metadata.Metadata metadata, MetadataDefinition definition)
        {
            return true;
        }
    }

    //checks for non-empty values
    //we could probably move this into one of the other validators since it's pretty simple what it does
    public class MetadataOccurrenceValidator
    {
        private MetadataValidator m_ParentValidator;
        private string m_NonEmptyHint = Tobi_Plugin_Validator_Metadata_Lang.NonEmptyHint;
        private readonly IEventAggregator m_EventAggregator;

        public MetadataOccurrenceValidator(MetadataValidator parentValidator, IEventAggregator eventAggregator)
        {
            m_ParentValidator = parentValidator;
            m_EventAggregator = eventAggregator;
        }

        public bool Validate(urakawa.metadata.Metadata metadata, MetadataDefinition definition)
        {
            //neither required nor optional fields may be empty
            //check both an empty string and our "magic" string value that is
            //used upon creation of a new metadata item
            if (!string.IsNullOrEmpty(metadata.NameContentAttribute.Value) &&
                    metadata.NameContentAttribute.Value != SupportedMetadata_Z39862005.MagicStringEmpty)
            {
                return true;
            }
            else
            {
                MetadataFormatValidationError err =
                    new MetadataFormatValidationError(metadata, definition, m_EventAggregator);
                err.Hint = m_NonEmptyHint;

                m_ParentValidator.ReportError(err);
                return false;
            }
        }
    }
}
