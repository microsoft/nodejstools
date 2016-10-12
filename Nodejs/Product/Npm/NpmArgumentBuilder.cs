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
using System.Globalization;

namespace Microsoft.NodejsTools.Npm {
    public static class NpmArgumentBuilder {
        public static string GetNpmInstallArguments(string packageName,
            string versionRange,
            DependencyType type,
            bool global = false,
            bool saveToPackageJson = true,
            string otherArguments = "") 
        {
            string dependencyArguments = "";
            if (global) {
                dependencyArguments = "-g";
            } else if (saveToPackageJson) {
                switch(type) {
                    case DependencyType.Standard:
                        dependencyArguments = "--save";
                        break;
                    case DependencyType.Development:
                        dependencyArguments = "--save-dev";
                        break;
                    case DependencyType.Optional:
                        dependencyArguments = "--save-optional";
                        break;
                }
            }

            otherArguments = otherArguments.TrimStart(' ', '\t');
            if (otherArguments.StartsWith("@", StringComparison.Ordinal)) {
                return string.Format(CultureInfo.InvariantCulture, "install {0}{1} {2}", packageName, otherArguments, dependencyArguments);
            } else if (!string.IsNullOrEmpty(versionRange)) {
                return string.Format(CultureInfo.InvariantCulture, "install {0}@\"{1}\" {2} {3}", packageName, versionRange, dependencyArguments, otherArguments);
            }

            return string.Format(CultureInfo.InvariantCulture, "install {0} {1} {2}", packageName, dependencyArguments, otherArguments);
        }
    }
}
