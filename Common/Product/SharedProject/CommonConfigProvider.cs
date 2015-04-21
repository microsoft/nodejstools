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

using System.Collections.Generic;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudioTools.Project {
    /// <summary>
    /// Enables the Any CPU, x86, x64, and ARM Platform form names for Dynamic Projects.
    /// Hooks language specific project config.
    /// Projects that are platform aware should have a PlatformAware and AvailablePlatforms
    /// property for configuration handling to function correctly.
    /// PlatformAware value can be true or false. AvailablePlatforms is a comma separated string of supported platforms (e.g. "x86, X64")
    /// If the PlatformAware property is ommited then this provider will only provide "Any CPU" platform.
    /// </summary>
    internal class CommonConfigProvider : ConfigProvider {
        private CommonProjectNode _project;
        private bool _isPlatformAware;

        public CommonConfigProvider(CommonProjectNode project)
            : base(project) {
            _project = project;
            bool.TryParse(this.ProjectMgr.BuildProject.GetPropertyValue(ProjectFileConstants.PlatformAware), out _isPlatformAware);
        }

        #region overridden methods

        protected override ProjectConfig CreateProjectConfiguration(string configName) {     
            if (_isPlatformAware) {
                if (configName != null) {
                    var configParts = configName.Split('|');

                    if (configParts.Length == 2) {
                        var config = _project.MakeConfiguration(configName);
                        config.PlatformName = configParts[1];
                        return config;
                    }
                }
            }

            return _project.MakeConfiguration(configName);
        }

        public override int GetPlatformNames(uint celt, string[] names, uint[] actual) {
            if (_isPlatformAware) {
                var platforms = GetSupportedPlatformsFromProject();
                return GetPlatforms(celt, names, actual, platforms);
            }
            else {
                if (names != null) {
                    names[0] = "Any CPU";
                }

                if (actual != null) {
                    actual[0] = 1;
                }

                return VSConstants.S_OK;
            }
        }

        public override int GetSupportedPlatformNames(uint celt, string[] names, uint[] actual) {
            if (_isPlatformAware) {
                var platforms = GetSupportedPlatformsFromProject();
                return GetPlatforms(celt, names, actual, platforms);
            }
            else {
                if (names != null) {
                    names[0] = "Any CPU";
                }

                if (actual != null) {
                    actual[0] = 1;
                }

                return VSConstants.S_OK;
            }
        }

        public override int GetCfgs(uint celt, IVsCfg[] a, uint[] actual, uint[] flags) {
            if (_isPlatformAware) {
                if (flags != null) {
                    flags[0] = 0;
                }

                int i = 0;
                string[] configList = GetPropertiesConditionedOn(ProjectFileConstants.Configuration);
                string[] platformList = GetSupportedPlatformsFromProject();

                if (a != null) {
                    foreach (string platformName in platformList) {
                        foreach (string configName in configList) {
                            a[i] = this.GetProjectConfiguration(configName + "|" + platformName);
                            i++;
                            if (i == celt) {
                                break;
                            }
                        }
                        if(i == celt) {
                            break;
                        }
                    }
                }
                else {
                    i = configList.Length * platformList.Length;
                }

                if (actual != null) {
                    actual[0] = (uint)i;
                }

                return VSConstants.S_OK;
            }

            return base.GetCfgs(celt, a, actual, flags);
        }

        public override int GetCfgOfName(string name, string platName, out IVsCfg cfg) {
            if (!string.IsNullOrEmpty(platName)) {
                cfg = this.GetProjectConfiguration(name + "|" + platName);

                return VSConstants.S_OK;
            }
            cfg = this.GetProjectConfiguration(name);

            return VSConstants.S_OK;
        }
        #endregion
    }
}
