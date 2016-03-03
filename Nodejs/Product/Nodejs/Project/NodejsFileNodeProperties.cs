//*********************************************************//
//    Copyright (c) Microsoft. All rights reserved.
//    
//    Apache 2.0 License
//    
//    You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//    
//    Unless required by applicable law or agreed to in writing, software 
//    distributed under the License is distributed on an "AS IS" BASIS, 
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or 
//    implied. See the License for the specific language governing 
//    permissions and limitations under the License.
//
//*********************************************************//

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

        [SRCategoryAttribute(NodeJsProjectSr.Advanced)]
        [LocDisplayName(NodeJsProjectSr.TestFramework)]
        [SRDescriptionAttribute(NodeJsProjectSr.TestFrameworkDescription)]
        [TypeConverter(typeof(TestFrameworkStringConverter))]
        public string TestFramework {
            get {
                return GetProperty(NodeJsProjectSr.TestFramework, string.Empty);
            }
            set {
                SetProperty(NodeJsProjectSr.TestFramework, value.ToString());
            }
        }
    }

    [ComVisible(true)]
    public class NodejsLinkFileNodeProperties : LinkFileNodeProperties {
        internal NodejsLinkFileNodeProperties(HierarchyNode node)
            : base(node) {
        }

        [SRCategoryAttribute(NodeJsProjectSr.Advanced)]
        [LocDisplayName(NodeJsProjectSr.TestFramework)]
        [SRDescriptionAttribute(NodeJsProjectSr.TestFrameworkDescription)]
        [TypeConverter(typeof(TestFrameworkStringConverter))]
        public string TestFramework {
            get {
                return GetProperty(NodeJsProjectSr.TestFramework, string.Empty);
            }
            set {
                SetProperty(NodeJsProjectSr.TestFramework, value.ToString());
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
