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
using System.Text;
using Microsoft.NodejsTools.Npm;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudioTools.Project;

namespace Microsoft.NodejsTools.Project{
    internal class DependencyNode : HierarchyNode{
        private readonly NodejsProjectNode _projectNode;
        private readonly DependencyNode _parent;
        private readonly string _displayString;

        public DependencyNode(
            NodejsProjectNode root,
            DependencyNode parent,
            IPackage package) : base(root){
            _projectNode = root;
            _parent = parent;
            Package = package;

            var buff = new StringBuilder(package.Name);
            if (package.IsMissing){
                buff.Append(" (missing)");
            } else{
                buff.Append('@');
                buff.Append(package.Version);

                if (! package.IsListedInParentPackageJson){
                    buff.Append(" (not listed in package.json)");
                } else if (package.IsDevDependency){
                    buff.Append(" (dev)");
                } else if (package.IsOptionalDependency){
                    buff.Append(" (optional)");
                }
            }

            if (package.IsBundledDependency){
                buff.Append("[bundled]");
            }

            _displayString = buff.ToString();
            ExcludeNodeFromScc = true;
        }

        public IPackage Package { get; internal set; }

        #region HierarchyNode implementation

        private string GetRelativeUrlFragment(){
            var buff = new StringBuilder();
            if (null != _parent){
                buff.Append(_parent.GetRelativeUrlFragment());
                buff.Append('/');
            }
            buff.Append("node_modules/");
            buff.Append(Package.Name);
            return buff.ToString();
        }

        public override string Url{
            get { return new Url(ProjectMgr.BaseURI, GetRelativeUrlFragment()).AbsoluteUrl; }
        }

        public override string Caption{
            get { return _displayString; }
        }

        public override Guid ItemTypeGuid{
            get { return VSConstants.GUID_ItemType_VirtualFolder; }
        }

        public override object GetIconHandle(bool open){
            int imageIndex = _projectNode.ImageIndexDependency;
            if (Package.IsMissing){
                if (Package.IsDevDependency){
                    imageIndex = _projectNode.ImageIndexDependencyDevMissing;
                } else if (Package.IsOptionalDependency){
                    imageIndex = _projectNode.ImageIndexDependencyOptionalMissing;
                } else if (Package.IsBundledDependency){
                    imageIndex = _projectNode.ImageIndexDependencyBundledMissing;
                } else{
                    imageIndex = _projectNode.ImageIndexDependencyMissing;
                }
            } else{
                if (! Package.IsListedInParentPackageJson){
                    imageIndex = _projectNode.ImageIndexDependencyNotListed;
                } else if (Package.IsDevDependency){
                    imageIndex = _projectNode.ImageIndexDependencyDev;
                } else if (Package.IsOptionalDependency){
                    imageIndex = _projectNode.ImageIndexDependnecyOptional;
                } else if (Package.IsBundledDependency){
                    imageIndex = _projectNode.ImageIndexDependencyBundled;
                } else{
                    imageIndex = _projectNode.ImageIndexDependency;
                }
            }

            return _projectNode.ImageHandler.GetIconHandle(imageIndex);
        }

        public override string GetEditLabel(){
            return null;
        }

        #endregion

        #region Dependency actions

        public async void Uninstall(){
            var modulesNode = _projectNode.ModulesNode;
            if (null != modulesNode){
                using (var commander = modulesNode.NpmController.CreateNpmCommander()){
                    await commander.UninstallPackageAsync(Package.Name);
                }
            }
        }

        #endregion
    }
}