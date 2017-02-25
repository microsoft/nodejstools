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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Build.Construction;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;

namespace Microsoft.VisualStudioTools.Project
{
    /// <summary>
    /// Base class for property pages based on a WinForm control.
    /// </summary>
    public abstract class CommonPropertyPage : IPropertyPage
    {
        private IPropertyPageSite _site;
        private bool _dirty, _loading;
        private CommonProjectNode _project;

        public abstract Control Control
        {
            get;
        }

        public abstract void Apply();
        public abstract void LoadSettings();

        public abstract string Name
        {
            get;
        }

        internal virtual CommonProjectNode Project
        {
            get
            {
                return this._project;
            }
            set
            {
                this._project = value;
            }
        }

        internal virtual IEnumerable<CommonProjectConfig> SelectedConfigs
        {
            get; set;
        }

        protected void SetProjectProperty(string propertyName, string propertyValue)
        {
            // SetProjectProperty's implementation will check whether the value
            // has changed.
            this.Project.SetProjectProperty(propertyName, propertyValue);
        }

        protected string GetProjectProperty(string propertyName)
        {
            return this.Project.GetUnevaluatedProperty(propertyName);
        }

        protected void SetUserProjectProperty(string propertyName, string propertyValue)
        {
            // SetUserProjectProperty's implementation will check whether the value
            // has changed.
            this.Project.SetUserProjectProperty(propertyName, propertyValue);
        }

        protected string GetUserProjectProperty(string propertyName)
        {
            return this.Project.GetUserProjectProperty(propertyName);
        }

        protected string GetConfigUserProjectProperty(string propertyName)
        {
            if (this.SelectedConfigs == null)
            {
                var condition = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    ConfigProvider.configPlatformString,
                    this.Project.CurrentConfig.GetPropertyValue("Configuration"),
                    this.Project.CurrentConfig.GetPropertyValue("Platform"));

                return GetUserPropertyUnderCondition(propertyName, condition);
            }
            else
            {
                var values = new StringCollection();

                foreach (var config in this.SelectedConfigs)
                {
                    var condition = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                        ConfigProvider.configPlatformString,
                        config.ConfigName,
                        config.PlatformName);

                    values.Add(GetUserPropertyUnderCondition(propertyName, condition));
                }

                switch (values.Count)
                {
                    case 0:
                        return null;
                    case 1:
                        return values[0];
                    default:
                        return "<different values>";
                }
            }
        }

        protected void SetConfigUserProjectProperty(string propertyName, string propertyValue)
        {
            if (this.SelectedConfigs == null)
            {
                var condition = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    ConfigProvider.configPlatformString,
                    this.Project.CurrentConfig.GetPropertyValue("Configuration"),
                    this.Project.CurrentConfig.GetPropertyValue("Platform"));

                SetUserPropertyUnderCondition(propertyName, propertyValue, condition);
            }
            else
            {
                foreach (var config in this.SelectedConfigs)
                {
                    var condition = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                        ConfigProvider.configPlatformString,
                        config.ConfigName,
                        config.GetConfigurationProperty("Platform", false));

                    SetUserPropertyUnderCondition(propertyName, propertyValue, condition);
                }
            }
        }

        private string GetUserPropertyUnderCondition(string propertyName, string condition)
        {
            var conditionTrimmed = (condition == null) ? String.Empty : condition.Trim();

            if (this.Project.UserBuildProject != null)
            {
                if (conditionTrimmed.Length == 0)
                {
                    return this.Project.UserBuildProject.GetProperty(propertyName).UnevaluatedValue;
                }

                // New OM doesn't have a convenient equivalent for setting a property with a particular property group condition. 
                // So do it ourselves.
                ProjectPropertyGroupElement matchingGroup = null;

                foreach (var group in this.Project.UserBuildProject.Xml.PropertyGroups)
                {
                    if (String.Equals(group.Condition.Trim(), conditionTrimmed, StringComparison.OrdinalIgnoreCase))
                    {
                        matchingGroup = group;
                        break;
                    }
                }

                if (matchingGroup != null)
                {
                    foreach (var property in matchingGroup.PropertiesReversed) // If there's dupes, pick the last one so we win
                    {
                        if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase)
                            && (property.Condition == null || property.Condition.Length == 0))
                        {
                            return property.Value;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Emulates the behavior of SetProperty(name, value, condition) on the old MSBuild object model.
        /// This finds a property group with the specified condition (or creates one if necessary) then sets the property in there.
        /// </summary>
        private void SetUserPropertyUnderCondition(string propertyName, string propertyValue, string condition)
        {
            var conditionTrimmed = (condition == null) ? String.Empty : condition.Trim();
            const string userProjectCreateProperty = "UserProject";

            if (this.Project.UserBuildProject == null)
            {
                this.Project.SetUserProjectProperty(userProjectCreateProperty, null);
            }

            if (conditionTrimmed.Length == 0)
            {
                var userProp = this.Project.UserBuildProject.GetProperty(userProjectCreateProperty);
                if (userProp != null)
                {
                    this.Project.UserBuildProject.RemoveProperty(userProp);
                }
                this.Project.UserBuildProject.SetProperty(propertyName, propertyValue);
                return;
            }

            // New OM doesn't have a convenient equivalent for setting a property with a particular property group condition. 
            // So do it ourselves.
            ProjectPropertyGroupElement newGroup = null;

            foreach (var group in this.Project.UserBuildProject.Xml.PropertyGroups)
            {
                if (String.Equals(group.Condition.Trim(), conditionTrimmed, StringComparison.OrdinalIgnoreCase))
                {
                    newGroup = group;
                    break;
                }
            }

            if (newGroup == null)
            {
                newGroup = this.Project.UserBuildProject.Xml.AddPropertyGroup(); // Adds after last existing PG, else at start of project
                newGroup.Condition = condition;
            }

            foreach (var property in newGroup.PropertiesReversed) // If there's dupes, pick the last one so we win
            {
                if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase)
                    && (property.Condition == null || property.Condition.Length == 0))
                {
                    property.Value = propertyValue;
                    return;
                }
            }

            newGroup.AddProperty(propertyName, propertyValue);
        }

        public bool Loading
        {
            get
            {
                return this._loading;
            }
            set
            {
                this._loading = value;
            }
        }

        public bool IsDirty
        {
            get
            {
                return this._dirty;
            }
            set
            {
                if (this._dirty != value && !this.Loading)
                {
                    this._dirty = value;
                    if (this._site != null)
                    {
                        this._site.OnStatusChange((uint)(this._dirty ? PropPageStatus.Dirty : PropPageStatus.Clean));
                    }
                }
            }
        }

        void IPropertyPage.Activate(IntPtr hWndParent, RECT[] pRect, int bModal)
        {
            NativeMethods.SetParent(this.Control.Handle, hWndParent);
        }

        int IPropertyPage.Apply()
        {
            try
            {
                Apply();
                return VSConstants.S_OK;
            }
            catch (Exception e)
            {
                return Marshal.GetHRForException(e);
            }
        }

        void IPropertyPage.Deactivate()
        {
            this.Project = null;
            this.Control.Dispose();
        }

        void IPropertyPage.GetPageInfo(PROPPAGEINFO[] pPageInfo)
        {
            Utilities.ArgumentNotNull("pPageInfo", pPageInfo);

            var info = new PROPPAGEINFO();

            info.cb = (uint)Marshal.SizeOf(typeof(PROPPAGEINFO));
            info.dwHelpContext = 0;
            info.pszDocString = null;
            info.pszHelpFile = null;
            info.pszTitle = this.Name;
            info.SIZE.cx = this.Control.Width;
            info.SIZE.cy = this.Control.Height;
            pPageInfo[0] = info;
        }

        void IPropertyPage.Help(string pszHelpDir)
        {
        }

        int IPropertyPage.IsPageDirty()
        {
            return (this.IsDirty ? (int)VSConstants.S_OK : (int)VSConstants.S_FALSE);
        }

        void IPropertyPage.Move(RECT[] pRect)
        {
            Utilities.ArgumentNotNull("pRect", pRect);

            var r = pRect[0];

            this.Control.Location = new Point(r.left, r.top);
            this.Control.Size = new Size(r.right - r.left, r.bottom - r.top);
        }

        void IPropertyPage.SetObjects(uint count, object[] punk)
        {
            if (punk == null)
            {
                return;
            }

            if (count > 0)
            {
                if (punk[0] is ProjectConfig)
                {
                    if (this._project == null)
                    {
                        this._project = (CommonProjectNode)((CommonProjectConfig)punk.First()).ProjectMgr;
                    }

                    var configs = new List<CommonProjectConfig>();

                    for (var i = 0; i < count; i++)
                    {
                        var config = (CommonProjectConfig)punk[i];

                        configs.Add(config);
                    }

                    this.SelectedConfigs = configs;
                }
                else if (punk[0] is NodeProperties)
                {
                    if (this._project == null)
                    {
                        this.Project = (CommonProjectNode)(punk[0] as NodeProperties).HierarchyNode.ProjectMgr;
                    }
                }
            }
            else
            {
                this.Project = null;
            }

            if (this._project != null)
            {
                LoadSettings();
            }
        }

        void IPropertyPage.SetPageSite(IPropertyPageSite pPageSite)
        {
            this._site = pPageSite;
        }

        void IPropertyPage.Show(uint nCmdShow)
        {
            this.Control.Visible = true; // TODO: pass SW_SHOW* flags through      
            this.Control.Show();
        }

        int IPropertyPage.TranslateAccelerator(MSG[] pMsg)
        {
            Utilities.ArgumentNotNull("pMsg", pMsg);

            var msg = pMsg[0];

            if ((msg.message < NativeMethods.WM_KEYFIRST || msg.message > NativeMethods.WM_KEYLAST) && (msg.message < NativeMethods.WM_MOUSEFIRST || msg.message > NativeMethods.WM_MOUSELAST))
            {
                return VSConstants.S_FALSE;
            }

            return (NativeMethods.IsDialogMessageA(this.Control.Handle, ref msg)) ? VSConstants.S_OK : VSConstants.S_FALSE;
        }
    }
}
