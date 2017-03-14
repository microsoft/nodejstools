// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Globalization;

namespace Microsoft.NodejsTools.Npm.SPI
{
    internal class NpmUninstallCommand : NpmCommand
    {
        public NpmUninstallCommand(
            string fullPathToRootPackageDirectory,
            string packageName,
            DependencyType type,
            bool global = false,
            string pathToNpm = null,
            bool useFallbackIfNpmNotFound = true)
            : base(fullPathToRootPackageDirectory, pathToNpm)
        {
            Arguments = global
                            ? string.Format(CultureInfo.InvariantCulture, "uninstall {0} --g", packageName)
                            : string.Format(CultureInfo.InvariantCulture,
                                "uninstall {0} --{1}",
                                packageName,
                                (type == DependencyType.Standard
                                     ? "save"
                                     : (type == DependencyType.Development ? "save-dev" : "save-optional")));
        }
    }
}

