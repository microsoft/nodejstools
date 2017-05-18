// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.NodejsTools.Npm.SPI;

namespace Microsoft.NodejsTools.Npm
{
    public static class RootPackageFactory
    {
        public static IRootPackage Create(
            string fullPathToRootDirectory,
            bool showMissingDevOptionalSubPackages = false,
            int maxDepth = 1)
        {
            return new RootPackage(
                fullPathToRootDirectory,
                showMissingDevOptionalSubPackages,
                null,
                0,
                maxDepth);
        }
    }
}
