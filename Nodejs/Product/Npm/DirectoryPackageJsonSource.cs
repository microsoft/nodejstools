// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.IO;

namespace Microsoft.NodejsTools.Npm
{
    public class DirectoryPackageJsonSource : IPackageJsonSource
    {
        private readonly FilePackageJsonSource _source;

        public DirectoryPackageJsonSource(string fullDirectoryPath)
        {
            _source = new FilePackageJsonSource(Path.Combine(fullDirectoryPath, "package.json"));
        }

        public dynamic Package
        {
            get { return _source.Package; }
        }
    }
}

