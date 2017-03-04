// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudioTools.MockVsTests
{
    internal class MockVsSolutionBuildManager : IVsSolutionBuildManager
    {
        public int AdviseUpdateSolutionEvents(IVsUpdateSolutionEvents pIVsUpdateSolutionEvents, out uint pdwCookie)
        {
            throw new NotImplementedException();
        }

        public int CanCancelUpdateSolutionConfiguration(out int pfCanCancel)
        {
            throw new NotImplementedException();
        }

        public int CancelUpdateSolutionConfiguration()
        {
            throw new NotImplementedException();
        }

        public int DebugLaunch(uint grfLaunch)
        {
            throw new NotImplementedException();
        }

        public int FindActiveProjectCfg(IntPtr pvReserved1, IntPtr pvReserved2, IVsHierarchy pIVsHierarchy_RequestedProject, IVsProjectCfg[] ppIVsProjectCfg_Active = null)
        {
            throw new NotImplementedException();
        }

        public int GetProjectDependencies(IVsHierarchy pHier, uint celt, IVsHierarchy[] rgpHier, uint[] pcActual = null)
        {
            throw new NotImplementedException();
        }

        public int QueryBuildManagerBusy(out int pfBuildManagerBusy)
        {
            pfBuildManagerBusy = 0;
            return VSConstants.S_OK;
        }

        public int QueryDebugLaunch(uint grfLaunch, out int pfCanLaunch)
        {
            throw new NotImplementedException();
        }

        public int StartSimpleUpdateProjectConfiguration(IVsHierarchy pIVsHierarchyToBuild, IVsHierarchy pIVsHierarchyDependent, string pszDependentConfigurationCanonicalName, uint dwFlags, uint dwDefQueryResults, int fSuppressUI)
        {
            throw new NotImplementedException();
        }

        public int StartSimpleUpdateSolutionConfiguration(uint dwFlags, uint dwDefQueryResults, int fSuppressUI)
        {
            throw new NotImplementedException();
        }

        public int UnadviseUpdateSolutionEvents(uint dwCookie)
        {
            throw new NotImplementedException();
        }

        public int UpdateSolutionConfigurationIsActive(out int pfIsActive)
        {
            throw new NotImplementedException();
        }

        public int get_CodePage(out uint puiCodePage)
        {
            throw new NotImplementedException();
        }

        public int get_IsDebug(out int pfIsDebug)
        {
            throw new NotImplementedException();
        }

        public int get_StartupProject(out IVsHierarchy ppHierarchy)
        {
            throw new NotImplementedException();
        }

        public int put_CodePage(uint uiCodePage)
        {
            throw new NotImplementedException();
        }

        public int put_IsDebug(int fIsDebug)
        {
            throw new NotImplementedException();
        }

        public int set_StartupProject(IVsHierarchy pHierarchy)
        {
            throw new NotImplementedException();
        }
    }
}

