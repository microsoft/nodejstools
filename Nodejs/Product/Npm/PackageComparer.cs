// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.NodejsTools.Npm
{
    public class PackageComparer : IComparer<IPackage>
    {
        public int Compare(IPackage x, IPackage y)
        {
            if (x == y)
            {
                return 0;
            }
            else if (null == x)
            {
                return -1;
            }
            else if (null == y)
            {
                return 1;
            }
            //  TODO: should take into account versions!
            return StringComparer.Ordinal.Compare(x.Name, y.Name);
        }
    }

    public class PackageEqualityComparer : EqualityComparer<IPackage>
    {
        public override bool Equals(IPackage p1, IPackage p2)
        {
            return p1.Name == p2.Name
                && p1.Version == p2.Version
                && p1.IsBundledDependency == p2.IsBundledDependency
                && p1.IsDevDependency == p2.IsDevDependency
                && p1.IsListedInParentPackageJson == p2.IsListedInParentPackageJson
                && p1.IsMissing == p2.IsMissing
                && p1.IsOptionalDependency == p2.IsOptionalDependency;
        }

        public override int GetHashCode(IPackage obj)
        {
            if (obj.Name == null || obj.Version == null)
            {
                return obj.GetHashCode();
            }

            return obj.Name.GetHashCode() ^ obj.Version.GetHashCode();
        }
    }
}

