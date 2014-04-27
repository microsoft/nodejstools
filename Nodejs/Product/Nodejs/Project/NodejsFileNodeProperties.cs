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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudioTools.Project;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using VSLangProj;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace Microsoft.NodejsTools.Project {

    [ComVisible(true)]
    public class NodejsIncludedFileNodeProperties : IncludedFileNodeProperties {
        internal NodejsIncludedFileNodeProperties(HierarchyNode node)
            : base(node) {
        }

        [SRCategoryAttribute(SR.Advanced)]
        [LocDisplayName(SR.TestFramework)]
        [SRDescriptionAttribute(SR.TestFrameworkDescription)]
        [TypeConverter(typeof(TestFrameworkStringConverter))]
        public string ItemTestFramework {
            get {
                var framework = this.HierarchyNode.ItemNode.GetMetadata(SR.TestFramework);
                if (String.IsNullOrWhiteSpace(framework)) {
                    return String.Empty;
                }
                return Convert.ToString(framework);
            }
            set {
                this.HierarchyNode.ItemNode.SetMetadata(SR.TestFramework, value.ToString());
            }
        }

        /// <summary>
        /// Specifies the build action as a TestFrameworkType so that automation can get the
        /// expected enum value.
        /// </summary>
        [Browsable(false)]
        public TestFrameworkType TestFramework {
            get {
                var res = TestFrameworkStringConverter.Instance.ConvertFromString(HierarchyNode.ItemNode.ItemTypeName);
                if (res is TestFrameworkType) {
                    return (TestFrameworkType)res;
                }
                return TestFrameworkType.Default;
            }
            set {
                this.HierarchyNode.ItemNode.ItemTypeName = TestFrameworkTypeConverter.Instance.ConvertToString(value);
            }
        }
    }

    [ComVisible(true)]
    public class NodejsLinkFileNodeProperties : LinkFileNodeProperties {
        internal NodejsLinkFileNodeProperties(HierarchyNode node)
            : base(node) {
        }

        [SRCategoryAttribute(SR.Advanced)]
        [LocDisplayName(SR.TestFramework)]
        [SRDescriptionAttribute(SR.TestFrameworkDescription)]
        public string TestFramework {
            get {
                var framework = this.HierarchyNode.ItemNode.GetMetadata(SR.TestFramework);
                if (String.IsNullOrEmpty(framework)) {
                    return String.Empty;
                }
                return Convert.ToString(framework);
            }
            set {

                this.HierarchyNode.ItemNode.SetMetadata(SR.TestFramework, value.ToString());
            }
        }
    }

    class TestFrameworkTypeConverter : StringConverter {
        internal static readonly TestFrameworkTypeConverter Instance = new TestFrameworkTypeConverter();

        public TestFrameworkTypeConverter() {
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) {
            return true;
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
            if (sourceType == typeof(string)) {
                return true;
            }
            return base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
            if (destinationType == typeof(string)) {
                switch ((TestFrameworkType)value) {
                    case TestFrameworkType.Default:
                        return "Default";
                    case TestFrameworkType.Mocha:
                        return "Mocha";
                }
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
            if (value is string) {
                string strVal = (string)value;
                if (strVal.Equals("Default", StringComparison.OrdinalIgnoreCase)) {
                    return TestFrameworkType.Default;
                } else if (strVal.Equals("Mocha", StringComparison.OrdinalIgnoreCase)) {
                    return TestFrameworkType.Mocha;
                }
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context) {
            return new StandardValuesCollection(new[] { TestFrameworkType.Default, TestFrameworkType.Mocha });
        }
    }
    /// <summary>
    /// This type converter doesn't really do any conversions, but allows us to provide
    /// a list of standard values for the test framework.
    /// </summary>
    class TestFrameworkStringConverter : StringConverter {
        internal static readonly TestFrameworkStringConverter Instance = new TestFrameworkStringConverter();

        public TestFrameworkStringConverter() {
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) {
            return true;
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
            if (sourceType == typeof(string)) {
                return true;
            }
            return base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
            return value;
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
            return value;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context) {
            FileNodeProperties nodeProps = context.Instance as FileNodeProperties;
            IEnumerable<string> itemNames = new[] { "Default" /*TODO: share resource string*/, "Mocha" };
            return new StandardValuesCollection(itemNames.ToArray());
        }
    }
}
