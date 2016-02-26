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
using System.Text;
using System.Windows.Forms;
using Microsoft.NodejsTools.Npm;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudioTools;
using Microsoft.VisualStudioTools.Project;
using VsCommands2K = Microsoft.VisualStudio.VSConstants.VSStd2KCmdID;

namespace Microsoft.NodejsTools.Project {
    internal class DependencyNode : HierarchyNode {
        private readonly NodejsProjectNode _projectNode;
        private readonly DependencyNode _parent;
        private readonly string _displayString;

        public DependencyNode(
            NodejsProjectNode root,
            DependencyNode parent,
            IPackage package)
            : base(root) {
            _projectNode = root;
            _parent = parent;
            Package = package;

            var buff = new StringBuilder(package.Name);
            if (package.IsMissing) {
                buff.Append(" (missing)");
            } else {
                buff.Append('@');
                buff.Append(package.Version);

                if (!package.IsListedInParentPackageJson) {
                    buff.AppendFormat(" (not listed in {0})", NodejsConstants.PackageJsonFile);
                } else {
                    List<string> dependencyTypes = new List<string>(3);
                    if (package.IsDependency) {
                        dependencyTypes.Add("standard");
                    }
                    if (package.IsDevDependency) {
                        dependencyTypes.Add("dev");
                    }
                    if (package.IsOptionalDependency) {
                        dependencyTypes.Add("optional");
                    }

                    if (package.IsDevDependency || package.IsOptionalDependency) {
                        buff.Append(" (");
                        buff.Append(string.Join(", ", dependencyTypes.ToArray()));
                        buff.Append(")");
                    }
                }
            }

            if (package.IsBundledDependency) {
                buff.Append("[bundled]");
            }

            _displayString = buff.ToString();
            ExcludeNodeFromScc = true;
        }

        public IPackage Package { get; internal set; }

        internal INpmController NpmController {
            get {
                if (null != _projectNode) {
                    var modulesNode = _projectNode.ModulesNode;
                    if (null != modulesNode) {
                        return modulesNode.NpmController;
                    }
                }
                return null;
            }
        }

        #region HierarchyNode implementation

        private string GetRelativeUrlFragment() {
            var buff = new StringBuilder();
            if (null != _parent) {
                buff.Append(_parent.GetRelativeUrlFragment());
                buff.Append('/');
            }
            buff.Append("node_modules/");
            buff.Append(Package.Name);
            return buff.ToString();
        }

        public override string Url {
            get { return new Url(ProjectMgr.BaseURI, GetRelativeUrlFragment()).AbsoluteUrl; }
        }

        public override string Caption {
            get { return _displayString; }
        }

        public override Guid ItemTypeGuid {
            get { return VSConstants.GUID_ItemType_VirtualFolder; }
        }

        public override Guid MenuGroupId {
            get { return Guids.NodejsNpmCmdSet; }
        }

        public override int MenuCommandId {
            get { return PkgCmdId.menuIdNpm; }
        }

#if DEV14_OR_LATER
        [Obsolete]
#endif
        public override object GetIconHandle(bool open) {
            int imageIndex = _projectNode.ImageIndexFromNameDictionary[NodejsProjectImageName.Dependency];
            if (Package.IsMissing) {
                imageIndex = _projectNode.ImageIndexFromNameDictionary[NodejsProjectImageName.DependencyMissing];
            } else {
                if (!Package.IsListedInParentPackageJson) {
                    imageIndex = _projectNode.ImageIndexFromNameDictionary[NodejsProjectImageName.DependencyNotListed];
                } else {
                    imageIndex = _projectNode.ImageIndexFromNameDictionary[NodejsProjectImageName.Dependency];
                }
            }

            return _projectNode.ImageHandler.GetIconHandle(imageIndex);
        }

        public override string GetEditLabel() {
            return null;
        }

        protected override NodeProperties CreatePropertiesObject() {
            return new DependencyNodeProperties(this);
        }

        internal DependencyNodeProperties GetPropertiesObject() {
            return CreatePropertiesObject() as DependencyNodeProperties;
        }

#endregion

#region Command handling

        internal override int QueryStatusOnNode(Guid cmdGroup, uint cmd, IntPtr pCmdText, ref QueryStatusResult result) {
            //  Latter condition is because it's only valid to carry out npm operations
            //  on top level dependencies of the user's project, not sub-dependencies.
            //  Performing operations on sub-dependencies would just break things.
            if (cmdGroup == Guids.NodejsNpmCmdSet) {
                switch (cmd) {
                    case PkgCmdId.cmdidNpmOpenModuleHomepage:
                        if (this.Package.Homepages != null) {
                            using (var enumerator = this.Package.Homepages.GetEnumerator()) {
                                if (enumerator.MoveNext() && !string.IsNullOrEmpty(enumerator.Current)) {
                                    result = QueryStatusResult.ENABLED | QueryStatusResult.SUPPORTED;
                                } else {
                                    result = QueryStatusResult.SUPPORTED;
                                }
                            }
                        }
                        return VSConstants.S_OK;
                }

                if (null == _parent) {
                    switch (cmd) {
                        case PkgCmdId.cmdidNpmInstallSingleMissingModule:
                            if (GetPropertiesObject().IsGlobalInstall) {
                                result = QueryStatusResult.SUPPORTED | QueryStatusResult.INVISIBLE;
                            } else if (null == _projectNode.ModulesNode
                                || _projectNode.ModulesNode.IsCurrentStateASuppressCommandsMode()) {
                                result = QueryStatusResult.SUPPORTED;
                            } else {
                                if (null != Package && Package.IsMissing) {
                                    result = QueryStatusResult.ENABLED | QueryStatusResult.SUPPORTED;
                                } else {
                                    result = QueryStatusResult.SUPPORTED;
                                }
                            }
                            return VSConstants.S_OK;

                        case PkgCmdId.cmdidNpmUpdateSingleModule:
                        case PkgCmdId.cmdidNpmUninstallModule:
                            if (null != _projectNode.ModulesNode &&
                                !_projectNode.ModulesNode.IsCurrentStateASuppressCommandsMode()) {
                                result = QueryStatusResult.ENABLED | QueryStatusResult.SUPPORTED;
                            } else {
                                result = QueryStatusResult.SUPPORTED;
                            }
                            return VSConstants.S_OK;

                        case PkgCmdId.cmdidNpmInstallModules:
                        case PkgCmdId.cmdidNpmUpdateModules:
                            result = QueryStatusResult.SUPPORTED | QueryStatusResult.INVISIBLE;
                            return VSConstants.S_OK;
                    }
                }
            } else if (cmdGroup == Microsoft.VisualStudioTools.Project.VsMenus.guidStandardCommandSet2K) {
                switch ((VsCommands2K)cmd) {
                    case CommonConstants.OpenFolderInExplorerCmdId:
                        result = QueryStatusResult.SUPPORTED | QueryStatusResult.ENABLED;
                        return VSConstants.S_OK;
                }
            }

            return base.QueryStatusOnNode(cmdGroup, cmd, pCmdText, ref result);
        }

        internal override int ExecCommandOnNode(Guid cmdGroup, uint cmd, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut) {
            if (cmdGroup == Guids.NodejsNpmCmdSet) {
                switch (cmd) {
                    case PkgCmdId.cmdidNpmOpenModuleHomepage:
                        if (this.Package.Homepages != null) {
                            using (var enumerator = this.Package.Homepages.GetEnumerator()) {
                                if (enumerator.MoveNext() && !string.IsNullOrEmpty(enumerator.Current)) {
                                    Process.Start(enumerator.Current);
                                }
                            }
                        }
                        return VSConstants.S_OK;
                }
                if (null == _parent) {
                    switch (cmd) {
                        case PkgCmdId.cmdidNpmInstallSingleMissingModule:
                            if (null != _projectNode.ModulesNode) {
                                var t = _projectNode.ModulesNode.InstallMissingModule(this);
                            }
                            return VSConstants.S_OK;

                        case PkgCmdId.cmdidNpmUninstallModule:
                            if (null != _projectNode.ModulesNode) {
                                var t = _projectNode.ModulesNode.UninstallModule(this);
                            }
                            return VSConstants.S_OK;

                        case PkgCmdId.cmdidNpmUpdateSingleModule:
                            if (null != _projectNode.ModulesNode) {
                                var t = _projectNode.ModulesNode.UpdateModule(this);
                            }
                            return VSConstants.S_OK;
                    }
                }
            } else if (cmdGroup == Microsoft.VisualStudioTools.Project.VsMenus.guidStandardCommandSet2K) {
                switch ((VsCommands2K)cmd) {
                    case CommonConstants.OpenFolderInExplorerCmdId:
                        string path = this.Package.Path;
                        try {
                            Process.Start(path);
                        } catch (Exception ex) {
                            if (ex is InvalidOperationException || ex is Win32Exception) {
                                MessageBox.Show(
                                    String.Format("Path to module does not exist:\n {0}", path),
                                    SR.ProductName,
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                                return VSConstants.S_FALSE;
                            }
                            throw;
                        }
                        return VSConstants.S_OK;
                }
            }

            return base.ExecCommandOnNode(cmdGroup, cmd, nCmdexecopt, pvaIn, pvaOut);
        }

#endregion
    }
}