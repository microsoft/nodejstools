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
using System.Collections;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;

namespace Microsoft.VisualStudioTools.Project {
    /// <summary>
    /// Base class for property pages based on a WinForm control.
    /// </summary>
    public abstract class CommonPropertyPage : IPropertyPage {
        private IPropertyPageSite _site;
        private bool _dirty, _loading;
        private CommonProjectNode _project;

        public abstract Control Control {
            get;
        }

        public abstract void Apply();
        public abstract void LoadSettings();

        public abstract string Name {
            get;
        }

        internal virtual CommonProjectNode Project {
            get {
                return _project;
            }
            set {
                _project = value;
            }
        }

        protected void SetProjectProperty(string propertyName, string propertyValue) {
            // SetProjectProperty's implementation will check whether the value
            // has changed.
            Project.SetProjectProperty(propertyName, propertyValue);
        }

        protected string GetProjectProperty(string propertyName) {
            return Project.GetUnevaluatedProperty(propertyName);
        }

        public bool Loading {
            get {
                return _loading;
            }
            set {
                _loading = value;
            }
        }

        public bool IsDirty {
            get {
                return _dirty;
            }
            set {
                if (_dirty != value && !Loading) {
                    _dirty = value;
                    if (_site != null) {
                        _site.OnStatusChange((uint)(_dirty ? PropPageStatus.Dirty : PropPageStatus.Clean));
                    }
                }
            }
        }

        void IPropertyPage.Activate(IntPtr hWndParent, RECT[] pRect, int bModal) {
            NativeMethods.SetParent(Control.Handle, hWndParent);
        }

        int IPropertyPage.Apply() {
            try {
                Apply();
                return VSConstants.S_OK;
            } catch (Exception e) {
                return Marshal.GetHRForException(e);
            }
        }

        void IPropertyPage.Deactivate() {
            Project = null;
            Control.Dispose();
        }

        void IPropertyPage.GetPageInfo(PROPPAGEINFO[] pPageInfo) {
            Utilities.ArgumentNotNull("pPageInfo", pPageInfo);

            PROPPAGEINFO info = new PROPPAGEINFO();

            info.cb = (uint)Marshal.SizeOf(typeof(PROPPAGEINFO));
            info.dwHelpContext = 0;
            info.pszDocString = null;
            info.pszHelpFile = null;
            info.pszTitle = Name;
            info.SIZE.cx = Control.Width;
            info.SIZE.cy = Control.Height;
            pPageInfo[0] = info;
        }

        void IPropertyPage.Help(string pszHelpDir) {
        }

        int IPropertyPage.IsPageDirty() {
            return (IsDirty ? (int)VSConstants.S_OK : (int)VSConstants.S_FALSE);
        }

        void IPropertyPage.Move(RECT[] pRect) {
            Utilities.ArgumentNotNull("pRect", pRect);

            RECT r = pRect[0];

            Control.Location = new Point(r.left, r.top);
            Control.Size = new Size(r.right - r.left, r.bottom - r.top);
        }

        void IPropertyPage.SetObjects(uint count, object[] punk) {
            if (punk == null) {
                return;
            }

            if (count > 0) {
                if (punk[0] is ProjectConfig) {
                    ArrayList configs = new ArrayList();

                    for (int i = 0; i < count; i++) {
                        CommonProjectConfig config = (CommonProjectConfig)punk[i];

                        if (_project == null) {
                            Project = (CommonProjectNode)config.ProjectMgr;
                            break;
                        }

                        configs.Add(config);
                    }
                } else if (punk[0] is NodeProperties) {
                    if (_project == null) {
                        Project = (CommonProjectNode)(punk[0] as NodeProperties).HierarchyNode.ProjectMgr;
                    }
                }
            } else {
                Project = null;
            }

            if (_project != null) {
                LoadSettings();
            }
        }

        void IPropertyPage.SetPageSite(IPropertyPageSite pPageSite) {
            _site = pPageSite;
        }

        void IPropertyPage.Show(uint nCmdShow) {
            Control.Visible = true; // TODO: pass SW_SHOW* flags through      
            Control.Show();
        }

        int IPropertyPage.TranslateAccelerator(MSG[] pMsg) {
            Utilities.ArgumentNotNull("pMsg", pMsg);

            MSG msg = pMsg[0];

            if ((msg.message < NativeMethods.WM_KEYFIRST || msg.message > NativeMethods.WM_KEYLAST) && (msg.message < NativeMethods.WM_MOUSEFIRST || msg.message > NativeMethods.WM_MOUSELAST)) {
                return VSConstants.S_FALSE;
            }

            return (NativeMethods.IsDialogMessageA(Control.Handle, ref msg)) ? VSConstants.S_OK : VSConstants.S_FALSE;
        }
    }
}
