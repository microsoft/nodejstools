//*********************************************************//
//    Copyright (c) Microsoft. All rights reserved.
//    
//    Apache 2.0 License
//    
//    You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//    
//    Unless required by applicable law or agreed to in writing, software 
//    distributed under the License is distributed on an "AS IS" BASIS, 
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or 
//    implied. See the License for the specific language governing 
//    permissions and limitations under the License.
//
//*********************************************************//

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
            var projCfg = this._webCfg as IVsProjectCfg;
            if (projCfg != null)
            {
                return projCfg.EnumOutputs(out ppIVsEnumOutputs);
            }
            ppIVsEnumOutputs = null;
            return VSConstants.E_NOTIMPL;
        }

        public int OpenOutput(string szOutputCanonicalName, out IVsOutput ppIVsOutput)
        {
            var projCfg = this._webCfg as IVsProjectCfg;
            if (projCfg != null)
            {
                return projCfg.OpenOutput(szOutputCanonicalName, out ppIVsOutput);
            }
            ppIVsOutput = null;
            return VSConstants.E_NOTIMPL;
        }

        public int get_BuildableProjectCfg(out IVsBuildableProjectCfg ppIVsBuildableProjectCfg)
        {
            var projCfg = this._webCfg as IVsProjectCfg;
            if (projCfg != null)
            {
                return projCfg.get_BuildableProjectCfg(out ppIVsBuildableProjectCfg);
            }
            ppIVsBuildableProjectCfg = null;
            return VSConstants.E_NOTIMPL;
        }

        public int get_CanonicalName(out string pbstrCanonicalName)
        {
            var projCfg = this._webCfg as IVsProjectCfg;
            if (projCfg != null)
            {
                return projCfg.get_CanonicalName(out pbstrCanonicalName);
            }
            pbstrCanonicalName = null;
            return VSConstants.E_NOTIMPL;
        }

        public int get_IsPackaged(out int pfIsPackaged)
        {
            var projCfg = this._webCfg as IVsProjectCfg;
            if (projCfg != null)
            {
                return projCfg.get_IsPackaged(out pfIsPackaged);
            }
            pfIsPackaged = 0;
            return VSConstants.E_NOTIMPL;
        }

        public int get_IsSpecifyingOutputSupported(out int pfIsSpecifyingOutputSupported)
        {
            var projCfg = this._webCfg as IVsProjectCfg;
            if (projCfg != null)
            {
                return projCfg.get_IsSpecifyingOutputSupported(out pfIsSpecifyingOutputSupported);
            }
            pfIsSpecifyingOutputSupported = 0;
            return VSConstants.E_NOTIMPL;
        }

        public int get_Platform(out Guid pguidPlatform)
        {
            var projCfg = this._webCfg as IVsProjectCfg;
            if (projCfg != null)
            {
                return projCfg.get_Platform(out pguidPlatform);
            }
            pguidPlatform = Guid.Empty;
            return VSConstants.E_NOTIMPL;
        }

        public int get_ProjectCfgProvider(out IVsProjectCfgProvider ppIVsProjectCfgProvider)
        {
            var projCfg = this._webCfg as IVsProjectCfg;
            if (projCfg != null)
            {
                return projCfg.get_ProjectCfgProvider(out ppIVsProjectCfgProvider);
            }
            ppIVsProjectCfgProvider = null;
            return VSConstants.E_NOTIMPL;
        }

        public int get_RootURL(out string pbstrRootURL)
        {
            var projCfg = this._webCfg as IVsProjectCfg;
            if (projCfg != null)
            {
                return projCfg.get_RootURL(out pbstrRootURL);
            }
            pbstrRootURL = null;
            return VSConstants.E_NOTIMPL;
        }

        public int get_TargetCodePage(out uint puiTargetCodePage)
        {
            var projCfg = this._webCfg as IVsProjectCfg;
            if (projCfg != null)
            {
                return projCfg.get_TargetCodePage(out puiTargetCodePage);
            }
            puiTargetCodePage = 0;
            return VSConstants.E_NOTIMPL;
        }

        public int get_UpdateSequenceNumber(ULARGE_INTEGER[] puliUSN)
        {
            var projCfg = this._webCfg as IVsProjectCfg;
            if (projCfg != null)
            {
                return projCfg.get_UpdateSequenceNumber(puliUSN);
            }
            return VSConstants.E_NOTIMPL;
        }

        #endregion

        #region IVsProjectCfg2 Members

        public int OpenOutputGroup(string szCanonicalName, out IVsOutputGroup ppIVsOutputGroup)
        {
            var projCfg = this._innerNodeCfg as IVsProjectCfg2;
            if (projCfg != null)
            {
                return projCfg.OpenOutputGroup(szCanonicalName, out ppIVsOutputGroup);
            }
            ppIVsOutputGroup = null;
            return VSConstants.E_NOTIMPL;
        }

        public int OutputsRequireAppRoot(out int pfRequiresAppRoot)
        {
            var projCfg = this._innerNodeCfg as IVsProjectCfg2;
            if (projCfg != null)
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
                var pyCfg = this._innerNodeCfg as IVsProjectFlavorCfg;
                if (pyCfg != null)
                {
                    return pyCfg.get_CfgType(ref iidCfg, out ppCfg);
                }
            }

            var projCfg = this._webCfg as IVsProjectFlavorCfg;
            if (projCfg != null)
            {
                return projCfg.get_CfgType(ref iidCfg, out ppCfg);
            }
            ppCfg = IntPtr.Zero;
            return VSConstants.E_NOTIMPL;
        }

        public int get_IsPrivate(out int pfPrivate)
        {
            var projCfg = this._innerNodeCfg as IVsProjectCfg2;
            if (projCfg != null)
            {
                return projCfg.get_IsPrivate(out pfPrivate);
            }
            pfPrivate = 0;
            return VSConstants.E_NOTIMPL;
        }

        public int get_OutputGroups(uint celt, IVsOutputGroup[] rgpcfg, uint[] pcActual = null)
        {
            var projCfg = this._innerNodeCfg as IVsProjectCfg2;
            if (projCfg != null)
            {
                return projCfg.get_OutputGroups(celt, rgpcfg, pcActual);
            }
            return VSConstants.E_NOTIMPL;
        }

        public int get_VirtualRoot(out string pbstrVRoot)
        {
            var projCfg = this._innerNodeCfg as IVsProjectCfg2;
            if (projCfg != null)
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
            var cfg = this._webCfg as IVsProjectFlavorCfg;
            if (cfg != null)
            {
                return cfg.Close();
            }
            return VSConstants.S_OK;
        }

        #endregion

        #region IVsDebuggableProjectCfg Members

        public int DebugLaunch(uint grfLaunch)
        {
            var cfg = this._innerNodeCfg as IVsDebuggableProjectCfg;
            if (cfg != null)
            {
                return cfg.DebugLaunch(grfLaunch);
            }
            return VSConstants.E_NOTIMPL;
        }

        public int QueryDebugLaunch(uint grfLaunch, out int pfCanLaunch)
        {
            var cfg = this._innerNodeCfg as IVsDebuggableProjectCfg;
            if (cfg != null)
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
            var cfg = this._innerNodeCfg as ISpecifyPropertyPages;
            if (cfg != null)
            {
                cfg.GetPages(pPages);
            }
        }

        #endregion

        #region IVsSpecifyProjectDesignerPages Members

        public int GetProjectDesignerPages(CAUUID[] pPages)
        {
            var cfg = this._innerNodeCfg as IVsSpecifyProjectDesignerPages;
            if (cfg != null)
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
            var cfg = this._innerNodeCfg as IVsCfgBrowseObject;
            if (cfg != null)
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
