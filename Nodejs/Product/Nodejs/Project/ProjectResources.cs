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

        internal const string AzureToolsInstallInstructions = "AzureToolsInstallInstructions";
        internal const string AzureToolsRequired = "AzureToolsRequired";
        internal const string AzureToolsUpgradeInstructions = "AzureToolsUpgradeInstructions";
        internal const string AzureToolsUpgradeRecommended = "AzureToolsUpgradeRecommended";
        internal const string CatalogLoadingDefault = "CatalogLoadingDefault";
        internal const string CatalogLoadingNoNpm = "CatalogLoadingNoNpm";
        internal const string CategoryStatus = "CategoryStatus";
        internal const string CategoryVersion = "CategoryVersion";
        internal const string CacheDirectoryClearFailedTitle = "CacheDirectoryClearFailedTitle";
        internal const string CacheDirectoryClearFailedCaption = "CacheDirectoryClearFailedCaption";
        internal const string ContinueWithoutAzureToolsUpgrade = "ContinueWithoutAzureToolsUpgrade";
        internal const string DebuggerConnectionClosed = "DebuggerConnectionClosed";
        internal const string DebuggerModuleUpdateFailed = "DebuggerModuleUpdateFailed";
        internal const string DebuggerPort = "DebuggerPort";
        internal const string DontShowAgain = "DontShowAgain";
        internal const string DownloadAndInstall = "DownloadAndInstall";
        internal const string EcmaScript5 = "EcmaScript5";
        internal const string EcmaScript6 = "EcmaScript6";
        internal const string EnvironmentVariables = "EnvironmentVariables";
        internal const string ErrorNoDte = "ErrorNoDte";
        internal const string IncludeNodeModulesCancelTitle = "IncludeNodeModulesCancelTitle";
        internal const string IncludeNodeModulesContent = "IncludeNodeModulesContent";
        internal const string IncludeNodeModulesIncludeDescription = "IncludeNodeModulesIncludeDescription";
        internal const string IncludeNodeModulesIncludeTitle = "IncludeNodeModulesIncludeTitle";
        internal const string IncludeNodeModulesInformation = "IncludeNodeModulesInformation";
        internal const string InsertSnippet = "InsertSnippet";
        internal const string InvalidPortNumber = "InvalidPortNumber";
        internal const string LaunchUrlToolTip = "LaunchUrlToolTip";
        internal const string LinkStatusLinkedToProject = "LinkStatusLinkedToProject";
        internal const string LinkStatusLocallyInstalled = "LinkStatusLocallyInstalled";
        internal const string LinkStatusNotApplicableSubPackages = "LinkStatusNotApplicableSubPackages";
        internal const string LinkStatusNotLinkedToProject = "LinkStatusNotLinkedToProject";
        internal const string LinkStatusUnknown = "LinkStatusUnknown";
        internal const string LongPathClickToCopy = "LongPathClickToCopy";
        internal const string LongPathDoNothingAndDoNotWarnAgain = "LongPathDoNothingAndDoNotWarnAgain";
        internal const string LongPathDoNothingAndDoNotWarnAgainDetail = "LongPathDoNothingAndDoNotWarnAgainDetail";
        internal const string LongPathDoNothingButWarnNextTime = "LongPathDoNothingButWarnNextTime";
        internal const string LongPathFooter = "LongPathFooter";
        internal const string LongPathHidePathsExceedingTheLimit = "LongPathHidePathsExceedingTheLimit";
        internal const string LongPathNpmDedupe = "LongPathNpmDedupe";
        internal const string LongPathNpmDedupeDetail = "LongPathNpmDedupeDetail";
        internal const string LongPathNpmDedupeDidNotHelp = "LongPathNpmDedupeDidNotHelp";
        internal const string LongPathShowPathsExceedingTheLimit = "LongPathShowPathsExceedingTheLimit";
        internal const string LongPathWarningText = "LongPathWarningText";
        internal const string LongPathWarningTitle = "LongPathWarningTitle";
        internal const string Milliseconds = "Milliseconds";
        internal const string NewVersionNo = "NewVersionNo";
        internal const string NewVersionNotApplicableSubpackage = "NewVersionNotApplicableSubpackage";
        internal const string NewVersionPackageCatalogNotRetrieved = "NewVersionPackageCatalogNotRetrieved";
        internal const string NewVersionUnknown = "NewVersionUnknown";
        internal const string NewVersionYes = "NewVersionYes";
        internal const string NoKeywordsInPackage = "NoKeywordsInPackage";
        internal const string NodeExeArguments = "NodeExeArguments";
        internal const string NodeExeArgumentsDescription = "NodeExeArgumentsDescription";
        internal const string NodeExeArgumentsToolTip = "NodeExeArgumentsToolTip";
        internal const string NodeExeDoesntExist = "NodeExeDoesntExist";
        internal const string NodeExePath = "NodeExePath";
        internal const string NodeExePathDescription = "NodeExePathDescription";
        internal const string NodeExePathNotFound = "NodeExePathNotFound";
        internal const string NodeExePathToolTip = "NodeExePathToolTip";
        internal const string NodejsNotInstalled = "NodejsNotInstalled";
        internal const string NodejsNotInstalledShort = "NodejsNotInstalledShort";
        internal const string NodejsNotSupported = "NodejsNotSupported";
        internal const string NodejsPort = "NodejsPort";
        internal const string NodejsPortDescription = "NodejsPortDescription";
        internal const string NodejsPortToolTip = "NodejsPortToolTip";
        internal const string NpmCancelled = "NpmCancelled";
        internal const string NpmCancelledWithErrors = "NpmCancelledWithErrors";
        internal const string NpmCompletedWithErrors = "NpmCompletedWithErrors";
        internal const string NpmNodePackageInstallation = "NpmNodePackageInstallation";
        internal const string NpmNodePackageInstallationDescription = "NpmNodePackageInstallationDescription";
        internal const string NpmNodePath = "NpmNodePath";
        internal const string NpmNodePathDescription = "NpmNodePathDescription";
        internal const string NpmOutputPaneTitle = "NpmOutputPaneTitle";
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
        internal const string NpmPackageNewVersionAvailable = "NpmPackageNewVersionAvailable";
        internal const string NpmPackageNewVersionAvailableDescription = "NpmPackageNewVersionAvailableDescription";
        internal const string NpmPackagePath = "NpmPackagePath";
        internal const string NpmPackagePathDescription = "NpmPackagePathDescription";
        internal const string NpmPackageRequestedVersionRange = "NpmPackageRequestedVersionRange";
        internal const string NpmPackageRequestedVersionRangeDescription = "NpmPackageRequestedVersionRangeDescription";
        internal const string NpmPackageType = "NpmPackageType";
        internal const string NpmPackageTypeDescription = "NpmPackageTypeDescription";
        internal const string NpmPackageVersion = "NpmPackageVersion";
        internal const string NpmPackageVersionDescription = "NpmPackageVersionDescription";
        internal const string NpmReplCommandCompletedWithErrors = "NpmReplCommandCompletedWithErrors";
        internal const string NpmStatusExecuting = "NpmStatusExecuting";
        internal const string NpmStatusExecutingErrors = "NpmStatusExecutingErrors";
        internal const string NpmStatusExecutingQueued = "NpmStatusExecutingQueued";
        internal const string NpmStatusExecutingQueuedErrors = "NpmStatusExecutingQueuedErrors";
        internal const string NpmStatusReady = "NpmStatusReady";
        internal const string NpmStatusReadyWithErrors = "NpmStatusReadyWithErrors";
        internal const string NpmSuccessfullyCompleted = "NpmSuccessfullyCompleted";
        internal const string PackageCatalogRefresh0Days = "PackageCatalogRefresh0Days";
        internal const string PackageCatalogRefresh1Day = "PackageCatalogRefresh1Day";
        internal const string PackageCatalogRefresh1Month = "PackageCatalogRefresh1Month";
        internal const string PackageCatalogRefresh1Week = "PackageCatalogRefresh1Week";
        internal const string PackageCatalogRefresh2To7Days = "PackageCatalogRefresh2To7Days";
        internal const string PackageCatalogRefresh2Weeks = "PackageCatalogRefresh2Weeks";
        internal const string PackageCatalogRefresh3Months = "PackageCatalogRefresh3Months";
        internal const string PackageCatalogRefresh3Weeks = "PackageCatalogRefresh3Weeks";
        internal const string PackageCatalogRefresh6Months = "PackageCatalogRefresh6Months";
        internal const string PackageCatalogRefreshFailed = "PackageCatalogRefreshFailed";
        internal const string PackageCatalogRefreshing = "PackageCatalogRefreshing";
        internal const string PackageCount = "PackageCount";
        internal const string PackageInstallationLocal = "PackageInstallationLocal";
        internal const string PackageInstalledLocally = "PackageInstalledLocally";
        internal const string PackageInstalledLocallyOldVersion = "PackageInstalledLocallyOldVersion";
        internal const string PackageMatchCount = "PackageMatchCount";
        internal const string PackageTypeLocal = "PackageTypeLocal";
        internal const string PackageTypeLocalSubpackage = "PackageTypeLocalSubpackage";
        internal const string PropertiesClassLocalPackage = "PropertiesClassLocalPackage";
        internal const string PropertiesClassLocalSubPackage = "PropertiesClassLocalSubPackage";
        internal const string PropertiesClassNpm = "PropertiesClassNpm";
        internal const string RemoteDebugProxyFileDoesNotExist = "RemoteDebugProxyFileDoesNotExist";
        internal const string RemoteDebugProxyFolderDoesNotExist = "RemoteDebugProxyFolderDoesNotExist";
        internal const string ReplInitializationMessage = "ReplInitializationMessage";
        internal const string ReplWindowNpmInitNoYesFlagWarning = "ReplWindowNpmInitNoYesFlagWarning";
        internal const string RequestedVersionRangeNone = "RequestedVersionRangeNone";
        internal const string ScriptArgumentsToolTip = "ScriptArgumentsToolTip";
        internal const string ScriptFileToolTip = "ScriptFileTooltip";
        internal const string Seconds = "Seconds";
        internal const string StartBrowserToolTip = "StartBrowserToolTip";
        internal const string StatusAnalysisLoadFailed = "StatusAnalysisLoadFailed";
        internal const string StatusAnalysisLoaded = "StatusAnalysisLoaded";
        internal const string StatusAnalysisLoading = "StatusAnalysisLoading";
        internal const string StatusAnalysisSaved = "StatusAnalysisSaved";
        internal const string StatusAnalysisSaving = "StatusAnalysisSaving";
        internal const string StatusAnalysisUpToDate = "StatusAnalysisUpToDate";
        internal const string StatusTypingsLoaded = "StatusTypingsLoaded";
        internal const string StatusTypingsLoading = "StatusTypingsLoading";
        internal const string SurroundWith = "SurroundWith";
        internal const string TestFramework = "TestFramework";
        internal const string TestFrameworkDescription = "TestFrameworkDescription";
        internal const string TypingsInfoBarSpan1 = "TypingsInfoBarSpan1";
        internal const string TypingsInfoBarSpan2 = "TypingsInfoBarSpan2";
        internal const string TypingsInfoBarSpan3 = "TypingsInfoBarSpan3";
        internal const string TypingsOpenOptionsText = "TypingsOpenOptionsText";
        internal const string TypeScriptMinVersionNotInstalled = "TypeScriptMinVersionNotInstalled";
        internal const string TypingsToolCouldNotStart = "TypingsToolCouldNotStart";
        internal const string TypingsToolInstallFailed = "TypingsToolInstallFailed";
        internal const string TypingsToolNotInstalledError = "TypingsToolNotInstalledError";
        internal const string TypingsToolTypingsInstallCompleted = "TypingsToolTypingsInstallCompleted";
        internal const string TypingsToolTypingsInstallErrorOccurred = "TypingsToolTypingsInstallErrorOccurred";
        internal const string UpgradedEnvironmentVariables = "UpgradedEnvironmentVariables";
        internal const string WorkingDirInvalidOrMissing = "WorkingDirInvalidOrMissing";
        internal const string WorkingDirToolTip = "WorkingDirToolTip";
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
