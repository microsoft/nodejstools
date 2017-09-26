// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.NodejsTools.Npm;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools.Project;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Imaging;

namespace Microsoft.NodejsTools.Project
{
    internal abstract class AbstractNpmNode : HierarchyNode
    {
        protected readonly NodejsProjectNode _projectNode;

        protected AbstractNpmNode(NodejsProjectNode root)
            : base(root)
        {
            this._projectNode = root;
            this.ExcludeNodeFromScc = true;
        }

        #region HierarchyNode implementation

        public override Guid ItemTypeGuid => VSConstants.GUID_ItemType_VirtualFolder;
        public override Guid MenuGroupId => Guids.NodejsNpmCmdSet;
        public override int MenuCommandId => PkgCmdId.menuIdNpm;
        
        /// <summary>
        /// Disable inline editing of Caption.
        /// </summary>
        public sealed override string GetEditLabel()
        {
            return null;
        }

        protected override bool SupportsIconMonikers => true;
        
        /// <summary>
        /// Returns the icon to use.
        /// </summary>
        protected override ImageMoniker GetIconMoniker(bool open)
        {
            return KnownMonikers.Reference;
        }

        protected override NodeProperties CreatePropertiesObject()
        {
            return new NpmNodeProperties(this);
        }
        #endregion

        public abstract void ManageNpmModules();

        protected void ReloadHierarchy(HierarchyNode parent, IEnumerable<IPackage> modules)
        {
            //  We're going to reuse nodes for which matching modules exist in the new set.
            //  The reason for this is that we want to preserve the expansion state of the
            //  hierarchy. If we just bin everything off and recreate it all from scratch
            //  it'll all be in the collapsed state, which will be annoying for users who
            //  have drilled down into the hierarchy
            var recycle = new Dictionary<string, DependencyNode>();
            var remove = GetNodesToRemoveOrRecycle(parent, modules, recycle);
            RemoveUnusedModuleNodesFromHierarchy(parent, remove);
            BuildModuleHierarchy(parent, modules, recycle);
        }

        private void RemoveUnusedModuleNodesFromHierarchy(HierarchyNode parent, List<HierarchyNode> remove)
        {
            foreach (var obsolete in remove)
            {
                parent.RemoveChild(obsolete);
                this.ProjectMgr.OnItemDeleted(obsolete);
            }
        }

        private IEnumerable<IPackage> BuildModuleHierarchy(HierarchyNode parent, IEnumerable<IPackage> modules, IReadOnlyDictionary<string, DependencyNode> recycle)
        {
            if (modules == null)
            {
                return Enumerable.Empty<IPackage>();
            }

            var newModules = new List<IPackage>();
            foreach (var package in modules)
            {
                DependencyNode child;
                if (recycle.ContainsKey(package.Name))
                {
                    child = recycle[package.Name];
                    child.Package = package;
                }
                else
                {
                    child = new DependencyNode(this._projectNode, parent as DependencyNode, package);
                    parent.AddChild(child);
                    newModules.Add(package);
                }

                ReloadHierarchy(child, package.Modules);
                if (this.ProjectMgr.ParentHierarchy != null)
                {
                    child.ExpandItem(EXPANDFLAGS.EXPF_CollapseFolder);
                }
            }
            return newModules;
        }

        /// <summary>
        /// Compute the nodes that should be removed or recycled from the current heirarchy
        /// </summary>
        /// <param name="parent">Existing heirarchy</param>
        /// <param name="modules">New set of modules</param>
        /// <param name="recycle">Set of existing nodes that should be reused</param>
        /// <returns>List of nodes that should be removed</returns>
        private static List<HierarchyNode> GetNodesToRemoveOrRecycle(HierarchyNode parent, IEnumerable<IPackage> modules, IDictionary<string, DependencyNode> recycle)
        {
            var remove = new List<HierarchyNode>();
            for (var current = parent.FirstChild; null != current; current = current.NextSibling)
            {
                var dep = current as DependencyNode;
                if (null == dep)
                {
                    if (!(current is LocalModulesNode))
                    {
                        remove.Add(current);
                    }
                    continue;
                }

                if (modules != null && modules.Contains(dep.Package, PackageEqualityComparer.Instance))
                {
                    recycle[dep.Package.Name] = dep;
                }
                else
                {
                    remove.Add(current);
                }
            }
            return remove;
        }
    }
}
