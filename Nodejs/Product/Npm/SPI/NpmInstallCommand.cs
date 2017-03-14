// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.NodejsTools.Npm.SPI
{
    internal class NpmInstallCommand : NpmCommand
    {
        public NpmInstallCommand(
            string fullPathToRootPackageDirectory,
            string pathToNpm = null,
            bool useFallbackIfNpmNotFound = true)
            : base(fullPathToRootPackageDirectory, pathToNpm)
        {
            Arguments = "install";
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
            : base(fullPathToRootPackageDirectory, pathToNpm)
        {
            Arguments = NpmArgumentBuilder.GetNpmInstallArguments(
                packageName,
                versionRange,
                type,
                global,
                saveToPackageJson);
        }
    }
}

