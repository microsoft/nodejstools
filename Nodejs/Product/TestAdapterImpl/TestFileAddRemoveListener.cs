// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.IO;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace Microsoft.NodejsTools.TestAdapter
{
    internal sealed class TestFileChangedEventArgs : EventArgs
    {
        public readonly IVsProject Project;
        public readonly string File;
        public readonly WatcherChangeTypes ChangedReason;

        public TestFileChangedEventArgs(string file, WatcherChangeTypes reason, IVsProject project = null)
        {
            this.Project = project;
            this.File = file;
            this.ChangedReason = reason;
        }
    }

    internal sealed class TestFileAddRemoveListener : IVsTrackProjectDocumentsEvents2, IDisposable
    {
        private readonly IVsTrackProjectDocuments2 _projectDocTracker;
        private uint _cookie = VSConstants.VSCOOKIE_NIL;
        private Guid _testProjectGuid;

        /// <summary>
        /// Fires a task when a build completes
        /// </summary>
        public event EventHandler<TestFileChangedEventArgs> TestFileChanged;

        public TestFileAddRemoveListener(IServiceProvider serviceProvider, Guid projectGuid)
        {
            ValidateArg.NotNull(serviceProvider, "serviceProvider");

            this._testProjectGuid = projectGuid;

            this._projectDocTracker = serviceProvider.GetService<IVsTrackProjectDocuments2>(typeof(SVsTrackProjectDocuments));
        }

        public void StartListeningForTestFileChanges()
        {
            if (this._projectDocTracker != null)
            {
                var hr = this._projectDocTracker.AdviseTrackProjectDocumentsEvents(this, out this._cookie);
                ErrorHandler.ThrowOnFailure(hr); // do nothing if this fails
            }
        }

        public void StopListeningForTestFileChanges()
        {
            if (this._cookie != VSConstants.VSCOOKIE_NIL && this._projectDocTracker != null)
            {
                var hr = this._projectDocTracker.UnadviseTrackProjectDocumentsEvents(this._cookie);
                ErrorHandler.Succeeded(hr); // do nothing if this fails

                this._cookie = VSConstants.VSCOOKIE_NIL;
            }
        }

        private int NotifyTestFileAddRemove(int changedProjectCount, IVsProject[] changedProjects, string[] changedProjectItems, int[] rgFirstIndices, WatcherChangeTypes reason)
        {
            for (var index = 0; index < changedProjectCount; index++)
            {
                var projectItem = changedProjectItems[index];
                var projectIndex = rgFirstIndices[index];
                var project = changedProjects[projectIndex];

                if (project != null)
                {
                    TestFileChanged?.Invoke(this, new TestFileChangedEventArgs(projectItem, reason, project));
                }
            }

            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnAfterAddDirectoriesEx(int cProjects, int cDirectories, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, VSADDDIRECTORYFLAGS[] rgFlags)
        {
            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnAfterAddFilesEx(int cProjects, int cFiles, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, VSADDFILEFLAGS[] rgFlags)
        {
            return NotifyTestFileAddRemove(cProjects, rgpProjects, rgpszMkDocuments, rgFirstIndices, WatcherChangeTypes.Created);
        }

        int IVsTrackProjectDocumentsEvents2.OnAfterRemoveDirectories(int cProjects, int cDirectories, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, VSREMOVEDIRECTORYFLAGS[] rgFlags)
        {
            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnAfterRemoveFiles(int cProjects, int cFiles, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, VSREMOVEFILEFLAGS[] rgFlags)
        {
            return NotifyTestFileAddRemove(cProjects, rgpProjects, rgpszMkDocuments, rgFirstIndices, WatcherChangeTypes.Deleted);
        }

        int IVsTrackProjectDocumentsEvents2.OnAfterRenameDirectories(int cProjects, int cDirs, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgszMkOldNames, string[] rgszMkNewNames, VSRENAMEDIRECTORYFLAGS[] rgFlags)
        {
            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnAfterRenameFiles(int cProjects, int cFiles, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgszMkOldNames, string[] rgszMkNewNames, VSRENAMEFILEFLAGS[] rgFlags)
        {
            NotifyTestFileAddRemove(cProjects, rgpProjects, rgszMkOldNames, rgFirstIndices, WatcherChangeTypes.Deleted);
            return NotifyTestFileAddRemove(cProjects, rgpProjects, rgszMkNewNames, rgFirstIndices, WatcherChangeTypes.Created);
        }

        int IVsTrackProjectDocumentsEvents2.OnAfterSccStatusChanged(int cProjects, int cFiles, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, uint[] rgdwSccStatus)
        {
            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnQueryAddDirectories(IVsProject pProject, int cDirectories, string[] rgpszMkDocuments, VSQUERYADDDIRECTORYFLAGS[] rgFlags, VSQUERYADDDIRECTORYRESULTS[] pSummaryResult, VSQUERYADDDIRECTORYRESULTS[] rgResults)
        {
            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnQueryAddFiles(IVsProject pProject, int cFiles, string[] rgpszMkDocuments, VSQUERYADDFILEFLAGS[] rgFlags, VSQUERYADDFILERESULTS[] pSummaryResult, VSQUERYADDFILERESULTS[] rgResults)
        {
            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnQueryRemoveDirectories(IVsProject pProject, int cDirectories, string[] rgpszMkDocuments, VSQUERYREMOVEDIRECTORYFLAGS[] rgFlags, VSQUERYREMOVEDIRECTORYRESULTS[] pSummaryResult, VSQUERYREMOVEDIRECTORYRESULTS[] rgResults)
        {
            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnQueryRemoveFiles(IVsProject pProject, int cFiles, string[] rgpszMkDocuments, VSQUERYREMOVEFILEFLAGS[] rgFlags, VSQUERYREMOVEFILERESULTS[] pSummaryResult, VSQUERYREMOVEFILERESULTS[] rgResults)
        {
            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnQueryRenameDirectories(IVsProject pProject, int cDirs, string[] rgszMkOldNames, string[] rgszMkNewNames, VSQUERYRENAMEDIRECTORYFLAGS[] rgFlags, VSQUERYRENAMEDIRECTORYRESULTS[] pSummaryResult, VSQUERYRENAMEDIRECTORYRESULTS[] rgResults)
        {
            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnQueryRenameFiles(IVsProject pProject, int cFiles, string[] rgszMkOldNames, string[] rgszMkNewNames, VSQUERYRENAMEFILEFLAGS[] rgFlags, VSQUERYRENAMEFILERESULTS[] pSummaryResult, VSQUERYRENAMEFILERESULTS[] rgResults)
        {
            return VSConstants.S_OK;
        }

        public void Dispose()
        {
            Dispose(true);
            // Use SupressFinalize in case a subclass
            // of this type implements a finalizer.
            GC.SuppressFinalize(this);
        }

        public void Dispose(bool disposing)
        {
            if (disposing)
            {
                StopListeningForTestFileChanges();
            }
        }
    }
}
