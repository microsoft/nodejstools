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
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Build.Construction;
using Microsoft.NodejsTools.Project;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools.Project;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
using SR = Microsoft.NodejsTools.Project.SR;

namespace Microsoft.NodejsTools {
    [Guid(Guids.NodejsBaseProjectFactoryString)]
    class BaseNodeProjectFactory : ProjectFactory {
        public BaseNodeProjectFactory(NodejsProjectPackage package) : base(package) {
        }

        internal override ProjectNode CreateProject() {
            NodejsProjectNode project = new NodejsProjectNode((NodejsProjectPackage)Package);
            project.SetSite((IOleServiceProvider)((IServiceProvider)Package).GetService(typeof(IOleServiceProvider)));
            return project;
        }

        protected override ProjectUpgradeState UpgradeProjectCheck(ProjectRootElement projectXml, ProjectRootElement userProjectXml, Action<__VSUL_ERRORLEVEL, string> log, ref Guid projectFactory, ref __VSPPROJECTUPGRADEVIAFACTORYFLAGS backupSupport) {
            var envVarsProp = projectXml.Properties.FirstOrDefault(p => p.Name == NodejsConstants.EnvironmentVariables);
            if (envVarsProp != null && !string.IsNullOrEmpty(envVarsProp.Value)) {
                return ProjectUpgradeState.OneWayUpgrade;
            }

            return ProjectUpgradeState.NotNeeded;
        }

        protected override void UpgradeProject(ref ProjectRootElement projectXml, ref ProjectRootElement userProjectXml, Action<__VSUL_ERRORLEVEL, string> log) {
            var envVarsProp = projectXml.Properties.FirstOrDefault(p => p.Name == NodejsConstants.EnvironmentVariables);
            if (envVarsProp != null) {
                var globals = projectXml.PropertyGroups.FirstOrDefault() ?? projectXml.AddPropertyGroup();
                AddOrSetProperty(globals, NodejsConstants.Environment, envVarsProp.Value.Replace(";", "\r\n"));
                envVarsProp.Parent.RemoveChild(envVarsProp);
                log(__VSUL_ERRORLEVEL.VSUL_INFORMATIONAL, SR.GetString(SR.UpgradedEnvironmentVariables));
            }
        }

        private static void AddOrSetProperty(ProjectPropertyGroupElement group, string name, string value) {
            bool anySet = false;
            foreach (var prop in group.Properties.Where(p => p.Name == name)) {
                prop.Value = value;
                anySet = true;
            }

            if (!anySet) {
                group.AddProperty(name, value);
            }
        }
    }
}
