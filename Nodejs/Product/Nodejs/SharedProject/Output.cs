// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.Build.Execution;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudioTools.Project
{
    internal class Output : IVsOutput2
    {
        private ProjectNode project;
        private ProjectItemInstance output;

        /// <summary>
        /// Constructor for IVSOutput2 implementation
        /// </summary>
        /// <param name="projectManager">Project that produce this output</param>
        /// <param name="outputAssembly">MSBuild generated item corresponding to the output assembly (by default, these would be of type MainAssembly</param>
        public Output(ProjectNode projectManager, ProjectItemInstance outputAssembly)
        {
            Utilities.ArgumentNotNull("projectManager", projectManager);

            this.project = projectManager;
            this.output = outputAssembly;
        }

        internal string CanonicalName
        {
            get
            {
                return ErrorHandler.Succeeded(get_CanonicalName(out var canonicalName)) ? canonicalName : null;
            }
        }

        internal string GetMetadata(string name)
        {
            return ErrorHandler.Succeeded(get_Property(name, out var value)) ? value as string : null;
        }

        #region IVsOutput2 Members

        public int get_CanonicalName(out string pbstrCanonicalName)
        {
            if (this.output == null)
            {
                pbstrCanonicalName = this.project.Url;
                return VSConstants.S_OK;
            }

            // Get the output assembly path (including the name)
            pbstrCanonicalName = this.output.GetMetadataValue("FullPath");
            Debug.Assert(!string.IsNullOrEmpty(pbstrCanonicalName), "Output Assembly not defined");

            // Make sure we have a full path
            pbstrCanonicalName = CommonUtils.GetAbsoluteFilePath(this.project.ProjectHome, pbstrCanonicalName);
            return VSConstants.S_OK;
        }

        /// <summary>
        /// This path must start with file:/// if it wants other project
        /// to be able to reference the output on disk.
        /// If the output is not on disk, then this requirement does not
        /// apply as other projects probably don't know how to access it.
        /// </summary>
        public virtual int get_DeploySourceURL(out string pbstrDeploySourceURL)
        {
            if (this.output == null)
            {
                // we're lying here to keep callers happy who expect a path...  See also OutputGroup.get_KeyOutputObject
                pbstrDeploySourceURL = GetType().Assembly.CodeBase;
                return VSConstants.S_OK;
            }

            var path = this.output.GetMetadataValue(ProjectFileConstants.FinalOutputPath);
            if (string.IsNullOrEmpty(path))
            {
                pbstrDeploySourceURL = new Url(this.output.GetMetadataValue("FullPath")).Uri.AbsoluteUri;
                return VSConstants.S_OK;
            }
            if (path.Length < 9 || !StringComparer.OrdinalIgnoreCase.Equals(path.Substring(0, 8), "file:///"))
            {
                path = "file:///" + path;
            }
            pbstrDeploySourceURL = path;
            return VSConstants.S_OK;
        }

        public int get_DisplayName(out string pbstrDisplayName)
        {
            return this.get_CanonicalName(out pbstrDisplayName);
        }

        public virtual int get_Property(string szProperty, out object pvar)
        {
            if (this.output == null)
            {
                switch (szProperty)
                {
                    case "FinalOutputPath":
                        pvar = typeof(string).Assembly.CodeBase;
                        return VSConstants.S_OK;
                }
                pvar = null;
                return VSConstants.E_NOTIMPL;
            }
            var value = this.output.GetMetadataValue(szProperty);
            pvar = value;

            // If we don't have a value, we are expected to return unimplemented
            return string.IsNullOrEmpty(value) ? VSConstants.E_NOTIMPL : VSConstants.S_OK;
        }

        public int get_RootRelativeURL(out string pbstrRelativePath)
        {
            if (this.output == null)
            {
                pbstrRelativePath = this.project.ProjectHome;
                return VSConstants.E_FAIL;
            }

            pbstrRelativePath = string.Empty;
            // get the corresponding property

            if (ErrorHandler.Succeeded(this.get_Property("TargetPath", out var variant)))
            {
                var var = variant as String;

                if (var != null)
                {
                    pbstrRelativePath = var;
                }
            }
            else
            {
                var baseDir = this.project.ProjectHome;
                var fullPath = this.output.GetMetadataValue("FullPath");
                if (CommonUtils.IsSubpathOf(baseDir, fullPath))
                {
                    pbstrRelativePath = CommonUtils.GetRelativeFilePath(baseDir, fullPath);
                }
            }

            return VSConstants.S_OK;
        }

        public virtual int get_Type(out Guid pguidType)
        {
            pguidType = Guid.Empty;
            throw new NotImplementedException();
        }

        #endregion
    }
}
