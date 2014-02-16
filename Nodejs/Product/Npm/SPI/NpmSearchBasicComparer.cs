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
using System.Text;

namespace Microsoft.NodejsTools.Npm.SPI {
    internal class NpmSearchBasicComparer : AbstractNpmSearchComparer {

        private int CompareBasedOnDescriptions(IPackage x, IPackage y) {
            string d1 = x.Description, d2 = y.Description;
            if (null == d1) {
                return null == d2 ? CompareBasedOnKeywords(x, y) : 1;
            }
            
            if (null == d2) {
                return -1;
            }

            var result = string.Compare(x.Description, y.Description, StringComparison.CurrentCulture);
            return 0 == result ? CompareBasedOnKeywords(x, y) : result;
        }

        public override int Compare(IPackage x, IPackage y) {
            var result = string.Compare(x.Name, y.Name, StringComparison.CurrentCulture);
            return 0 == result ? CompareBasedOnDescriptions(x, y) : result;
        }
    }
}
