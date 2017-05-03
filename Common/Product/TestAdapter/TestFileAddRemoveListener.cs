// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace Microsoft.VisualStudioTools.TestAdapter
{
    internal enum TestFileChangedReason
    {
        None,
        Added,
        Removed,
        Changed,
        Renamed,
    }

    internal class TestFileChangedEventArgs : EventArgs
    {
        public IVsProject Project { get; private set; }
        public string File { get; private set; }
        public TestFileChangedReason ChangedReason { get; private set; }

        public TestFileChangedEventArgs(IVsProject project, string file, TestFileChangedReason reason)
        {
            Project = project;
            File = file;
            ChangedReason = reason;
        }
    }

    internal sealed class TestFileAddRemoveListener : IVsTrackProjectDocumentsEvents2, IDisposable
    {
        private IVsTrackProjectDocuments2 _projectDocTracker;
        private uint _cookie = VSConstants.VSCOOKIE_NIL;
        private Guid _testProjectGuid;

        /// <summary>
        /// Fires a task when a build completes
        /// </summary>
        public event EventHandler<TestFileChangedEventArgs> TestFileChanged;

        public TestFileAddRemoveListener(IServiceProvider serviceProvider, Guid projectGuid)
        {
            ValidateArg.NotNull(serviceProvider, "serviceProvider");

            _testProjectGuid = projectGuid;

            _projectDocTracker = serviceProvider.GetService<IVsTrackProjectDocuments2>(typeof(SVsTrackProjectDocuments));
        }

        public void StartListeningForTestFileChanges()
        {
            if (_projectDocTracker != null)
            {
                var hr = _projectDocTracker.AdviseTrackProjectDocumentsEvents(this, out _cookie);
                ErrorHandler.ThrowOnFailure(hr); // do nothing if this fails
            }
        }

        public void StopListeningForTestFileChanges()
        {
            if (_cookie != VSConstants.VSCOOKIE_NIL && _projectDocTracker != null)
            {
                var hr = _projectDocTracker.UnadviseTrackProjectDocumentsEvents(_cookie);
                ErrorHandler.Succeeded(hr); // do nothing if this fails

                _cookie = VSConstants.VSCOOKIE_NIL;
            }
        }

        private int NotifyTestFileAddRemove(int changedProjectCount, IVsProject[] changedProjects, string[] changedProjectItems, int[] rgFirstIndices, TestFileChangedReason reason)
        {
            for (var index = 0; index < changedProjectCount; index++)
            {
                var projectItem = changedProjectItems[index];
                var projectIndex = rgFirstIndices[index];
                var project = changedProjects[projectIndex];

                if (project != null && project.IsTestProject(_testProjectGuid))
                {
                    var evt = TestFileChanged;
                    if (evt != null)
                    {
                        evt(this, new TestFileChangedEventArgs(project, projectItem, reason));
                    }
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
            return NotifyTestFileAddRemove(cProjects, rgpProjects, rgpszMkDocuments, rgFirstIndices, TestFileChangedReason.Added);
        }

        int IVsTrackProjectDocumentsEvents2.OnAfterRemoveDirectories(int cProjects, int cDirectories, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, VSREMOVEDIRECTORYFLAGS[] rgFlags)
        {
            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnAfterRemoveFiles(int cProjects, int cFiles, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, VSREMOVEFILEFLAGS[] rgFlags)
        {
            return NotifyTestFileAddRemove(cProjects, rgpProjects, rgpszMkDocuments, rgFirstIndices, TestFileChangedReason.Removed);
        }

        int IVsTrackProjectDocumentsEvents2.OnAfterRenameDirectories(int cProjects, int cDirs, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgszMkOldNames, string[] rgszMkNewNames, VSRENAMEDIRECTORYFLAGS[] rgFlags)
        {
            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnAfterRenameFiles(int cProjects, int cFiles, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgszMkOldNames, string[] rgszMkNewNames, VSRENAMEFILEFLAGS[] rgFlags)
        {
            NotifyTestFileAddRemove(cProjects, rgpProjects, rgszMkOldNames, rgFirstIndices, TestFileChangedReason.Removed);
            return NotifyTestFileAddRemove(cProjects, rgpProjects, rgszMkNewNames, rgFirstIndices, TestFileChangedReason.Added);
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
