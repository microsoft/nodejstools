// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.NodejsTools.Project
{
    /// <summary>
    /// Merges the NTVS IVsCfg object with the Venus IVsCfg implementation redirecting
    /// things appropriately to either one.
    /// </summary>
    internal class NodejsProjectConfig :
        IVsCfg,
        IVsProjectCfg,
        IVsProjectCfg2,
        IVsProjectFlavorCfg,
        IVsDebuggableProjectCfg,
        ISpecifyPropertyPages,
        IVsSpecifyProjectDesignerPages,
        IVsCfgBrowseObject
    {
        private readonly IVsCfg _innerNodeCfg;
        private readonly IVsProjectFlavorCfg _webCfg;

        public NodejsProjectConfig(IVsCfg pythonCfg, IVsProjectFlavorCfg webConfig)
        {
            this._innerNodeCfg = pythonCfg;
            this._webCfg = webConfig;
        }

        #region IVsCfg Members

        public int get_DisplayName(out string pbstrDisplayName)
        {
            return this._innerNodeCfg.get_DisplayName(out pbstrDisplayName);
        }

        public int get_IsDebugOnly(out int pfIsDebugOnly)
        {
            return this._innerNodeCfg.get_IsDebugOnly(out pfIsDebugOnly);
        }

        public int get_IsReleaseOnly(out int pfIsReleaseOnly)
        {
            return this._innerNodeCfg.get_IsReleaseOnly(out pfIsReleaseOnly);
        }

        #endregion

        #region IVsProjectCfg Members

        public int EnumOutputs(out IVsEnumOutputs ppIVsEnumOutputs)
        {
            if (this._webCfg is IVsProjectCfg projCfg)
            {
                return projCfg.EnumOutputs(out ppIVsEnumOutputs);
            }
            ppIVsEnumOutputs = null;
            return VSConstants.E_NOTIMPL;
        }

        public int OpenOutput(string szOutputCanonicalName, out IVsOutput ppIVsOutput)
        {
            if (this._webCfg is IVsProjectCfg projCfg)
            {
                return projCfg.OpenOutput(szOutputCanonicalName, out ppIVsOutput);
            }
            ppIVsOutput = null;
            return VSConstants.E_NOTIMPL;
        }

        public int get_BuildableProjectCfg(out IVsBuildableProjectCfg ppIVsBuildableProjectCfg)
        {
            if (this._webCfg is IVsProjectCfg projCfg)
            {
                return projCfg.get_BuildableProjectCfg(out ppIVsBuildableProjectCfg);
            }
            ppIVsBuildableProjectCfg = null;
            return VSConstants.E_NOTIMPL;
        }

        public int get_CanonicalName(out string pbstrCanonicalName)
        {
            if (this._webCfg is IVsProjectCfg projCfg)
            {
                return projCfg.get_CanonicalName(out pbstrCanonicalName);
            }
            pbstrCanonicalName = null;
            return VSConstants.E_NOTIMPL;
        }

        public int get_IsPackaged(out int pfIsPackaged)
        {
            if (this._webCfg is IVsProjectCfg projCfg)
            {
                return projCfg.get_IsPackaged(out pfIsPackaged);
            }
            pfIsPackaged = 0;
            return VSConstants.E_NOTIMPL;
        }

        public int get_IsSpecifyingOutputSupported(out int pfIsSpecifyingOutputSupported)
        {
            if (this._webCfg is IVsProjectCfg projCfg)
            {
                return projCfg.get_IsSpecifyingOutputSupported(out pfIsSpecifyingOutputSupported);
            }
            pfIsSpecifyingOutputSupported = 0;
            return VSConstants.E_NOTIMPL;
        }

        public int get_Platform(out Guid pguidPlatform)
        {
            if (this._webCfg is IVsProjectCfg projCfg)
            {
                return projCfg.get_Platform(out pguidPlatform);
            }
            pguidPlatform = Guid.Empty;
            return VSConstants.E_NOTIMPL;
        }

        public int get_ProjectCfgProvider(out IVsProjectCfgProvider ppIVsProjectCfgProvider)
        {
            if (this._webCfg is IVsProjectCfg projCfg)
            {
                return projCfg.get_ProjectCfgProvider(out ppIVsProjectCfgProvider);
            }
            ppIVsProjectCfgProvider = null;
            return VSConstants.E_NOTIMPL;
        }

        public int get_RootURL(out string pbstrRootURL)
        {
            if (this._webCfg is IVsProjectCfg projCfg)
            {
                return projCfg.get_RootURL(out pbstrRootURL);
            }
            pbstrRootURL = null;
            return VSConstants.E_NOTIMPL;
        }

        public int get_TargetCodePage(out uint puiTargetCodePage)
        {
            if (this._webCfg is IVsProjectCfg projCfg)
            {
                return projCfg.get_TargetCodePage(out puiTargetCodePage);
            }
            puiTargetCodePage = 0;
            return VSConstants.E_NOTIMPL;
        }

        public int get_UpdateSequenceNumber(ULARGE_INTEGER[] puliUSN)
        {
            if (this._webCfg is IVsProjectCfg projCfg)
            {
                return projCfg.get_UpdateSequenceNumber(puliUSN);
            }
            return VSConstants.E_NOTIMPL;
        }

        #endregion

        #region IVsProjectCfg2 Members

        public int OpenOutputGroup(string szCanonicalName, out IVsOutputGroup ppIVsOutputGroup)
        {
            if (this._innerNodeCfg is IVsProjectCfg2 projCfg)
            {
                return projCfg.OpenOutputGroup(szCanonicalName, out ppIVsOutputGroup);
            }
            ppIVsOutputGroup = null;
            return VSConstants.E_NOTIMPL;
        }

        public int OutputsRequireAppRoot(out int pfRequiresAppRoot)
        {
            if (this._innerNodeCfg is IVsProjectCfg2 projCfg)
            {
                return projCfg.OutputsRequireAppRoot(out pfRequiresAppRoot);
            }
            pfRequiresAppRoot = 1;
            return VSConstants.E_NOTIMPL;
        }

        public int get_CfgType(ref Guid iidCfg, out IntPtr ppCfg)
        {
            if (iidCfg == typeof(IVsDebuggableProjectCfg).GUID)
            {
                if (this._innerNodeCfg is IVsProjectFlavorCfg pyCfg)
                {
                    return pyCfg.get_CfgType(ref iidCfg, out ppCfg);
                }
            }

            if (this._webCfg is IVsProjectFlavorCfg projCfg)
            {
                return projCfg.get_CfgType(ref iidCfg, out ppCfg);
            }
            ppCfg = IntPtr.Zero;
            return VSConstants.E_NOTIMPL;
        }

        public int get_IsPrivate(out int pfPrivate)
        {
            if (this._innerNodeCfg is IVsProjectCfg2 projCfg)
            {
                return projCfg.get_IsPrivate(out pfPrivate);
            }
            pfPrivate = 0;
            return VSConstants.E_NOTIMPL;
        }

        public int get_OutputGroups(uint celt, IVsOutputGroup[] rgpcfg, uint[] pcActual = null)
        {
            if (this._innerNodeCfg is IVsProjectCfg2 projCfg)
            {
                return projCfg.get_OutputGroups(celt, rgpcfg, pcActual);
            }
            return VSConstants.E_NOTIMPL;
        }

        public int get_VirtualRoot(out string pbstrVRoot)
        {
            if (this._innerNodeCfg is IVsProjectCfg2 projCfg)
            {
                return projCfg.get_VirtualRoot(out pbstrVRoot);
            }
            pbstrVRoot = null;
            return VSConstants.E_NOTIMPL;
        }

        #endregion

        #region IVsProjectFlavorCfg Members

        public int Close()
        {
            if (this._webCfg is IVsProjectFlavorCfg cfg)
            {
                return cfg.Close();
            }
            return VSConstants.S_OK;
        }

        #endregion

        #region IVsDebuggableProjectCfg Members

        public int DebugLaunch(uint grfLaunch)
        {
            if (this._innerNodeCfg is IVsDebuggableProjectCfg cfg)
            {
                return cfg.DebugLaunch(grfLaunch);
            }
            return VSConstants.E_NOTIMPL;
        }

        public int QueryDebugLaunch(uint grfLaunch, out int pfCanLaunch)
        {
            if (this._innerNodeCfg is IVsDebuggableProjectCfg cfg)
            {
                return cfg.QueryDebugLaunch(grfLaunch, out pfCanLaunch);
            }
            pfCanLaunch = 0;
            return VSConstants.E_NOTIMPL;
        }

        #endregion

        #region ISpecifyPropertyPages Members

        public void GetPages(CAUUID[] pPages)
        {
            if (this._innerNodeCfg is ISpecifyPropertyPages cfg)
            {
                cfg.GetPages(pPages);
            }
        }

        #endregion

        #region IVsSpecifyProjectDesignerPages Members

        public int GetProjectDesignerPages(CAUUID[] pPages)
        {
            if (this._innerNodeCfg is IVsSpecifyProjectDesignerPages cfg)
            {
                return cfg.GetProjectDesignerPages(pPages);
            }
            return VSConstants.E_NOTIMPL;
        }

        #endregion

        #region IVsCfgBrowseObject Members

        public int GetCfg(out IVsCfg ppCfg)
        {
            ppCfg = this;
            return VSConstants.S_OK;
        }

        public int GetProjectItem(out IVsHierarchy pHier, out uint pItemid)
        {
            if (this._innerNodeCfg is IVsCfgBrowseObject cfg)
            {
                return cfg.GetProjectItem(out pHier, out pItemid);
            }
            pHier = null;
            pItemid = 0;
            return VSConstants.E_NOTIMPL;
        }

        #endregion
    }
}
