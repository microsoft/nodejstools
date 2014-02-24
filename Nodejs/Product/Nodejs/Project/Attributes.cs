/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using CommonSR = Microsoft.VisualStudioTools.Project.SR;

namespace Microsoft.NodejsTools.Project {
    internal class SR : CommonSR {
        internal const string NodeExeArguments = "NodeExeArguments";
        internal const string NodeExeArgumentsDescription = "NodeExeArgumentsDescription";
        internal const string NodeExePath = "NodeExePath";
        internal const string NodeExePathDescription = "NodeExePathDescription";
        internal const string NodejsPort = "NodejsPort";
        internal const string NodejsPortDescription = "NodejsPortDescription";

        internal const string NpmPackageName = "NpmPackageName";
        internal const string NpmPackageNameDescription = "NpmPackageNameDescription";
        internal const string NpmPackageVersion = "NpmPackageVersion";
        internal const string NpmPackageVersionDescription = "NpmPackageVersionDescription";
        internal const string NpmPackageRequestedVersionRange = "NpmPackageRequestedVersionRange";
        internal const string NpmPackageRequestedVersionRangeDescription = "NpmPackageRequestedVersionRangeDescription";
        internal const string NpmPackageNewVersionAvailable = "NpmPackageNewVersionAvailable";
        internal const string NpmPackageNewVersionAvailableDescription = "NpmPackageNewVersionAvailableDescription";
        internal const string NpmPackageDescription = "NpmPackageDescription";
        internal const string NpmPackageDescriptionDescription = "NpmPackageDescriptionDescription";
        internal const string NpmPackageKeywords = "NpmPackageKeywords";
        internal const string NpmPackageKeywordsDescription = "NpmPackageKeywordsDescription";
        internal const string NpmPackageAuthor = "NpmPackageAuthor";
        internal const string NpmPackageAuthorDescription = "NpmPackageAuthorDescription";
        internal const string NpmPackagePublishDateTime = "NpmPackagePublishDateTime";
        internal const string NpmPackagePublishDateTimeDescription = "NpmPackagePublishDateTimeDescription";
        internal const string NpmPackagePath = "NpmPackagePath";
        internal const string NpmPackagePathDescription = "NpmPackagePathDescription";
        internal const string NpmPackageType = "NpmPackageType";
        internal const string NpmPackageTypeDescription = "NpmPackageTypeDescription";
        internal const string NpmPackageLinkStatus = "NpmPackageLinkStatus";
        internal const string NpmPackageLinkStatusDescription = "NpmPackageLinkStatusDescription";
        internal const string NpmPackageIsListedInParentPackageJson = "NpmPackageIsListedInParentPackageJson";
        internal const string NpmPackageIsListedInParentPackageJsonDescription = "NpmPackageIsListedInParentPackageJsonDescription";
        internal const string NpmPackageIsMissing = "NpmPackageIsMissing";
        internal const string NpmPackageIsMissingDescription = "NpmPackageIsMissingDescription";
        internal const string NpmPackageIsDevDependency = "NpmPackageIsDevDependency";
        internal const string NpmPackageIsDevDependencyDescription = "NpmPackageIsDevDependencyDescription";
        internal const string NpmPackageIsOptionalDependency = "NpmPackageIsOptionalDependency";
        internal const string NpmPackageIsOptionalDependencyDescription = "NpmPackageIsOptionalDependencyDescription";
        internal const string NpmPackageIsBundledDependency = "NpmPackageIsBundledDependency";
        internal const string NpmPackageIsBundledDependencyDescription = "NpmPackageIsBundledDependencyDescription";

        internal const string CategoryVersion = "CategoryVersion";
        internal const string CategoryStatus = "CategoryStatus";

        internal const string NpmNodePackageInstallation = "NpmNodePackageInstallation";
        internal const string NpmNodePackageInstallationDescription = "NpmNodePackageInstallationDescription";
        internal const string NpmNodePath = "NpmNodePath";
        internal const string NpmNodePathDescription = "NpmNodePathDescription";

        internal static new string GetString(string value) {
            string result = Microsoft.NodejsTools.Resources.ResourceManager.GetString(value, CultureInfo.CurrentUICulture) ?? CommonSR.GetString(value);
            if (result == null) {
                Debug.Assert(false, "String resource '" + value + "' is missing");
                result = value;
            }
            return result;
        }

        internal static new string GetString(string value, params object[] args) {
            string result = Microsoft.NodejsTools.Resources.ResourceManager.GetString(value, CultureInfo.CurrentUICulture) ?? CommonSR.GetString(value);
            if (result == null) {
                Debug.Assert(false, "String resource '" + value + "' is missing");
                result = value;
            }
            return string.Format(CultureInfo.CurrentUICulture, result, args);
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    internal sealed class LocDisplayNameAttribute : DisplayNameAttribute {
        readonly string value;

        public LocDisplayNameAttribute(string name) {
            value = name;
        }

        public override string DisplayName {
            get {
                return SR.GetString(value);
            }
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    internal sealed class SRCategoryAttribute : CategoryAttribute {
        public SRCategoryAttribute(string name) : base(name) { }

        protected override string GetLocalizedString(string value) {
            return SR.GetString(value);
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    internal sealed class SRDescriptionAttribute : DescriptionAttribute {
        readonly string value;

        public SRDescriptionAttribute(string name) {
            value = name;
        }

        public override string Description {
            get {
                return SR.GetString(value);
            }
        }
    }
}
