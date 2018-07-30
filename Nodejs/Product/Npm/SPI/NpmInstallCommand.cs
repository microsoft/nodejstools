// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.NodejsTools.Npm.SPI
{
    public class NpmInstallCommand : NpmCommand
    {
        public static bool IsInstalling { get; private set; } = false;

        public NpmInstallCommand(
            string fullPathToRootPackageDirectory,
            string pathToNpm = null,
            bool useFallbackIfNpmNotFound = true)
            : base(fullPathToRootPackageDirectory, showConsole: false, pathToNpm: pathToNpm)
        {
            this.Arguments = "install";

            this.CommandStarted += (sender, eventArgs) => { IsInstalling = true; };
            this.CommandStarted += (sender, eventArgs) => { IsInstalling = false; };
        }

        public NpmInstallCommand(
            string fullPathToRootPackageDirectory,
            string packageName,
            string versionRange,
            DependencyType type,
            bool global = false,
            bool saveToPackageJson = true,
            string pathToNpm = null,
            bool useFallbackIfNpmNotFound = true)
            : base(fullPathToRootPackageDirectory, showConsole: false, pathToNpm: pathToNpm)
        {
            this.Arguments = NpmArgumentBuilder.GetNpmInstallArguments(
                packageName,
                versionRange,
                type,
                global,
                saveToPackageJson);
        }
    }
}
