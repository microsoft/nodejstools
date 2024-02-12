// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

// TODO: Check if this specific using is needed. It's .npm in PackageJsonScannerFactory and .TypeScript in TypeScriptScannerFactory
using Microsoft.NodejsTools;
using Microsoft.NodejsTools.Npm;

using Microsoft.VisualStudio.Workspace;
using Microsoft.VisualStudio.Workspace.Build;
using Microsoft.VisualStudio.Workspace.Debug;
using Microsoft.VisualStudio.Workspace.Indexing;

/*using Microsoft.MigrateToJsps;*/ //Added this to access the LaunchJson.cs file to deserialize the object. Commented it out when I copied over the relevant code to this folder instead.
using Newtonsoft.Json;

namespace Microsoft.NodejsTools.Workspace
{
    [ExportFileScanner(
        ProviderType, "JsonFile", //TODO: Change to Whatever the equivalent is for launch.json file (In Originals: TypeScriptFile and PackageJsonProject)
        new string[] { NodejsConstants.LaunchJsonFile },
        new Type[] { typeof(IReadOnlyCollection<FileDataValue>), typeof(IReadOnlyCollection<FileReferenceInfo>) },
        ProviderPriority.Normal)]
    public sealed class LaunchJsonScannerFactory : IWorkspaceProviderFactory<IFileScanner>
    {
        // TODO: Change this to the proper GUID. Right now, it's just a one-offset from the TS Scanner
        private const string ProviderType = "{1EBD9DE4-22CE-4281-A5D6-CB078794E4CE}"; 

        public IFileScanner CreateProvider(IWorkspace workspaceContext)
        {
            return new LaunchJsonScanner(workspaceContext);
        }

        internal static bool IsLaunchJsonFile(string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            return StringComparer.OrdinalIgnoreCase.Equals(fileName, NodejsConstants.LaunchJsonFile);
        }

        private sealed class LaunchJsonScanner : BaseFileScanner, IFileScanner, IFileScannerUpToDateCheck
        {
            public LaunchJsonScanner(IWorkspace workspaceContext)
                : base(workspaceContext)
            {
            }

            // Check if the file is of the form launch.json.
            // Note: We will need to make sure this is the correct launch.json file in the future, but this will work for prototyping.
            protected override Task<bool> IsValidFileAsync(string filePath)
            {
                var isValidFile = IsLaunchJsonFile(filePath);
                return Task.FromResult(isValidFile);
            }

            protected override Task<List<FileReferenceInfo>> ComputeFileReferencesAsync(string filePath, CancellationToken cancellationToken)
            {
                Debug.Assert(IsLaunchJsonFile(filePath), $"{filePath} should be a launch.json file");
                var json = File.ReadAllText(filePath);
                var launchjson = JsonConvert.DeserializeObject<LaunchJson>(json);

                // TODO: Determine what content is supposed to be returned by the ComputeFileReferenceAsync file
                // TODO: Extract that content from the LaunchJson Object. Make sure that all of the relevant configurations are included in this.

                var fileReferences = new List<FileReferenceInfo>();
                foreach (var configuration in launchjson.Configurations)
                {
                    var configurationString = configuration.ToJsonString();
                    fileReferences.Add(new FileReferenceInfo(configurationString,
                        context: "Debug",
                        target: configurationString, // NOTE: This is going to be referenced in ComputeFileDataValuesAsync and they need to match. Make sure the targets are unique and deterministic.
                        referenceType: (int)FileReferenceInfoType.Output));
                }

                return Task.FromResult(fileReferences);

                // Old Code Below Here 

                //var packageJson = PackageJsonFactory.Create(filePath);
                //var main = packageJson.Main;

                //var fileReferences = new List<FileReferenceInfo>();

                //if (!string.IsNullOrEmpty(main))
                //{
                //    fileReferences.Add(new FileReferenceInfo(main,
                //        context: "Debug",
                //        target: main, // NOTE: This is going to be referenced in ComputeFileDataValuesAsync and they need to match. Make sure the targets are unique and deterministic.
                //        referenceType: (int)FileReferenceInfoType.Output));
                //}

                //return Task.FromResult(fileReferences);

                // Old Code Ends here
            }

            protected override Task<List<FileDataValue>> ComputeFileDataValuesAsync(string filePath, CancellationToken cancellationToken)
            {
                Debug.Assert(IsLaunchJsonFile(filePath), $"{filePath} should be a launch.json file");
                var json = File.ReadAllText(filePath);
                var launchjson = JsonConvert.DeserializeObject<LaunchJson>(json);

                // TODO: iterate through the contents of the launchjson.Configurations property and create a new launchSetting for each of them
                // (there may be additional constraints on this later, so make sure it's easy to modify whether or not a given entry in the array is actually added).
                // NOTE: Each configuration will need its own unique identifier. It might be acceptable to just use the entire configuration entry itself as the "identifier".


                var fileDataValues = new List<FileDataValue>();
                foreach (var configuration in launchjson.Configurations)
                {
                    var configurationString = configuration.ToJsonString();
                    var launchSettings = new PropertySettings
                    {
                        [LaunchConfigurationConstants.NameKey] = $"node {configuration.Name} (package.json)",
                        [LaunchConfigurationConstants.TypeKey] = "default"
                    };

                    fileDataValues.Add(new FileDataValue(
                        DebugLaunchActionContext.ContextTypeGuid,
                        DebugLaunchActionContext.IsDefaultStartupProjectEntry,
                        launchSettings,
                        target: configurationString));

                    // Target has to match the name used in the debug action context so it can be found during project configuration
                    fileDataValues.Add(new FileDataValue(DebugLaunchActionContext.ContextTypeGuid, configurationString, null, target: configurationString));

                    // Also need a null target so that can be found for the context menu when querying for build configurations. 
                    // (See Microsoft.VisualStudio.Workspace.VSIntegration.UI.FileContextActionsCommandHandlersProvider.Provider.GetActionProviderForProjectConfiguration)
                    fileDataValues.Add(new FileDataValue(DebugLaunchActionContext.ContextTypeGuid, configurationString, null, target: null));
                }

                return Task.FromResult(fileDataValues);

                // Old Code Below Here

                //var packageJson = PackageJsonFactory.Create(this.EnsureRooted(filePath));

                //var main = packageJson.Main;

                ////var fileDataValues = new List<FileDataValue>();
                //if (!string.IsNullOrEmpty(main))
                //{
                //    var launchSettings = new PropertySettings
                //    {
                //        [LaunchConfigurationConstants.NameKey] = $"node {main} (package.json)",
                //        [LaunchConfigurationConstants.TypeKey] = "default"
                //    };

                //    fileDataValues.Add(new FileDataValue(
                //        DebugLaunchActionContext.ContextTypeGuid,
                //        DebugLaunchActionContext.IsDefaultStartupProjectEntry,
                //        launchSettings,
                //        target: main));

                //    // Target has to match the name used in the debug action context so it can be found during project configuration
                //    fileDataValues.Add(new FileDataValue(DebugLaunchActionContext.ContextTypeGuid, main, null, target: main));

                //    // Also need a null target so that can be found for the context menu when querying for build configurations. 
                //    // (See Microsoft.VisualStudio.Workspace.VSIntegration.UI.FileContextActionsCommandHandlersProvider.Provider.GetActionProviderForProjectConfiguration)
                //    fileDataValues.Add(new FileDataValue(DebugLaunchActionContext.ContextTypeGuid, main, null, target: null));
                //}

                //// Note: Didn't copy this block into the new code
                //var testRoot = packageJson.TestRoot;
                //if (!string.IsNullOrEmpty(testRoot))
                //{
                //    fileDataValues.Add(new FileDataValue(NodejsConstants.TestRootDataValueGuid, NodejsConstants.TestRootDataValueName, testRoot));
                //}

                //return Task.FromResult(fileDataValues);
                // Old Code Ends Here
            }

            #region TypeScriptScannerFactory 
            //protected override async Task<List<FileReferenceInfo>> ComputeFileReferencesAsync(string filePath, CancellationToken cancellationToken)
            //{
            //    Debug.Assert(await this.IsValidFileAsync(filePath), $"{filePath} should be a launch.json file.");

            //    var outFile = await DetermineOutFileAsync(filePath);

            //    if (string.IsNullOrEmpty(outFile))
            //    {
            //        return new List<FileReferenceInfo>(0);
            //    }

            //    var fileReferences = new List<FileReferenceInfo>
            //    {
            //        new FileReferenceInfo(outFile,
            //                              context: "Debug",
            //                              target: outFile,
            //                              referenceType: (int)FileReferenceInfoType.Output)
            //    };

            //    return fileReferences;
            //}

            //protected override async Task<List<FileDataValue>> ComputeFileDataValuesAsync(string filePath, CancellationToken cancellationToken)
            //{
            //    var outFile = await DetermineOutFileAsync(filePath);

            //    if (string.IsNullOrEmpty(outFile))
            //    {
            //        return new List<FileDataValue>(0);
            //    }

            //    var fileDataValues = new List<FileDataValue>();
            //    if (!string.IsNullOrEmpty(outFile))
            //    {
            //        var launchSettings = new PropertySettings
            //        {
            //            [LaunchConfigurationConstants.NameKey] = $"node test foo {outFile}",
            //            [LaunchConfigurationConstants.TypeKey] = "default"
            //        };

            //        fileDataValues.Add(new FileDataValue(
            //            DebugLaunchActionContext.ContextTypeGuid,
            //            DebugLaunchActionContext.IsDefaultStartupProjectEntry,
            //            launchSettings,
            //            target: outFile));

            //        fileDataValues.Add(
            //            new FileDataValue(BuildConfigurationContext.ContextTypeGuid, outFile, null,
            //            context: "Debug", target: outFile));

            //        fileDataValues.Add(
            //            new FileDataValue(BuildConfigurationContext.ContextTypeGuid, outFile, null,
            //            context: "Debug", target: null));
            //    }
            //    return fileDataValues;
            //}
            #endregion

            

        }
    }
}
