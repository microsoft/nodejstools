// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

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
        private IPropertyPageSite site;
        private CommonProjectNode _project;
        private bool dirty;

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
            var conditionTrimmed = (condition == null) ? string.Empty : condition.Trim();

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
                    if (StringComparer.OrdinalIgnoreCase.Equals(group.Condition.Trim(), conditionTrimmed))
                    {
                        matchingGroup = group;
                        break;
                    }
                }

                if (matchingGroup != null)
                {
                    foreach (var property in matchingGroup.PropertiesReversed) // If there's dupes, pick the last one so we win
                    {
                        if (StringComparer.OrdinalIgnoreCase.Equals(property.Name, propertyName)
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
            var conditionTrimmed = (condition == null) ? string.Empty : condition.Trim();
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
                if (StringComparer.OrdinalIgnoreCase.Equals(group.Condition.Trim(), conditionTrimmed))
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
                if (StringComparer.OrdinalIgnoreCase.Equals(property.Name, propertyName)
                    && (property.Condition == null || property.Condition.Length == 0))
                {
                    property.Value = propertyValue;
                    return;
                }
            }

            newGroup.AddProperty(propertyName, propertyValue);
        }

        public bool Loading { get; set; }

        public bool IsDirty
        {
            get
            {
                return this.dirty;
            }
            set
            {
                if (this.dirty != value && !this.Loading)
                {
                    this.dirty = value;
                    if (this.site != null)
                    {
                        this.site.OnStatusChange((uint)(this.dirty ? PropPageStatus.Dirty : PropPageStatus.Clean));
                    }
                }
            }
        }

        void IPropertyPage.Activate(IntPtr hWndParent, RECT[] pRect, int bModal)
        {
            this.Control.Visible = false;

            // suspend to reduce flashing
            this.Control.SuspendLayout();

            try
            {
                var parent = Control.FromHandle(hWndParent);
                this.Control.Parent = parent;

                // move to final location
                ((IPropertyPage)this).Move(pRect);
            }
            finally
            {
                this.Control.ResumeLayout();
                this.Control.Visible = true;
                this.Control.Focus();
            }
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
            // not implemented
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
            this.site = pPageSite;
        }

        void IPropertyPage.Show(uint nCmdShow)
        {
            const int SW_HIDE = 0;

            if (nCmdShow != SW_HIDE)
            {
                this.Control.Visible = true;
                this.Control.Show();
            }
            else
            {
                this.Control.Visible = false;
                this.Control.Hide();
            }
        }

        int IPropertyPage.TranslateAccelerator(MSG[] pMsg)
        {
            Utilities.ArgumentNotNull("pMsg", pMsg);

            var msg = pMsg[0];

            var message = Message.Create(msg.hwnd, (int)msg.message, msg.wParam, msg.lParam);

            var target = Control.FromChildHandle(message.HWnd);
            if (target != null && target.PreProcessMessage(ref message))
            {
                // handled the message
                pMsg[0].message = (uint)message.Msg;
                pMsg[0].wParam = message.WParam;
                pMsg[0].lParam = message.LParam;

                return VSConstants.S_OK;
            }

            return VSConstants.S_FALSE;
        }
    }
}
