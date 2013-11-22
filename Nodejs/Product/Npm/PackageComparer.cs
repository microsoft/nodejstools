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

using System.Collections.Generic;

namespace Microsoft.NodejsTools.Npm {
    public class PackageComparer : IComparer<IPackage> {
        public int Compare(IPackage x, IPackage y) {
            if (x == y) {
                return 0;
            } else if (null == x) {
                return -1;
            } else if (null == y) {
                return 1;
            }
            //  TODO: should take into account versions!
            return x.Name.CompareTo(y.Name);
        }
    }
}