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
using System.IO;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudioTools {
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    internal class ProvideDebugPortSupplierAttribute : RegistrationAttribute {
        private readonly string _id, _name;
        private readonly Type _portSupplier;

        public ProvideDebugPortSupplierAttribute(string name, Type portSupplier, string id) {
            _name = name;
            _portSupplier = portSupplier;
            _id = id;
        }

        public override void Register(RegistrationContext context) {
            var engineKey = context.CreateKey("AD7Metrics\\PortSupplier\\" + _id);
            engineKey.SetValue("Name", _name);
            engineKey.SetValue("CLSID", _portSupplier.GUID.ToString("B"));

            var clsidKey = context.CreateKey("CLSID");
            var clsidGuidKey = clsidKey.CreateSubkey(_portSupplier.GUID.ToString("B"));
            clsidGuidKey.SetValue("Assembly", _portSupplier.Assembly.FullName);
            clsidGuidKey.SetValue("Class", _portSupplier.FullName);
            clsidGuidKey.SetValue("InprocServer32", context.InprocServerPath);
            clsidGuidKey.SetValue("CodeBase", Path.Combine(context.ComponentPath, _portSupplier.Module.Name));
            clsidGuidKey.SetValue("ThreadingModel", "Free");
        }

        public override void Unregister(RegistrationContext context) {
        }
    }
}
