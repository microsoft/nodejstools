// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Npm
{
    public interface IPackageCatalog
    {
        DateTime LastRefreshed { get; }

        Task<IEnumerable<IPackage>> GetCatalogPackagesAsync(string filterText, Uri registryUrl = null);

        IPackage this[string name] { get; }

        long? ResultsCount { get; }
    }
}

