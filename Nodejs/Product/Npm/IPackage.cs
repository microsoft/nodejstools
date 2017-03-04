// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.NodejsTools.Npm
{
    public interface IPackage : IRootPackage
    {
        string PublishDateTimeString { get; }

        string RequestedVersionRange { get; }

        IEnumerable<string> Keywords { get; }

        IEnumerable<SemverVersion> AvailableVersions { get; }

        bool IsListedInParentPackageJson { get; }

        bool IsMissing { get; }

        bool IsDependency { get; }

        bool IsDevDependency { get; }

        bool IsOptionalDependency { get; }

        bool IsBundledDependency { get; }

        PackageFlags Flags { get; }
    }
}

