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
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.NodejsTools.Npm.SPI {
    internal class NpmSearchRegexComparer : AbstractNpmSearchComparer {

        private readonly Regex _regex;

        public NpmSearchRegexComparer(Regex regex) {
            _regex = regex;
        }

        private int GetFirstMatchIndex(MatchCollection matches) {
            return null == matches || matches.Count == 0 ? int.MaxValue : matches[0].Index;
        }

        private IList<MatchCollection> GetKeywordMatches(IPackage x) {
            return x.Keywords.Select(keyword => _regex.Matches(keyword)).Where(matches => matches.Count > 0).ToList();
        }

        private int GetSumOfLowestMatchIndexes(IList<MatchCollection> matches) {
            return matches.Count == 0 ? int.MaxValue : matches.Sum(collection => collection[0].Index);
        }

        private new int CompareBasedOnKeywords(IPackage x, IPackage y) {
            var xMatches = GetKeywordMatches(x);
            var yMatches = GetKeywordMatches(y);

            var result = -xMatches.Count.CompareTo(yMatches.Count); //  Highest number wins
            if (0 == result) {
                result = GetSumOfLowestMatchIndexes(xMatches).CompareTo(GetSumOfLowestMatchIndexes(yMatches));
                if (0 == result) {
                    result = base.CompareBasedOnKeywords(x, y);
                }
            }

            return result;
        }

        private int CompareBasedOnDescriptions(IPackage x, IPackage y) {
            string d1 = x.Description, d2 = y.Description;
            if (null == d1 || null == d2) {
                return CompareBasedOnKeywords(x, y);
            }

            var xMatches = _regex.Matches(d1);
            var yMatches = _regex.Matches(d2);

            var result = GetFirstMatchIndex(xMatches).CompareTo(GetFirstMatchIndex(yMatches));
            if (0 == result) {
                result = -xMatches.Count.CompareTo(yMatches.Count);
                if (0 == result) {
                    result = CompareBasedOnKeywords(x, y);
                    if (0 == result) {
                        result = string.Compare(d1, d2, StringComparison.CurrentCulture);
                    }
                }
            }

            return result;
        }

        public override int Compare(IPackage x, IPackage y) {
            var xMatches = _regex.Matches(x.Name);
            var yMatches = _regex.Matches(y.Name);

            var result = GetFirstMatchIndex(xMatches).CompareTo(GetFirstMatchIndex(yMatches));
            if (0 == result) {
                result = -xMatches.Count.CompareTo(yMatches.Count); //  Highest number wins
                if (0 == result) {
                    result = CompareBasedOnDescriptions(x, y);
                    if (0 == result) {
                        result = string.Compare(x.Name, y.Name, StringComparison.CurrentCulture);
                    } 
                }
            }

            return result;
        }
    }
}
