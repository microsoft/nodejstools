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
using System.Linq;
using System.Text.RegularExpressions;

namespace Microsoft.NodejsTools.Npm.SPI {
    internal class PackageCatalogFilter : IPackageCatalogFilter {

        private readonly IPackageCatalog _source;

        public PackageCatalogFilter(IPackageCatalog source) {
            _source = source;
        }

        private IList<IPackage> FilterFromString(string filterString) {
            filterString = filterString.ToLower();

            var target = new List<IPackage>();
            foreach (var package in _source.Results) {
                if (string.IsNullOrEmpty(filterString) || package.Name.ToLower().Contains(filterString)) {
                    target.Add(package);
                    continue;
                }

                var description = package.Description;
                if (null != description && description.ToLower().Contains(filterString)) {
                    target.Add(package);
                    continue;
                }

                if (package.Keywords.Any(keyword => keyword.ToLower().Contains(filterString))) {
                    target.Add(package);
                }
            }

            target.Sort(new NpmSearchFilterStringComparer(filterString));
            return target;
        } 

        private IList<IPackage> FilterFromRegex(string pattern) {
            if (pattern.Length == 0) {
                return _source.Results;
            }

            var regex = new Regex(pattern, RegexOptions.IgnoreCase);
            var target = new List<IPackage>();
            foreach (var package in _source.Results) {
                if (regex.IsMatch(package.Name)) {
                    target.Add(package);
                    continue;
                }

                var description = package.Description;
                if (null != description && regex.IsMatch(description)) {
                    target.Add(package);
                    continue;
                }

                if (package.Keywords.Any(regex.IsMatch)) {
                    target.Add(package);
                }
            }

            target.Sort(new NpmSearchRegexComparer(regex));
            return target;
        }

        private static bool IsRegex(string filterString) {
            return filterString[0] == '/';
        }

        private static string StripSlashes(string source) {
            source = source.Length > 1 ? source.Substring(1) : string.Empty;
            return source.Length > 0 && source[source.Length - 1] == '/'
                ? (source.Length > 1 ? source.Substring(0, source.Length - 1) : string.Empty)
                : source;
        }

        public IList<IPackage> Filter(string filterString) {
            if (null == _source) {
                return new List<IPackage>();
            }
            
            if (string.IsNullOrEmpty(filterString)) {
                return _source.Results;
            }

            return IsRegex(filterString) ? FilterFromRegex(StripSlashes(filterString)) : FilterFromString(filterString);
        }
    }
}
