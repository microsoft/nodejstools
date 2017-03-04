// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Build.Construction;
using Microsoft.NodejsTools.Project;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools.Project;

namespace Microsoft.NodejsTools
{
    [Guid(Guids.NodejsBaseProjectFactoryString)]
    internal class BaseNodeProjectFactory : ProjectFactory
    {
        public BaseNodeProjectFactory(NodejsProjectPackage package)
            : base((IServiceProvider)package)
        {
        }

        internal override ProjectNode CreateProject()
        {
            var project = new NodejsProjectNode((NodejsProjectPackage)this.Site);
            return project;
        }

        protected override ProjectUpgradeState UpgradeProjectCheck(ProjectRootElement projectXml, ProjectRootElement userProjectXml, Action<__VSUL_ERRORLEVEL, string> log, ref Guid projectFactory, ref __VSPPROJECTUPGRADEVIAFACTORYFLAGS backupSupport)
        {
            var envVarsProp = projectXml.Properties.FirstOrDefault(p => p.Name == NodeProjectProperty.EnvironmentVariables);
            if (envVarsProp != null && !string.IsNullOrEmpty(envVarsProp.Value))
            {
                return ProjectUpgradeState.OneWayUpgrade;
            }

            return ProjectUpgradeState.NotNeeded;
        }

        protected override void UpgradeProject(ref ProjectRootElement projectXml, ref ProjectRootElement userProjectXml, Action<__VSUL_ERRORLEVEL, string> log)
        {
            var envVarsProp = projectXml.Properties.FirstOrDefault(p => p.Name == NodeProjectProperty.EnvironmentVariables);
            if (envVarsProp != null)
            {
                var globals = projectXml.PropertyGroups.FirstOrDefault() ?? projectXml.AddPropertyGroup();
                AddOrSetProperty(globals, NodeProjectProperty.Environment, envVarsProp.Value.Replace(";", "\r\n"));
                envVarsProp.Parent.RemoveChild(envVarsProp);
                log(__VSUL_ERRORLEVEL.VSUL_INFORMATIONAL, Resources.UpgradedEnvironmentVariables);
            }
        }

        private static void AddOrSetProperty(ProjectPropertyGroupElement group, string name, string value)
        {
            var anySet = false;
            foreach (var prop in group.Properties.Where(p => p.Name == name))
            {
                prop.Value = value;
                anySet = true;
            }

            if (!anySet)
            {
                group.AddProperty(name, value);
            }
        }
    }
}

