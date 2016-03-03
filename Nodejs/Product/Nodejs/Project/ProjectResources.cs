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
using System.Resources;
using System.Threading;
using CommonSR = Microsoft.VisualStudioTools.Project.SR;

namespace Microsoft.NodejsTools.Project {
    internal class NodeJsProjectSr : CommonSR {
        internal const string NodejsToolsForVisualStudio = "NodejsToolsForVisualStudio";

        internal const string CategoryStatus = "CategoryStatus";
        internal const string CategoryVersion = "CategoryVersion";
        internal const string NodeExeArguments = "NodeExeArguments";
        internal const string NodeExeArgumentsDescription = "NodeExeArgumentsDescription";
        internal const string NodeExePath = "NodeExePath";
        internal const string NodeExePathDescription = "NodeExePathDescription";
        internal const string NodejsPort = "NodejsPort";
        internal const string NodejsPortDescription = "NodejsPortDescription";
        internal const string NpmNodePackageInstallation = "NpmNodePackageInstallation";
        internal const string NpmNodePackageInstallationDescription = "NpmNodePackageInstallationDescription";
        internal const string NpmNodePath = "NpmNodePath";
        internal const string NpmNodePathDescription = "NpmNodePathDescription";
        internal const string NpmPackageAuthor = "NpmPackageAuthor";
        internal const string NpmPackageAuthorDescription = "NpmPackageAuthorDescription";
        internal const string NpmPackageDescription = "NpmPackageDescription";
        internal const string NpmPackageDescriptionDescription = "NpmPackageDescriptionDescription";
        internal const string NpmPackageIsBundledDependency = "NpmPackageIsBundledDependency";
        internal const string NpmPackageIsBundledDependencyDescription = "NpmPackageIsBundledDependencyDescription";
        internal const string NpmPackageIsDevDependency = "NpmPackageIsDevDependency";
        internal const string NpmPackageIsDevDependencyDescription = "NpmPackageIsDevDependencyDescription";
        internal const string NpmPackageIsListedInParentPackageJson = "NpmPackageIsListedInParentPackageJson";
        internal const string NpmPackageIsListedInParentPackageJsonDescription = "NpmPackageIsListedInParentPackageJsonDescription";
        internal const string NpmPackageIsMissing = "NpmPackageIsMissing";
        internal const string NpmPackageIsMissingDescription = "NpmPackageIsMissingDescription";
        internal const string NpmPackageIsOptionalDependency = "NpmPackageIsOptionalDependency";
        internal const string NpmPackageIsOptionalDependencyDescription = "NpmPackageIsOptionalDependencyDescription";
        internal const string NpmPackageKeywords = "NpmPackageKeywords";
        internal const string NpmPackageKeywordsDescription = "NpmPackageKeywordsDescription";
        internal const string NpmPackageLinkStatus = "NpmPackageLinkStatus";
        internal const string NpmPackageLinkStatusDescription = "NpmPackageLinkStatusDescription";
        internal const string NpmPackageName = "NpmPackageName";
        internal const string NpmPackageNameDescription = "NpmPackageNameDescription";
        internal const string NpmPackagePath = "NpmPackagePath";
        internal const string NpmPackagePathDescription = "NpmPackagePathDescription";
        internal const string NpmPackageRequestedVersionRange = "NpmPackageRequestedVersionRange";
        internal const string NpmPackageRequestedVersionRangeDescription = "NpmPackageRequestedVersionRangeDescription";
        internal const string NpmPackageType = "NpmPackageType";
        internal const string NpmPackageTypeDescription = "NpmPackageTypeDescription";
        internal const string NpmPackageVersion = "NpmPackageVersion";
        internal const string NpmPackageVersionDescription = "NpmPackageVersionDescription";
        internal const string TestFramework = "TestFramework";
        internal const string TestFrameworkDescription = "TestFrameworkDescription";

        private static readonly Lazy<ResourceManager> _manager = new Lazy<ResourceManager>(
            () => new System.Resources.ResourceManager("Microsoft.NodejsTools.Resources", typeof(NodeJsProjectSr).Assembly),
            LazyThreadSafetyMode.ExecutionAndPublication
        );

        private static ResourceManager Manager {
            get {
                return _manager.Value;
            }
        }

        internal static new string GetString(string value, params object[] args) {
            return GetStringInternal(Manager, value, args) ?? CommonSR.GetString(value, args);
        }

        internal static string ProductName {
            get {
                return Resources.NodejsToolsForVisualStudio;
            }
        }
    }
}
