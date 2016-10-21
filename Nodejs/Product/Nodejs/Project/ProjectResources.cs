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
    internal class SR : CommonSR {
        internal const string NodejsToolsForVisualStudio = "NodejsToolsForVisualStudio";

        internal const string AzureRemoteDebugCouldNotAttachToWebsiteErrorMessage = "AzureRemoveDebugCouldNotAttachToWebsiteErrorMessage";
        internal const string AzureRemoteDebugWaitCaption = "AzureRemoteDebugWaitCaption";
        internal const string AzureRemoteDebugWaitMessage = "AzureRemoteDebugWaitMessage";
        internal const string AzureRemoveDebugCouldNotAttachToWebsiteExceptionErrorMessage = "AzureRemoveDebugCouldNotAttachToWebsiteExceptionErrorMessage";
        internal const string AzureToolsInstallInstructions = "AzureToolsInstallInstructions";
        internal const string AzureToolsRequired = "AzureToolsRequired";
        internal const string AzureToolsUpgradeInstructions = "AzureToolsUpgradeInstructions";
        internal const string AzureToolsUpgradeRecommended = "AzureToolsUpgradeRecommended";
        internal const string CacheDirectoryClearFailedCaption = "CacheDirectoryClearFailedCaption";
        internal const string CacheDirectoryClearFailedTitle = "CacheDirectoryClearFailedTitle";
        internal const string CatalogLoadingDefault = "CatalogLoadingDefault";
        internal const string CategoryStatus = "CategoryStatus";
        internal const string CategoryVersion = "CategoryVersion";
        internal const string ContinueWithoutAzureToolsUpgrade = "ContinueWithoutAzureToolsUpgrade";
        internal const string DebugCouldNotResolveStartupFileErrorMessage = "DebugCouldNotResolveStartupFileErrorMessage";
        internal const string DebugWorkingDirectoryDoesNotExistErrorMessage = "DebugWorkingDirectoryDoesNotExistErrorMessage";
        internal const string DebugInterpreterDoesNotExistErrorMessage = "DebugInterpreterDoesNotExistErrorMessage";
        internal const string DebugTypeScriptCombineNotSupportedWarningMessage = "DebugTypeScriptCombineNotSupportedWarningMessage";
        internal const string DebuggerConnectionClosed = "DebuggerConnectionClosed";
        internal const string DebuggerModuleUpdateFailed = "DebuggerModuleUpdateFailed";
        internal const string DebuggerPort = "DebuggerPort";
        internal const string DontShowAgain = "DontShowAgain";
        internal const string DownloadAndInstall = "DownloadAndInstall";
        internal const string EnvironmentVariables = "EnvironmentVariables";
        internal const string ErrorNoDte = "ErrorNoDte";
        internal const string ImportingProjectAccessErrorStatusText = "ImportingProjectAccessErrorStatusText";
        internal const string ImportingProjectErrorStatusText = "ImportingProjectErrorStatusText";
        internal const string ImportingProjectStatusText = "ImportingProjectStatusText";
        internal const string ImportingProjectUnexpectedErrorMessage = "ImportingProjectUnexpectedErrorMessage";
        internal const string ImportWizzardCouldNotStartNotAutomationObjectErrorMessage = "ImportWizzardCouldNotStartNotAutomationObjectErrorMessage";
        internal const string IncludeNodeModulesCancelTitle = "IncludeNodeModulesCancelTitle";
        internal const string IncludeNodeModulesContent = "IncludeNodeModulesContent";
        internal const string IncludeNodeModulesIncludeDescription = "IncludeNodeModulesIncludeDescription";
        internal const string IncludeNodeModulesIncludeTitle = "IncludeNodeModulesIncludeTitle";
        internal const string IncludeNodeModulesInformation = "IncludeNodeModulesInformation";
        internal const string InteractiveWindowFailedToStartProcessErrorMessage = "InteractiveWindowFailedToStartProcessErrorMessage";
        internal const string InteractiveWindowNoProcessErrorMessage = "InteractiveWindowNoProcessErrorMessage";
        internal const string InteractiveWindowProcessExitedMessage = "InteractiveWindowProcessExitedMessage";
        internal const string InteractiveWindowTitle = "InteractiveWindowTitle";
        internal const string NoKeywordsInPackage = "NoKeywordsInPackage";
        internal const string NodeExeArguments = "NodeExeArguments";
        internal const string NodeExeArgumentsDescription = "NodeExeArgumentsDescription";
        internal const string NodeExeArgumentsToolTip = "NodeExeArgumentsToolTip";
        internal const string NodeExeDoesntExist = "NodeExeDoesntExist";
        internal const string NodeExePath = "NodeExePath";
        internal const string NodeExePathDescription = "NodeExePathDescription";
        internal const string NodeExePathNotFound = "NodeExePathNotFound";
        internal const string NodejsNotInstalled = "NodejsNotInstalled";
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
        internal const string NpmPackageInstallHelpMessage = "NpmPackageInstallHelpMessage";
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
            () => new System.Resources.ResourceManager("Microsoft.NodejsTools.Resources", typeof(SR).Assembly),
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
                return GetString(NodejsToolsForVisualStudio);
            }
        }
    }
}
