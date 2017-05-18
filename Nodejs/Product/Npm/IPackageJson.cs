// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.NodejsTools.Npm
{
    public interface IPackageJson
    {
        string Name { get; }
        SemverVersion Version { get; }
        IPerson Author { get; }
        string Description { get; }
        IKeywords Keywords { get; }
        IHomepages Homepages { get; }
        IFiles Files { get; }
        IDependencies Dependencies { get; }
        IDependencies DevDependencies { get; }
        IBundledDependencies BundledDependencies { get; }
        IDependencies OptionalDependencies { get; }
        IDependencies AllDependencies { get; }
        IEnumerable<string> RequiredBy { get; }
    }
}

