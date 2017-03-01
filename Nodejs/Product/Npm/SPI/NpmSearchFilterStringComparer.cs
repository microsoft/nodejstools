// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Linq;

namespace Microsoft.NodejsTools.Npm.SPI
{
    internal class NpmSearchFilterStringComparer : AbstractNpmSearchComparer
    {
        private readonly string _filterString;

        public NpmSearchFilterStringComparer(string filterString)
        {
            _filterString = filterString;
        }

        private int GetExactKeywordMatchCount(IPackage source)
        {
            return source.Keywords == null ? 0 : source.Keywords.Count(keyword => keyword.ToLower(CultureInfo.InvariantCulture) == _filterString);
        }

        private int GetStartsWithMatchCount(IPackage source)
        {
            return source.Keywords == null ? 0 : source.Keywords.Count(keyword => keyword.ToLower(CultureInfo.InvariantCulture).StartsWith(_filterString, StringComparison.Ordinal));
        }

        private int GetPartialKeywordMatchCount(IPackage source)
        {
            return source.Keywords == null ? 0 : source.Keywords.Count(keyword => keyword.ToLower(CultureInfo.InvariantCulture).Contains(_filterString));
        }

        private new int CompareBasedOnKeywords(IPackage x, IPackage y)
        {
            int xCount = GetExactKeywordMatchCount(x),
                yCount = GetExactKeywordMatchCount(y);

            if (xCount == yCount)
            {
                xCount = GetStartsWithMatchCount(x);
                yCount = GetStartsWithMatchCount(y);

                if (xCount == yCount)
                {
                    xCount = GetPartialKeywordMatchCount(x);
                    yCount = GetPartialKeywordMatchCount(y);

                    if (xCount == yCount)
                    {
                        var result = base.CompareBasedOnKeywords(x, y);
                        return 0 == result
                            ? StringComparer.CurrentCulture.Compare(x.Name, y.Name)
                            : result;
                    }
                }
            }

            return xCount > yCount ? -1 : 1;
        }

        private int CompareBasedOnDescriptions(IPackage x, IPackage y)
        {
            string d1 = x.Description, d2 = y.Description;
            if (null == d1)
            {
                if (null == d2)
                {
                    return CompareBasedOnKeywords(x, y);
                }

                return d2.Contains(_filterString) ? 1 : CompareBasedOnKeywords(x, y);
            }

            if (null == d2)
            {
                return d1.Contains(_filterString) ? -1 : CompareBasedOnKeywords(x, y);
            }

            if (d1.Contains(_filterString))
            {
                return d2.Contains(_filterString)
                    ? StringComparer.CurrentCulture.Compare(d1, d2)
                    : -1;
            }

            if (d2.Contains(_filterString))
            {
                return 1;
            }

            return CompareBasedOnKeywords(x, y);
        }

        public override int Compare(IPackage x, IPackage y)
        {
            if (string.IsNullOrEmpty(_filterString))
            {
                //  Version numbers are irrelevant, and names are good enough since they must be unique in the repo
                return StringComparer.CurrentCulture.Compare(x.Name, y.Name);
            }

            if (x.Name == _filterString)
            {
                return y.Name == _filterString
                    ? 0 //  In theory should never happen since package names have to be unique
                    : -1; //  Exact matches are always the best
            }

            if (y.Name == _filterString)
            {  //  Again, exact matches are always best
                return 1;
            }

            // Matches at the beginning are better than matches in the string
            if (x.Name.StartsWith(_filterString, StringComparison.CurrentCulture))
            {
                if (y.Name.StartsWith(_filterString, StringComparison.CurrentCulture))
                {
                    var result = StringComparer.CurrentCulture.Compare(x.Name, y.Name);
                    return 0 == result ? CompareBasedOnDescriptions(x, y) : result;
                }

                return -1;
            }

            if (y.Name.StartsWith(_filterString, StringComparison.CurrentCulture))
            {
                return 1;
            }

            if (x.Name.Contains(_filterString))
            {
                return y.Name.Contains(_filterString)
                    ? StringComparer.CurrentCulture.Compare(x.Name, y.Name)
                    : -1;
            }

            if (y.Name.Contains(_filterString))
            {
                return 1;
            }

            return CompareBasedOnDescriptions(x, y);
        }
    }
}

