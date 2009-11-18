﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.21006.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Microsoft.Practices.Unity.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Microsoft.Practices.Unity.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The type {0} has multiple constructors of length {1}. Unable to disambiguate..
        /// </summary>
        internal static string AmbiguousInjectionConstructor {
            get {
                return ResourceManager.GetString("AmbiguousInjectionConstructor", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The provided string argument must not be empty..
        /// </summary>
        internal static string ArgumentMustNotBeEmpty {
            get {
                return ResourceManager.GetString("ArgumentMustNotBeEmpty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The current build operation (build key {2}) failed: {3} (Strategy type {0}, index {1}).
        /// </summary>
        internal static string BuildFailedException {
            get {
                return ResourceManager.GetString("BuildFailedException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The current type, {0}, is an interface and cannot be constructed. Are you missing a type mapping?.
        /// </summary>
        internal static string CannotConstructInterface {
            get {
                return ResourceManager.GetString("CannotConstructInterface", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot extract type from build key {0}..
        /// </summary>
        internal static string CannotExtractTypeFromBuildKey {
            get {
                return ResourceManager.GetString("CannotExtractTypeFromBuildKey", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The method {0}.{1}({2}) is an open generic method. Open generic methods cannot be injected..
        /// </summary>
        internal static string CannotInjectGenericMethod {
            get {
                return ResourceManager.GetString("CannotInjectGenericMethod", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The property {0} on type {1} is an indexer. Indexed properties cannot be injected..
        /// </summary>
        internal static string CannotInjectIndexer {
            get {
                return ResourceManager.GetString("CannotInjectIndexer", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The method {1} on type {0} has an out parameter. Injection cannot be performed..
        /// </summary>
        internal static string CannotInjectMethodWithOutParam {
            get {
                return ResourceManager.GetString("CannotInjectMethodWithOutParam", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The method {0}.{1}({2}) has at least one out parameter. Methods with out parameters cannot be injected..
        /// </summary>
        internal static string CannotInjectMethodWithOutParams {
            get {
                return ResourceManager.GetString("CannotInjectMethodWithOutParams", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The method {0}.{1}({2}) has at least one ref parameter.Methods with ref parameters cannot be injected..
        /// </summary>
        internal static string CannotInjectMethodWithRefParams {
            get {
                return ResourceManager.GetString("CannotInjectMethodWithRefParams", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The method {1} on type {0} is marked for injection, but it is an open generic method. Injection cannot be performed..
        /// </summary>
        internal static string CannotInjectOpenGenericMethod {
            get {
                return ResourceManager.GetString("CannotInjectOpenGenericMethod", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The method {0}.{1}({2}) is static. Static methods cannot be injected..
        /// </summary>
        internal static string CannotInjectStaticMethod {
            get {
                return ResourceManager.GetString("CannotInjectStaticMethod", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Resolving parameter &quot;{0}&quot; of constructor {1}.
        /// </summary>
        internal static string ConstructorArgumentResolveOperation {
            get {
                return ResourceManager.GetString("ConstructorArgumentResolveOperation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The parameter {0} could not be resolved when attempting to call constructor {1}..
        /// </summary>
        internal static string ConstructorParameterResolutionFailed {
            get {
                return ResourceManager.GetString("ConstructorParameterResolutionFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Calling constructor {0}.
        /// </summary>
        internal static string InvokingConstructorOperation {
            get {
                return ResourceManager.GetString("InvokingConstructorOperation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Calling method {0}.{1}.
        /// </summary>
        internal static string InvokingMethodOperation {
            get {
                return ResourceManager.GetString("InvokingMethodOperation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An item with the given key is already present in the dictionary..
        /// </summary>
        internal static string KeyAlreadyPresent {
            get {
                return ResourceManager.GetString("KeyAlreadyPresent", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The lifetime manager is already registered. Lifetime managers cannot be reused, please create a new one..
        /// </summary>
        internal static string LifetimeManagerInUse {
            get {
                return ResourceManager.GetString("LifetimeManagerInUse", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Resolving parameter &quot;{0}&quot; of method {1}.{2}.
        /// </summary>
        internal static string MethodArgumentResolveOperation {
            get {
                return ResourceManager.GetString("MethodArgumentResolveOperation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The value for parameter &quot;{1}&quot; of method {0} could not be resolved. .
        /// </summary>
        internal static string MethodParameterResolutionFailed {
            get {
                return ResourceManager.GetString("MethodParameterResolutionFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Could not resolve dependency for build key {0}..
        /// </summary>
        internal static string MissingDependency {
            get {
                return ResourceManager.GetString("MissingDependency", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The type {0} has multiple constructors marked with the InjectionConstructor attribute. Unable to disambiguate..
        /// </summary>
        internal static string MultipleInjectionConstructors {
            get {
                return ResourceManager.GetString("MultipleInjectionConstructors", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The supplied type {0} must be an open generic type..
        /// </summary>
        internal static string MustHaveOpenGenericType {
            get {
                return ResourceManager.GetString("MustHaveOpenGenericType", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The supplied type {0} does not have the same number of generic arguments as the target type {1}..
        /// </summary>
        internal static string MustHaveSameNumberOfGenericArguments {
            get {
                return ResourceManager.GetString("MustHaveSameNumberOfGenericArguments", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The type {0} does not have an accessible constructor..
        /// </summary>
        internal static string NoConstructorFound {
            get {
                return ResourceManager.GetString("NoConstructorFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The type {0} does not have a generic argument named &quot;{1}&quot;.
        /// </summary>
        internal static string NoMatchingGenericArgument {
            get {
                return ResourceManager.GetString("NoMatchingGenericArgument", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to while resolving.
        /// </summary>
        internal static string NoOperationExceptionReason {
            get {
                return ResourceManager.GetString("NoOperationExceptionReason", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The type {0} does not have a constructor that takes the parameters ({1})..
        /// </summary>
        internal static string NoSuchConstructor {
            get {
                return ResourceManager.GetString("NoSuchConstructor", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The type {0} does not have a public method named {1} that takes the parameters ({2})..
        /// </summary>
        internal static string NoSuchMethod {
            get {
                return ResourceManager.GetString("NoSuchMethod", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The type {0} does not contain a property named {1}..
        /// </summary>
        internal static string NoSuchProperty {
            get {
                return ResourceManager.GetString("NoSuchProperty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The type {0} is not a generic type, and you are attempting to inject a generic parameter named &quot;{1}&quot;..
        /// </summary>
        internal static string NotAGenericType {
            get {
                return ResourceManager.GetString("NotAGenericType", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The type {0} is not an array type with rank 1, and you are attempting to use a [DependencyArray] attribute on a parameter or property with this type..
        /// </summary>
        internal static string NotAnArrayTypeWithRankOne {
            get {
                return ResourceManager.GetString("NotAnArrayTypeWithRankOne", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The property {0} on type {1} is not settable..
        /// </summary>
        internal static string PropertyNotSettable {
            get {
                return ResourceManager.GetString("PropertyNotSettable", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The property {0} on type {1} is of type {2}, and cannot be injected with a value of type {3}..
        /// </summary>
        internal static string PropertyTypeMismatch {
            get {
                return ResourceManager.GetString("PropertyTypeMismatch", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The value for the property &quot;{0}&quot; could not be resolved..
        /// </summary>
        internal static string PropertyValueResolutionFailed {
            get {
                return ResourceManager.GetString("PropertyValueResolutionFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The provided string argument must not be empty..
        /// </summary>
        internal static string ProvidedStringArgMustNotBeEmpty {
            get {
                return ResourceManager.GetString("ProvidedStringArgMustNotBeEmpty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Resolution of the dependency failed, type = &quot;{0}&quot;, name = &quot;{1}&quot;.
        ///Exception occurred while: {2}.
        ///Exception is: {3} - {4}
        ///-----------------------------------------------
        ///At the time of the exception, the container was:
        ///.
        /// </summary>
        internal static string ResolutionFailed {
            get {
                return ResourceManager.GetString("ResolutionFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Resolving {0},{1}.
        /// </summary>
        internal static string ResolutionTraceDetail {
            get {
                return ResourceManager.GetString("ResolutionTraceDetail", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Resolving {0},{1} (mapped from {2}, {3}).
        /// </summary>
        internal static string ResolutionWithMappingTraceDetail {
            get {
                return ResourceManager.GetString("ResolutionWithMappingTraceDetail", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Resolving value for property {0}.{1}.
        /// </summary>
        internal static string ResolvingPropertyValueOperation {
            get {
                return ResourceManager.GetString("ResolvingPropertyValueOperation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Setting value for property {0}.{1}.
        /// </summary>
        internal static string SettingPropertyOperation {
            get {
                return ResourceManager.GetString("SettingPropertyOperation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The type {1} cannot be assigned to variables of type {0}..
        /// </summary>
        internal static string TypesAreNotAssignable {
            get {
                return ResourceManager.GetString("TypesAreNotAssignable", resourceCulture);
            }
        }
    }
}
