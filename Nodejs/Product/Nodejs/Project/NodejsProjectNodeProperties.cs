// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.VisualStudioTools;
using Microsoft.VisualStudioTools.Project;
using Microsoft.VisualStudioTools.Project.Automation;

namespace Microsoft.NodejsTools.Project
{
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Guid("04726C27-8125-471A-BAC0-2301D273DB5E")]
    public class NodejsProjectNodeProperties : CommonProjectNodeProperties, EnvDTE80.IInternalExtenderProvider
    {
        internal NodejsProjectNodeProperties(ProjectNode node)
            : base(node)
        {
        }

        [PropertyNameAttribute("WebApplication.AspNetDebugging")]
        [Browsable(false)]
        public bool AspNetDebugging => true;

        [PropertyNameAttribute("WebApplication.NativeDebugging")]
        [Browsable(false)]
        public bool NativeDebugging => false;

        [Browsable(false)]
        public uint TargetFramework => 0x40005;

        [SRCategory(SR.General)]
        [ResourcesDisplayName(nameof(Resources.NodeExePath))]
        [ResourcesDescription(nameof(Resources.NodeExePathDescription))]
        public string NodeExePath
        {
            get
            {
                return this.HierarchyNode.ProjectMgr.Site.GetUIThread().Invoke(() =>
                {
                    return Nodejs.GetAbsoluteNodeExePath(
                        this.ProjectHome,
                        this.Node.ProjectMgr.GetProjectProperty(NodeProjectProperty.NodeExePath, true));
                });
            }
            set
            {
                this.HierarchyNode.ProjectMgr.Site.GetUIThread().Invoke(() =>
                {
                    this.Node.ProjectMgr.SetProjectProperty(NodeProjectProperty.NodeExePath, value);
                });
            }
        }

        [SRCategory(SR.General)]
        [ResourcesDisplayName(nameof(Resources.NodeExeArguments))]
        [ResourcesDescription(nameof(Resources.NodeExeArgumentsDescription))]
        public string NodeExeArguments
        {
            get
            {
                return this.HierarchyNode.ProjectMgr.Site.GetUIThread().Invoke(() =>
                {
                    return this.Node.ProjectMgr.GetProjectProperty(NodeProjectProperty.NodeExeArguments, true);
                });
            }
            set
            {
                this.HierarchyNode.ProjectMgr.Site.GetUIThread().Invoke(() =>
                {
                    this.Node.ProjectMgr.SetProjectProperty(NodeProjectProperty.NodeExeArguments, value);
                });
            }
        }

        [SRCategory(SR.General)]
        [SRDisplayName(SR.ScriptArguments)]
        [SRDescription(SR.ScriptArgumentsDescription)]
        public string ScriptArguments
        {
            get
            {
                return this.HierarchyNode.ProjectMgr.Site.GetUIThread().Invoke(() =>
                {
                    return this.Node.ProjectMgr.GetProjectProperty(NodeProjectProperty.ScriptArguments, true);
                });
            }
            set
            {
                this.HierarchyNode.ProjectMgr.Site.GetUIThread().Invoke(() =>
                {
                    this.Node.ProjectMgr.SetProjectProperty(NodeProjectProperty.ScriptArguments, value);
                });
            }
        }

        [SRCategory(SR.General)]
        [ResourcesDisplayName(nameof(Resources.NodejsPort))]
        [ResourcesDescription(nameof(Resources.NodejsPortDescription))]
        public int? NodejsPort
        {
            get
            {
                return this.HierarchyNode.ProjectMgr.Site.GetUIThread().Invoke((Func<int?>)(() =>
                {
                    if (Int32.TryParse(this.Node.ProjectMgr.GetProjectProperty(NodeProjectProperty.NodejsPort, true), out var port))
                    {
                        return port;
                    }
                    return null;
                }));
            }
            set
            {
                this.HierarchyNode.ProjectMgr.Site.GetUIThread().Invoke(() =>
                {
                    this.Node.ProjectMgr.SetProjectProperty(NodeProjectProperty.NodejsPort, value != null ? value.ToString() : string.Empty);
                });
            }
        }

        [SRCategory(SR.General)]
        [SRDisplayName(SR.LaunchUrl)]
        [SRDescription(SR.LaunchUrlDescription)]
        public string LaunchUrl
        {
            get
            {
                return this.HierarchyNode.ProjectMgr.Site.GetUIThread().Invoke(() =>
                {
                    return this.Node.ProjectMgr.GetProjectProperty(NodeProjectProperty.LaunchUrl, true);
                });
            }
            set
            {
                this.HierarchyNode.ProjectMgr.Site.GetUIThread().Invoke(() =>
                {
                    this.Node.ProjectMgr.SetProjectProperty(NodeProjectProperty.LaunchUrl, value);
                });
            }
        }

        [SRCategory(SR.General)]
        [SRDisplayName(SR.StartWebBrowser)]
        [SRDescription(SR.StartWebBrowserDescription)]
        public bool StartWebBrowser
        {
            get
            {
                return this.HierarchyNode.ProjectMgr.Site.GetUIThread().Invoke(() =>
                {
                    if (Boolean.TryParse(this.Node.ProjectMgr.GetProjectProperty(NodeProjectProperty.StartWebBrowser, true), out var res))
                    {
                        return res;
                    }
                    return true;
                });
            }
            set
            {
                this.HierarchyNode.ProjectMgr.Site.GetUIThread().Invoke(() =>
                {
                    this.Node.ProjectMgr.SetProjectProperty(NodeProjectProperty.StartWebBrowser, value.ToString());
                });
            }
        }

        object EnvDTE80.IInternalExtenderProvider.GetExtender(string extenderCATID, string extenderName, object extendeeObject, EnvDTE.IExtenderSite extenderSite, int cookie)
        {
            var outerHierarchy = this.Node.GetOuterInterface<EnvDTE80.IInternalExtenderProvider>();

            if (outerHierarchy != null)
            {
                var res = outerHierarchy.GetExtender(extenderCATID, extenderName, extendeeObject, extenderSite, cookie);
                if (extenderName == "WebApplication" && res is ICustomTypeDescriptor)
                {
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
    public class WebAppExtenderFilter : ICustomTypeDescriptor
    {
        private readonly ICustomTypeDescriptor _innerObject;

        public WebAppExtenderFilter(ICustomTypeDescriptor innerObject)
        {
            this._innerObject = innerObject;
        }

        internal object InnerObject => this._innerObject;

        #region ICustomTypeDescriptor Members

        public AttributeCollection GetAttributes()
        {
            return this._innerObject.GetAttributes();
        }

        public string GetClassName()
        {
            return this._innerObject.GetClassName();
        }

        public string GetComponentName()
        {
            return this._innerObject.GetComponentName();
        }

        public TypeConverter GetConverter()
        {
            return this._innerObject.GetConverter();
        }

        public EventDescriptor GetDefaultEvent()
        {
            return this._innerObject.GetDefaultEvent();
        }

        public PropertyDescriptor GetDefaultProperty()
        {
            return this._innerObject.GetDefaultProperty();
        }

        public object GetEditor(Type editorBaseType)
        {
            return this._innerObject.GetEditor(editorBaseType);
        }

        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return this._innerObject.GetEvents(attributes);
        }

        public EventDescriptorCollection GetEvents()
        {
            return this._innerObject.GetEvents();
        }

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            var res = new List<PropertyDescriptor>();
            var original = this._innerObject.GetProperties(attributes);
            foreach (PropertyDescriptor item in original)
            {
                if (!IsFiltered(item))
                {
                    res.Add(item);
                }
            }

            return new PropertyDescriptorCollection(res.ToArray());
        }

        private static bool IsFiltered(PropertyDescriptor item)
        {
            return item.Name == "StartWebServerOnDebug";
        }

        public PropertyDescriptorCollection GetProperties()
        {
            var res = new List<PropertyDescriptor>();
            var original = this._innerObject.GetProperties();
            foreach (PropertyDescriptor item in original)
            {
                if (!IsFiltered(item))
                {
                    res.Add(item);
                }
            }

            return new PropertyDescriptorCollection(res.ToArray());
        }

        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            return this._innerObject.GetPropertyOwner(pd);
        }

        #endregion
    }
}
