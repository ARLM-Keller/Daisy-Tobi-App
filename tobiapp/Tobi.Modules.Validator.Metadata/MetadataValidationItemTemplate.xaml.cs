﻿using System.ComponentModel.Composition;
using System.Windows;


namespace Tobi.Plugin.Validator.Metadata
{
    [Export(typeof(ResourceDictionary)), PartCreationPolicy(CreationPolicy.Shared)]
    public partial class MetadataValidationItemTemplate : ResourceDictionary
    {
        public MetadataValidationItemTemplate()
        {
            InitializeComponent();
        }
    }
}
