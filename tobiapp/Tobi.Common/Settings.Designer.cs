﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.17929
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Tobi.Common {
    
    
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
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool WindowShellRightToLeft {
            get {
                return ((bool)(this["WindowShellRightToLeft"]));
            }
            set {
                this["WindowShellRightToLeft"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool EnableMediaKit {
            get {
                return ((bool)(this["EnableMediaKit"]));
            }
            set {
                this["EnableMediaKit"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("#FF99FF66")]
        public global::System.Windows.Media.Color SearchHits_Color {
            get {
                return ((global::System.Windows.Media.Color)(this["SearchHits_Color"]));
            }
            set {
                this["SearchHits_Color"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool EnableAudioCues {
            get {
                return ((bool)(this["EnableAudioCues"]));
            }
            set {
                this["EnableAudioCues"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool WpfSoftwareRender {
            get {
                return ((bool)(this["WpfSoftwareRender"]));
            }
            set {
                this["WpfSoftwareRender"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("video,audio,math,svg,img,caption,doctitle,docauthor,pagenum,hd,h1,h2,h3,h4,h5,h6," +
            "p,lic,li,dd,dt,quote,td,th,a,sent,span,")]
        public string TextAudioSyncGranularity {
            get {
                return ((string)(this["TextAudioSyncGranularity"]));
            }
            set {
                this["TextAudioSyncGranularity"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool EnableTextSyncGranularity {
            get {
                return ((bool)(this["EnableTextSyncGranularity"]));
            }
            set {
                this["EnableTextSyncGranularity"] = value;
            }
        }
    }
}
