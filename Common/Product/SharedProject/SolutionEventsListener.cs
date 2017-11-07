// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudioTools
{
    internal class ProjectEventArgs : EventArgs
    {
        public readonly IVsProject Project;

        public ProjectEventArgs(IVsProject project)
        {
            this.Project = project;
        }
    }

    internal class SolutionEventsListener : IVsSolutionEvents3, IVsSolutionEvents4, IVsUpdateSolutionEvents2, IVsUpdateSolutionEvents3, IDisposable
    {
        private readonly IVsSolution solution;
        private readonly IVsSolutionBuildManager3 buildManager;
        private uint cookie1 = VSConstants.VSCOOKIE_NIL;
        private uint cookie2 = VSConstants.VSCOOKIE_NIL;
        private uint cookie3 = VSConstants.VSCOOKIE_NIL;

        public uint Cookie2 { get => this.cookie2; set => this.cookie2 = value; }

        public event EventHandler SolutionOpened;
        public event EventHandler SolutionClosed;
        public event EventHandler<ProjectEventArgs> ProjectLoaded;
        public event EventHandler<ProjectEventArgs> ProjectUnloading;
        public event EventHandler<ProjectEventArgs> ProjectClosing;
        public event EventHandler<ProjectEventArgs> ProjectRenamed;
        public event EventHandler BuildCompleted;
        public event EventHandler BuildStarted;
        public event EventHandler ActiveSolutionConfigurationChanged;

        public SolutionEventsListener(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            this.solution = serviceProvider.GetService(typeof(SVsSolution)) as IVsSolution;
            if (this.solution == null)
            {
                throw new InvalidOperationException("Cannot get solution service");
            }
            this.buildManager = serviceProvider.GetService(typeof(SVsSolutionBuildManager)) as IVsSolutionBuildManager3;
        }

        public SolutionEventsListener(IVsSolution service, IVsSolutionBuildManager3 buildManager = null)
        {
            this.solution = service ?? throw new ArgumentNullException(nameof(service));
            this.buildManager = buildManager;
        }

        public void StartListeningForChanges()
        {
            ErrorHandler.ThrowOnFailure(this.solution.AdviseSolutionEvents(this, out this.cookie1));
            if (this.buildManager != null)
            {
                if (this.buildManager is IVsSolutionBuildManager2 bm2)
                {
                    ErrorHandler.ThrowOnFailure(bm2.AdviseUpdateSolutionEvents(this, out this.cookie2));
                }
                ErrorHandler.ThrowOnFailure(this.buildManager.AdviseUpdateSolutionEvents3(this, out this.cookie3));
            }
        }

        public void Dispose()
        {
            // Ignore failures in UnadviseSolutionEvents
            if (this.cookie1 != VSConstants.VSCOOKIE_NIL)
            {
                this.solution.UnadviseSolutionEvents(this.cookie1);
                this.cookie1 = VSConstants.VSCOOKIE_NIL;
            }
            if (this.cookie2 != VSConstants.VSCOOKIE_NIL)
            {
                ((IVsSolutionBuildManager2)this.buildManager).UnadviseUpdateSolutionEvents(this.cookie2);
                this.cookie2 = VSConstants.VSCOOKIE_NIL;
            }
            if (this.cookie3 != VSConstants.VSCOOKIE_NIL)
            {
                this.buildManager.UnadviseUpdateSolutionEvents3(this.cookie3);
                this.cookie3 = VSConstants.VSCOOKIE_NIL;
            }
        }

        int IVsUpdateSolutionEvents2.OnActiveProjectCfgChange(IVsHierarchy pIVsHierarchy) => VSConstants.E_NOTIMPL;

        int IVsUpdateSolutionEvents2.UpdateProjectCfg_Begin(IVsHierarchy pHierProj, IVsCfg pCfgProj, IVsCfg pCfgSln, uint dwAction, ref int pfCancel) => VSConstants.E_NOTIMPL;

        int IVsUpdateSolutionEvents2.UpdateProjectCfg_Done(IVsHierarchy pHierProj, IVsCfg pCfgProj, IVsCfg pCfgSln, uint dwAction, int fSuccess, int fCancel) => VSConstants.E_NOTIMPL;

        public int OnActiveProjectCfgChange(IVsHierarchy pIVsHierarchy) => VSConstants.E_NOTIMPL;

        public int UpdateSolution_Begin(ref int pfCancelUpdate)
        {
            BuildStarted?.Invoke(this, EventArgs.Empty);
            return VSConstants.S_OK;
        }

        public int UpdateSolution_Cancel() => VSConstants.E_NOTIMPL;

        public int UpdateSolution_Done(int fSucceeded, int fModified, int fCancelCommand)
        {
            BuildCompleted?.Invoke(this, EventArgs.Empty);
            return VSConstants.S_OK;
        }

        public int UpdateSolution_StartUpdate(ref int pfCancelUpdate) => VSConstants.E_NOTIMPL;

        int IVsUpdateSolutionEvents3.OnAfterActiveSolutionCfgChange(IVsCfg pOldActiveSlnCfg, IVsCfg pNewActiveSlnCfg)
        {
            ActiveSolutionConfigurationChanged?.Invoke(this, EventArgs.Empty);
            return VSConstants.S_OK;
        }

        int IVsUpdateSolutionEvents3.OnBeforeActiveSolutionCfgChange(IVsCfg pOldActiveSlnCfg, IVsCfg pNewActiveSlnCfg) => VSConstants.E_NOTIMPL;

        public int OnAfterCloseSolution(object pUnkReserved)
        {
            SolutionClosed?.Invoke(this, EventArgs.Empty);
            return VSConstants.S_OK;
        }

        public int OnAfterClosingChildren(IVsHierarchy pHierarchy) => VSConstants.E_NOTIMPL;

        public int OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy) => VSConstants.E_NOTIMPL;

        public int OnAfterMergeSolution(object pUnkReserved) => VSConstants.E_NOTIMPL;

        public int OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
        {
            if (pHierarchy is IVsProject project)
            {
                ProjectLoaded?.Invoke(this, new ProjectEventArgs(project));
            }
            return VSConstants.S_OK;
        }

        public int OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
        {
            SolutionOpened?.Invoke(this, EventArgs.Empty);
            return VSConstants.S_OK;
        }

        public int OnAfterOpeningChildren(IVsHierarchy pHierarchy) => VSConstants.E_NOTIMPL;

        public int OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
        {
            if (pHierarchy is IVsProject project)
            {
                ProjectClosing?.Invoke(this, new ProjectEventArgs(project));
            }
            return VSConstants.S_OK;
        }

        public int OnBeforeCloseSolution(object pUnkReserved) => VSConstants.E_NOTIMPL;

        public int OnBeforeClosingChildren(IVsHierarchy pHierarchy) => VSConstants.E_NOTIMPL;

        public int OnBeforeOpeningChildren(IVsHierarchy pHierarchy) => VSConstants.E_NOTIMPL;

        public int OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
        {
            if (pRealHierarchy is IVsProject project)
            {
                ProjectUnloading?.Invoke(this, new ProjectEventArgs(project));
            }
            return VSConstants.S_OK;
        }

        public int OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel) => VSConstants.E_NOTIMPL;

        public int OnQueryCloseSolution(object pUnkReserved, ref int pfCancel) => VSConstants.E_NOTIMPL;

        public int OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel) => VSConstants.E_NOTIMPL;

        public int OnAfterAsynchOpenProject(IVsHierarchy pHierarchy, int fAdded) => VSConstants.E_NOTIMPL;

        public int OnAfterChangeProjectParent(IVsHierarchy pHierarchy) => VSConstants.E_NOTIMPL;

        public int OnAfterRenameProject(IVsHierarchy pHierarchy)
        {
            if (pHierarchy is IVsProject project)
            {
                ProjectRenamed?.Invoke(this, new ProjectEventArgs(project));
            }
            return VSConstants.S_OK;
        }

        public int OnQueryChangeProjectParent(IVsHierarchy pHierarchy, IVsHierarchy pNewParentHier, ref int pfCancel) => VSConstants.E_NOTIMPL;
    }
}
