// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using Microsoft.NodejsTools.Npm;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudioTools;
using Microsoft.VisualStudioTools.Project;
using VsCommands2K = Microsoft.VisualStudio.VSConstants.VSStd2KCmdID;

namespace Microsoft.NodejsTools.Project
{
    internal class DependencyNode : HierarchyNode
    {
        private readonly NodejsProjectNode _projectNode;
        private readonly DependencyNode _parent;
        private readonly string _displayString;

        public DependencyNode(
            NodejsProjectNode root,
            DependencyNode parent,
            IPackage package)
            : base(root)
        {
            this._projectNode = root;
            this._parent = parent;
            this.Package = package;

            this._displayString = GetInitialPackageDisplayString(package);
            this.ExcludeNodeFromScc = true;
        }

        public IPackage Package { get; internal set; }

        internal INpmController NpmController
        {
            get
            {
                if (null != this._projectNode)
                {
                    var modulesNode = this._projectNode.ModulesNode;
                    if (null != modulesNode)
                    {
                        return modulesNode.NpmController;
                    }
                }
                return null;
            }
        }

        #region HierarchyNode implementation

        private string GetRelativeUrlFragment()
        {
            var buff = new StringBuilder();
            if (null != this._parent)
            {
                buff.Append(this._parent.GetRelativeUrlFragment());
                buff.Append('/');
            }
            buff.Append("node_modules/");
            buff.Append(this.Package.Name);
            return buff.ToString();
        }

        public override string Url => new Url(this.ProjectMgr.BaseURI, GetRelativeUrlFragment()).AbsoluteUrl;
        public override string Caption => this._displayString;
        public override Guid ItemTypeGuid => VSConstants.GUID_ItemType_VirtualFolder;
        public override Guid MenuGroupId => Guids.NodejsNpmCmdSet;
        public override int MenuCommandId => PkgCmdId.menuIdNpm;
        [Obsolete]
        public override object GetIconHandle(bool open)
        {
            var imageIndex = this._projectNode.ImageIndexFromNameDictionary[NodejsProjectImageName.Dependency];
            if (this.Package.IsMissing)
            {
                imageIndex = this._projectNode.ImageIndexFromNameDictionary[NodejsProjectImageName.DependencyMissing];
            }
            else
            {
                if (!this.Package.IsListedInParentPackageJson)
                {
                    imageIndex = this._projectNode.ImageIndexFromNameDictionary[NodejsProjectImageName.DependencyNotListed];
                }
                else
                {
                    imageIndex = this._projectNode.ImageIndexFromNameDictionary[NodejsProjectImageName.Dependency];
                }
            }

            return this._projectNode.ImageHandler.GetIconHandle(imageIndex);
        }

        public override string GetEditLabel()
        {
            return null;
        }

        protected override NodeProperties CreatePropertiesObject()
        {
            return new DependencyNodeProperties(this);
        }

        internal DependencyNodeProperties GetPropertiesObject()
        {
            return CreatePropertiesObject() as DependencyNodeProperties;
        }

        #endregion

        #region Command handling

        internal override int QueryStatusOnNode(Guid cmdGroup, uint cmd, IntPtr pCmdText, ref QueryStatusResult result)
        {
            //  Latter condition is because it's only valid to carry out npm operations
            //  on top level dependencies of the user's project, not sub-dependencies.
            //  Performing operations on sub-dependencies would just break things.
            if (cmdGroup == Guids.NodejsNpmCmdSet)
            {
                switch (cmd)
                {
                    case PkgCmdId.cmdidNpmOpenModuleHomepage:
                        if (this.Package.Homepages != null)
                        {
                            using (var enumerator = this.Package.Homepages.GetEnumerator())
                            {
                                if (enumerator.MoveNext() && !string.IsNullOrEmpty(enumerator.Current))
                                {
                                    result = QueryStatusResult.ENABLED | QueryStatusResult.SUPPORTED;
                                }
                                else
                                {
                                    result = QueryStatusResult.SUPPORTED;
                                }
                            }
                        }
                        return VSConstants.S_OK;
                }

                if (null == this._parent)
                {
                    switch (cmd)
                    {
                        case PkgCmdId.cmdidNpmInstallSingleMissingModule:
                            if (null == this._projectNode.ModulesNode
                                || this._projectNode.ModulesNode.IsCurrentStateASuppressCommandsMode())
                            {
                                result = QueryStatusResult.SUPPORTED;
                            }
                            else
                            {
                                if (null != this.Package && this.Package.IsMissing)
                                {
                                    result = QueryStatusResult.ENABLED | QueryStatusResult.SUPPORTED;
                                }
                                else
                                {
                                    result = QueryStatusResult.SUPPORTED;
                                }
                            }
                            return VSConstants.S_OK;

                        case PkgCmdId.cmdidNpmUpdateSingleModule:
                        case PkgCmdId.cmdidNpmUninstallModule:
                            if (null != this._projectNode.ModulesNode &&
                                !this._projectNode.ModulesNode.IsCurrentStateASuppressCommandsMode())
                            {
                                result = QueryStatusResult.ENABLED | QueryStatusResult.SUPPORTED;
                            }
                            else
                            {
                                result = QueryStatusResult.SUPPORTED;
                            }
                            return VSConstants.S_OK;

                        case PkgCmdId.cmdidNpmInstallModules:
                        case PkgCmdId.cmdidNpmUpdateModules:
                            result = QueryStatusResult.SUPPORTED | QueryStatusResult.INVISIBLE;
                            return VSConstants.S_OK;
                    }
                }
            }
            else if (cmdGroup == Microsoft.VisualStudioTools.Project.VsMenus.guidStandardCommandSet2K)
            {
                switch ((VsCommands2K)cmd)
                {
                    case CommonConstants.OpenFolderInExplorerCmdId:
                        result = QueryStatusResult.SUPPORTED | QueryStatusResult.ENABLED;
                        return VSConstants.S_OK;
                }
            }

            return base.QueryStatusOnNode(cmdGroup, cmd, pCmdText, ref result);
        }

        internal override int ExecCommandOnNode(Guid cmdGroup, uint cmd, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (cmdGroup == Guids.NodejsNpmCmdSet)
            {
                switch (cmd)
                {
                    case PkgCmdId.cmdidNpmOpenModuleHomepage:
                        if (this.Package.Homepages != null)
                        {
                            using (var enumerator = this.Package.Homepages.GetEnumerator())
                            {
                                if (enumerator.MoveNext() && !string.IsNullOrEmpty(enumerator.Current))
                                {
                                    Process.Start(enumerator.Current);
                                }
                            }
                        }
                        return VSConstants.S_OK;
                }
                if (null == this._parent)
                {
                    switch (cmd)
                    {
                        case PkgCmdId.cmdidNpmInstallSingleMissingModule:
                            if (null != this._projectNode.ModulesNode)
                            {
                                var t = this._projectNode.ModulesNode.InstallMissingModule(this);
                            }
                            return VSConstants.S_OK;

                        case PkgCmdId.cmdidNpmUninstallModule:
                            if (null != this._projectNode.ModulesNode)
                            {
                                var t = this._projectNode.ModulesNode.UninstallModule(this);
                            }
                            return VSConstants.S_OK;

                        case PkgCmdId.cmdidNpmUpdateSingleModule:
                            if (null != this._projectNode.ModulesNode)
                            {
                                var t = this._projectNode.ModulesNode.UpdateModule(this);
                            }
                            return VSConstants.S_OK;
                    }
                }
            }
            else if (cmdGroup == Microsoft.VisualStudioTools.Project.VsMenus.guidStandardCommandSet2K)
            {
                switch ((VsCommands2K)cmd)
                {
                    case CommonConstants.OpenFolderInExplorerCmdId:
                        var path = this.Package.Path;
                        try
                        {
                            Process.Start(path);
                        }
                        catch (Exception ex)
                        {
                            if (ex is InvalidOperationException || ex is Win32Exception)
                            {
                                MessageBox.Show(
                                    string.Format(CultureInfo.CurrentCulture, Resources.DependencyNodeModuleDoesNotExist, path),
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

        private static string GetInitialPackageDisplayString(IPackage package)
        {
            var buff = new StringBuilder(package.Name);
            if (package.IsMissing)
            {
                buff.Append(string.Format(CultureInfo.CurrentCulture, " ({0})", Resources.DependencyNodeLabelMissing));
            }
            else
            {
                buff.Append('@');
                buff.Append(package.Version);

                if (!package.IsListedInParentPackageJson)
                {
                    buff.AppendFormat(string.Format(CultureInfo.CurrentCulture, " ({0})",
                        string.Format(CultureInfo.CurrentCulture, Resources.DependencyNodeLabelNotListed, NodejsConstants.PackageJsonFile)));
                }
                else
                {
                    var dependencyTypes = GetDependencyTypeNames(package);
                    if (package.IsDevDependency || package.IsOptionalDependency)
                    {
                        buff.Append(" (");
                        buff.Append(string.Join(", ", dependencyTypes.ToArray()));
                        buff.Append(")");
                    }
                }
            }

            if (package.IsBundledDependency)
            {
                buff.Append(string.Format(CultureInfo.CurrentCulture, "[{0}]",
                    Resources.DependencyNodeLabelBundled));
            }
            return buff.ToString();
        }

        private static List<string> GetDependencyTypeNames(IPackage package)
        {
            var dependencyTypes = new List<string>(3);
            if (package.IsDependency)
            {
                dependencyTypes.Add("standard");
            }
            if (package.IsDevDependency)
            {
                dependencyTypes.Add("dev");
            }
            if (package.IsOptionalDependency)
            {
                dependencyTypes.Add("optional");
            }
            return dependencyTypes;
        }
    }
}

