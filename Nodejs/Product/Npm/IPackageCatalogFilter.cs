/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Npm {

    /// <summary>
    /// Used to filter the entire package catalog down to a more manageable/relevant
    /// list.
    /// </summary>
    public interface IPackageCatalogFilter {

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
        IList<IPackage> Filter(string filterString);  
    }
}
