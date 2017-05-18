// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.NodejsTools.Npm.SPI;

namespace Microsoft.NodejsTools.Npm
{
    public static class NpmControllerFactory
    {
        public static INpmController Create(
            string fullPathToRootPackageDirectory,
            string cachePath,
            bool showMissingDevOptionalSubPackages = false,
            INpmPathProvider npmPathProvider = null,
            bool useFallbackIfNpmNotFound = true)
        {
            return new NpmController(
                fullPathToRootPackageDirectory,
                cachePath,
                showMissingDevOptionalSubPackages,
                npmPathProvider);
        }
    }
}
