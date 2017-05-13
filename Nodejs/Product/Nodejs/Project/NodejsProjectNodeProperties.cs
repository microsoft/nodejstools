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
using System.Runtime.InteropServices;
using Microsoft.VisualStudioTools;
using Microsoft.VisualStudioTools.Project;
using Microsoft.VisualStudioTools.Project.Automation;

namespace Microsoft.NodejsTools.Project {
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Guid("04726C27-8125-471A-BAC0-2301D273DB5E")]
    public class NodejsProjectNodeProperties : CommonProjectNodeProperties, EnvDTE80.IInternalExtenderProvider {
        internal NodejsProjectNodeProperties(ProjectNode node)
            : base(node) {
        }

        [PropertyNameAttribute("WebApplication.AspNetDebugging")]
        [Browsable(false)]
        public bool AspNetDebugging {
            get {
                return true;
            }
        }

        [PropertyNameAttribute("WebApplication.NativeDebugging")]
        [Browsable(false)]
        public bool NativeDebugging {
            get {
                return false;
            }
        }

        [Browsable(false)]
        public uint TargetFramework {
            get {
                return 0x40005;
            }
        }

        [SRCategory(SR.General)]
        [ResourcesDisplayName(nameof(Resources.NodeExePath))]
        [ResourcesDescription(nameof(Resources.NodeExePathDescription))]
        public string NodeExePath {
            get {
                return HierarchyNode.ProjectMgr.Site.GetUIThread().Invoke(() => {
                    return Nodejs.GetAbsoluteNodeExePath(
                        ProjectHome,
                        Node.ProjectMgr.GetProjectProperty(NodeProjectProperty.NodeExePath, true));
                });
            }
            set {
                HierarchyNode.ProjectMgr.Site.GetUIThread().Invoke(() => {
                    Node.ProjectMgr.SetProjectProperty(NodeProjectProperty.NodeExePath, value);
                });
            }
        }

        [SRCategory(SR.General)]
        [ResourcesDisplayName(nameof(Resources.NodeExeArguments))]
        [ResourcesDescription(nameof(Resources.NodeExeArgumentsDescription))]
        public string NodeExeArguments {
            get {
                return HierarchyNode.ProjectMgr.Site.GetUIThread().Invoke(() => {
                    return this.Node.ProjectMgr.GetProjectProperty(NodeProjectProperty.NodeExeArguments, true);
                });
            }
            set {
                HierarchyNode.ProjectMgr.Site.GetUIThread().Invoke(() => {
                    Node.ProjectMgr.SetProjectProperty(NodeProjectProperty.NodeExeArguments, value);
                });
            }
        }

        [SRCategory(SR.General)]
        [SRDisplayName(SR.ScriptArguments)]
        [SRDescription(SR.ScriptArgumentsDescription)]
        public string ScriptArguments {
            get {
                return HierarchyNode.ProjectMgr.Site.GetUIThread().Invoke(() => {
                    return this.Node.ProjectMgr.GetProjectProperty(NodeProjectProperty.ScriptArguments, true);
                });
            }
            set {
                HierarchyNode.ProjectMgr.Site.GetUIThread().Invoke(() => {
                    Node.ProjectMgr.SetProjectProperty(NodeProjectProperty.ScriptArguments, value);
                });
            }
        }

        [SRCategory(SR.General)]
        [ResourcesDisplayName(nameof(Resources.NodejsPort))]
        [ResourcesDescription(nameof(Resources.NodejsPortDescription))]
        public int? NodejsPort {
            get {
                return HierarchyNode.ProjectMgr.Site.GetUIThread().Invoke((Func<int?>)(() => {
                    int port;
                    if (Int32.TryParse(Node.ProjectMgr.GetProjectProperty(NodeProjectProperty.NodejsPort, true), out port)) {
                        return port;
                    }
                    return null;
                }));
            }
            set {
                HierarchyNode.ProjectMgr.Site.GetUIThread().Invoke(() => {
                    Node.ProjectMgr.SetProjectProperty(NodeProjectProperty.NodejsPort, value != null ? value.ToString() : String.Empty);
                });
            }
        }

        [SRCategory(SR.General)]
        [SRDisplayName(SR.LaunchUrl)]
        [SRDescription(SR.LaunchUrlDescription)]
        public string LaunchUrl {
            get {
                return HierarchyNode.ProjectMgr.Site.GetUIThread().Invoke(() => {
                    return this.Node.ProjectMgr.GetProjectProperty(NodeProjectProperty.LaunchUrl, true);
                });
            }
            set {
                HierarchyNode.ProjectMgr.Site.GetUIThread().Invoke(() => {
                    Node.ProjectMgr.SetProjectProperty(NodeProjectProperty.LaunchUrl, value);
                });
            }
        }

        [SRCategory(SR.General)]
        [SRDisplayName(SR.StartWebBrowser)]
        [SRDescription(SR.StartWebBrowserDescription)]
        public bool StartWebBrowser {
            get {
                return HierarchyNode.ProjectMgr.Site.GetUIThread().Invoke(() => {
                    bool res;
                    if (Boolean.TryParse(Node.ProjectMgr.GetProjectProperty(NodeProjectProperty.StartWebBrowser, true), out res)) {
                        return res;
                    }
                    return true;
                });
            }
            set {
                HierarchyNode.ProjectMgr.Site.GetUIThread().Invoke(() => {
                    Node.ProjectMgr.SetProjectProperty(NodeProjectProperty.StartWebBrowser, value.ToString());
                });
            }
        }

        object EnvDTE80.IInternalExtenderProvider.GetExtender(string extenderCATID, string extenderName, object extendeeObject, EnvDTE.IExtenderSite extenderSite, int cookie) {
            EnvDTE80.IInternalExtenderProvider outerHierarchy = Node.GetOuterInterface<EnvDTE80.IInternalExtenderProvider>();

            if (outerHierarchy != null) {
                var res = outerHierarchy.GetExtender(extenderCATID, extenderName, extendeeObject, extenderSite, cookie);
                if (extenderName == "WebApplication" && res is ICustomTypeDescriptor) {
                    // we want to filter out the launch debug server option
                    return new WebAppExtenderFilter((ICustomTypeDescriptor)res);
                }
                return res;
            }
            return null;
        }
    }

    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDual)] // This line triggers an FXCop warning, but is ok
    public class WebAppExtenderFilter : ICustomTypeDescriptor {
        private readonly ICustomTypeDescriptor _innerObject;

        public WebAppExtenderFilter(ICustomTypeDescriptor innerObject) {
            _innerObject = innerObject;
        }

        internal object InnerObject {
            get {
                return _innerObject;
            }
        }

        #region ICustomTypeDescriptor Members

        public AttributeCollection GetAttributes() {
            return _innerObject.GetAttributes();
        }

        public string GetClassName() {
            return _innerObject.GetClassName();
        }

        public string GetComponentName() {
            return _innerObject.GetComponentName();
        }

        public TypeConverter GetConverter() {
            return _innerObject.GetConverter();
        }

        public EventDescriptor GetDefaultEvent() {
            return _innerObject.GetDefaultEvent();
        }

        public PropertyDescriptor GetDefaultProperty() {
            return _innerObject.GetDefaultProperty();
        }

        public object GetEditor(Type editorBaseType) {
            return _innerObject.GetEditor(editorBaseType);
        }

        public EventDescriptorCollection GetEvents(Attribute[] attributes) {
            return _innerObject.GetEvents(attributes);
        }

        public EventDescriptorCollection GetEvents() {
            return _innerObject.GetEvents();
        }

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes) {
            List<PropertyDescriptor> res = new List<PropertyDescriptor>();
            var original = _innerObject.GetProperties(attributes);
            foreach (PropertyDescriptor item in original) {
                if (!IsFiltered(item)) {
                    res.Add(item);
                }
            }

            return new PropertyDescriptorCollection(res.ToArray());
        }

        private static bool IsFiltered(PropertyDescriptor item) {
            return item.Name == "StartWebServerOnDebug";
        }

        public PropertyDescriptorCollection GetProperties() {
            List<PropertyDescriptor> res = new List<PropertyDescriptor>();
            var original = _innerObject.GetProperties();
            foreach (PropertyDescriptor item in original) {
                if (!IsFiltered(item)) {
                    res.Add(item);
                }
            }

            return new PropertyDescriptorCollection(res.ToArray());
        }

        public object GetPropertyOwner(PropertyDescriptor pd) {
            return _innerObject.GetPropertyOwner(pd);
        }

        #endregion
    }
}
