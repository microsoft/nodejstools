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

namespace Microsoft.NodejsTools.Npm.SPI {
    internal class PackageCatalogFilter : IPackageCatalogFilter {

        private readonly IPackageCatalog _source;

        public PackageCatalogFilter(IPackageCatalog source) {
            _source = source;
        }

        public IComparer<IPackage> Comparer { get; set; }

        public IList<IPackage> Filter(string filterString) {
            if (null == _source) {
                return new List<IPackage>();
            }

            throw new NotImplementedException();
        }
    }
}
