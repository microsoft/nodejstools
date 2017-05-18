// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.NodejsTools.Npm
{
    public interface IRootPackage
    {
        INodeModules Modules { get; }

        IPackageJson PackageJson { get; }

        bool HasPackageJson { get; }

        string Name { get; }

        SemverVersion Version { get; }

        IPerson Author { get; }

        string Description { get; }

        IEnumerable<string> Homepages { get; }

        string Path { get; }
    }
}
