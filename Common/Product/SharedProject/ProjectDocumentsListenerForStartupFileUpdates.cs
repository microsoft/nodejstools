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

using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudioTools.Project
{
    /// <summary>
    /// Updates the dynamic project property called StartupFile 
    /// </summary>
    internal class ProjectDocumentsListenerForStartupFileUpdates : ProjectDocumentsListener
    {
        #region fields
        /// <summary>
        /// The dynamic project who adviced for TrackProjectDocumentsEvents
        /// </summary>
        private CommonProjectNode _project;
        #endregion

        #region ctors
        public ProjectDocumentsListenerForStartupFileUpdates(System.IServiceProvider serviceProvider, CommonProjectNode project)
            : base(serviceProvider)
        {
            this._project = project;
        }
        #endregion

        #region overriden methods
        public override int OnAfterRenameFiles(int cProjects, int cFiles, IVsProject[] projects, int[] firstIndices, string[] oldFileNames, string[] newFileNames, VSRENAMEFILEFLAGS[] flags)
        {
            if (!this._project.IsRefreshing)
            {
                //Get the current value of the StartupFile Property
                var currentStartupFile = this._project.GetProjectProperty(CommonConstants.StartupFile, true);
                var fullPathToStartupFile = CommonUtils.GetAbsoluteFilePath(this._project.ProjectHome, currentStartupFile);

                //Investigate all of the oldFileNames if they are equal to the current StartupFile
                var index = 0;
                foreach (var oldfile in oldFileNames)
                {
                    FileNode node = null;
                    if ((flags[index] & VSRENAMEFILEFLAGS.VSRENAMEFILEFLAGS_Directory) != 0)
                    {
                        if (CommonUtils.IsSubpathOf(oldfile, fullPathToStartupFile))
                        {
                            // Get the newfilename and update the StartupFile property
                            var newfilename = Path.Combine(
                                newFileNames[index],
                                CommonUtils.GetRelativeFilePath(oldfile, fullPathToStartupFile)
                            );

                            node = this._project.FindNodeByFullPath(newfilename) as FileNode;
                            Debug.Assert(node != null);
                        }
                    }
                    else if (CommonUtils.IsSamePath(oldfile, fullPathToStartupFile))
                    {
                        //Get the newfilename and update the StartupFile property
                        var newfilename = newFileNames[index];
                        node = this._project.FindNodeByFullPath(newfilename) as FileNode;
                        Debug.Assert(node != null);
                    }

                    if (node != null)
                    {
                        // Startup file has been renamed
                        this._project.SetProjectProperty(
                            CommonConstants.StartupFile,
                            CommonUtils.GetRelativeFilePath(this._project.ProjectHome, node.Url));
                        break;
                    }
                    index++;
                }
            }
            return VSConstants.S_OK;
        }

        public override int OnAfterRemoveFiles(int cProjects, int cFiles, IVsProject[] projects, int[] firstIndices, string[] oldFileNames, VSREMOVEFILEFLAGS[] flags)
        {
            if (!this._project.IsRefreshing)
            {
                //Get the current value of the StartupFile Property
                var currentStartupFile = this._project.GetProjectProperty(CommonConstants.StartupFile, true);
                var fullPathToStartupFile = CommonUtils.GetAbsoluteFilePath(this._project.ProjectHome, currentStartupFile);

                //Investigate all of the oldFileNames if they are equal to the current StartupFile
                var index = 0;
                foreach (var oldfile in oldFileNames)
                {
                    //Compare the files and update the StartupFile Property if the currentStartupFile is an old file
                    if (CommonUtils.IsSamePath(oldfile, fullPathToStartupFile))
                    {
                        //Startup file has been removed
                        this._project.SetProjectProperty(CommonConstants.StartupFile, null);
                        break;
                    }
                    index++;
                }
            }
            return VSConstants.S_OK;
        }
        #endregion
    }
}
