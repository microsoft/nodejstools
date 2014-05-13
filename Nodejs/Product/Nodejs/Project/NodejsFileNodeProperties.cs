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
using Microsoft.NodejsTools.TestFrameworks;

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
        public string TestFramework {
            get {
                return GetProperty(SR.TestFramework, string.Empty);
            }
            set {
                SetProperty(SR.TestFramework, value.ToString());
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
        [TypeConverter(typeof(TestFrameworkStringConverter))]
        public string TestFramework {
            get {
                return GetProperty(SR.TestFramework, string.Empty);
            }
            set {
                SetProperty(SR.TestFramework, value.ToString());
            }
        }
    }

    /// <summary>
    /// This type converter doesn't really do any conversions, but allows us to provide
    /// a list of standard values for the test framework.
    /// </summary>
    class TestFrameworkStringConverter : StringConverter {
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
            TestFrameworkDirectories discover = new TestFrameworkDirectories();
            List<string> knownFrameworkList = discover.GetFrameworkNames();
            return new StandardValuesCollection(knownFrameworkList);
        }
    }
}
