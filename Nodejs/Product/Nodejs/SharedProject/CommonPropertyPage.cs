// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Build.Construction;
using Microsoft.VisualStudio.Editors.PropertyPages;

namespace Microsoft.VisualStudioTools.Project
{
    /// <summary>
    /// Base class for property pages based on a WinForm control.
    /// </summary>
    public abstract class CommonPropertyPage : PropPageBase
    {
        private CommonProjectNode _project;

        protected override Size DefaultSize { get; set; } = new Size(800, 600);

        public abstract void LoadSettings();

        public abstract Control Control { get; }

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

        protected override void SetObjects(uint count, object[] punk)
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
    }
}
