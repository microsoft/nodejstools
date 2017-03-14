// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.NodejsTools.Npm
{
    /// <summary>
    /// Used to filter the entire package catalog down to a more manageable/relevant
    /// list.
    /// </summary>
    public interface IPackageCatalogFilter
    {
        /// <summary>
        /// Filters the entire package list based on the supplied filter string and returns
        /// any packages that match. If filterString starts with a '/' it will be treated as
        /// a regular expression. In this case, if that last character in the string is also a
        /// '/' it will be strimmed and ignored, consistent with npm's command line behaviour.
        /// </summary>
        /// <param name="filterString">
        /// String or regular expression, denoted by a leading '/' character, with which to
        /// filter the package catalog. If the string is null or empty all packages will be returned.
        /// </param>
        /// <returns>
        /// List of matching packages. If there are no matches an empty list is returned.
        /// </returns>
        IEnumerable<IPackage> Filter(string filterString);
    }
}

