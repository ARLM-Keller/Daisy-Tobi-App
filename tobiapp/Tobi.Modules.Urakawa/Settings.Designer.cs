﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.17626
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Tobi.Plugin.Urakawa {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "11.0.0.0")]
    public sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool ExportIncludeImageDescriptions {
            get {
                return ((bool)(this["ExportIncludeImageDescriptions"]));
            }
            set {
                this["ExportIncludeImageDescriptions"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool AudioCodecDisableACM {
            get {
                return ((bool)(this["AudioCodecDisableACM"]));
            }
            set {
                this["AudioCodecDisableACM"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool AudioExportEncodeToMp3 {
            get {
                return ((bool)(this["AudioExportEncodeToMp3"]));
            }
            set {
                this["AudioExportEncodeToMp3"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool EnableRecentFilesMenu {
            get {
                return ((bool)(this["EnableRecentFilesMenu"]));
            }
            set {
                this["EnableRecentFilesMenu"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Hz44100")]
        public global::AudioLib.SampleRate AudioExportSampleRate {
            get {
                return ((global::AudioLib.SampleRate)(this["AudioExportSampleRate"]));
            }
            set {
                this["AudioExportSampleRate"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Hz44100")]
        public global::AudioLib.SampleRate AudioProjectSampleRate {
            get {
                return ((global::AudioLib.SampleRate)(this["AudioProjectSampleRate"]));
            }
            set {
                this["AudioProjectSampleRate"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool AudioProjectStereo {
            get {
                return ((bool)(this["AudioProjectStereo"]));
            }
            set {
                this["AudioProjectStereo"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool XUK_PrettyFormat {
            get {
                return ((bool)(this["XUK_PrettyFormat"]));
            }
            set {
                this["XUK_PrettyFormat"] = value;
            }
        }
    }
}
