// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.NodejsTools.Npm.SPI
{
    internal abstract class AbstractNpmSearchComparer : IComparer<IPackage>
    {
        protected int CompareBasedOnKeywords(IPackage x, IPackage y)
        {
            if (x.Keywords != null && y.Keywords != null)
            {
                return StringComparer.CurrentCulture.Compare(
                    string.Join(", ", x.Keywords),
                    string.Join(", ", y.Keywords));
            }

            return 0;
        }

        public abstract int Compare(IPackage x, IPackage y);
    }
}

