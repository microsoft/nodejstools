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

        [SRCategoryAttribute(SR.General)]
        [SRDisplayName(SR.NodeExePath)]
        [SRDescriptionAttribute(SR.NodeExePathDescription)]
        public string NodeExePath {
            get {
                return UIThread.Invoke(() => {
                    var res = this.Node.ProjectMgr.GetProjectProperty(NodejsConstants.NodeExePath, true);
                    if (String.IsNullOrWhiteSpace(res)) {
                        return Nodejs.NodeExePath ?? String.Empty;
                    }
                    return res;
                });
            }
            set {
                UIThread.Invoke(() => {
                    Node.ProjectMgr.SetProjectProperty(NodejsConstants.NodeExePath, value);
                });
            }
        }

        [SRCategoryAttribute(SR.General)]
        [SRDisplayName(SR.NodeExeArguments)]
        [SRDescriptionAttribute(SR.NodeExeArgumentsDescription)]
        public string NodeExeArguments {
            get {
                return UIThread.Invoke(() => {
                    return this.Node.ProjectMgr.GetProjectProperty(NodejsConstants.NodeExeArguments, true);
                });
            }
            set {
                UIThread.Invoke(() => {
                    Node.ProjectMgr.SetProjectProperty(NodejsConstants.NodeExeArguments, value);
                });
            }
        }

        [SRCategoryAttribute(SR.General)]
        [SRDisplayName(SR.ScriptArguments)]
        [SRDescriptionAttribute(SR.ScriptArgumentsDescription)]
        public string ScriptArguments {
            get {
                return UIThread.Invoke(() => {
                    return this.Node.ProjectMgr.GetProjectProperty(NodejsConstants.ScriptArguments, true);
                });
            }
            set {
                UIThread.Invoke(() => {
                    Node.ProjectMgr.SetProjectProperty(NodejsConstants.ScriptArguments, value);
                });
            }
        }

        [SRCategoryAttribute(SR.General)]
        [SRDisplayName(SR.NodejsPort)]
        [SRDescriptionAttribute(SR.NodejsPortDescription)]
        public int? NodejsPort {
            get {
                return UIThread.Invoke((Func<int?>)(() => {
                    int port;
                    if (Int32.TryParse(Node.ProjectMgr.GetProjectProperty(NodejsConstants.NodejsPort, true), out port)) {
                        return port;
                    }
                    return null;
                }));
            }
            set {
                UIThread.Invoke(() => {
                    Node.ProjectMgr.SetProjectProperty(NodejsConstants.NodejsPort, value != null ? value.ToString() : String.Empty);
                });
            }
        }

        [SRCategoryAttribute(SR.General)]
        [SRDisplayName(SR.LaunchUrl)]
        [SRDescriptionAttribute(SR.LaunchUrlDescription)]
        public string LaunchUrl {
            get {
                return UIThread.Invoke(() => {
                    return this.Node.ProjectMgr.GetProjectProperty(NodejsConstants.LaunchUrl, true);
                });
            }
            set {
                UIThread.Invoke(() => {
                    Node.ProjectMgr.SetProjectProperty(NodejsConstants.LaunchUrl, value);
                });
            }
        }

        [SRCategoryAttribute(SR.General)]
        [SRDisplayName(SR.StartWebBrowser)]
        [SRDescriptionAttribute(SR.StartWebBrowserDescription)]
        public bool StartWebBrowser {
            get {
                return UIThread.Invoke(() => {
                    bool res;
                    if (Boolean.TryParse(Node.ProjectMgr.GetProjectProperty(NodejsConstants.StartWebBrowser, true), out res)) {
                        return res;
                    }
                    return true;
                });
            }
            set {
                UIThread.Invoke(() => {
                    Node.ProjectMgr.SetProjectProperty(NodejsConstants.StartWebBrowser, value.ToString());
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
