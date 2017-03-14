// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.NodejsTools.Npm.SPI;

namespace Microsoft.NodejsTools.Npm
{
    public class PackageJsonFactory
    {
        public static IPackageJson Create(IPackageJsonSource source)
        {
            return null == source.Package ? null : new PackageJson(source.Package);
        }
    }
}

