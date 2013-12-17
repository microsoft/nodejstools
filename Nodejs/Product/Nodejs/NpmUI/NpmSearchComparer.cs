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
using Microsoft.NodejsTools.Npm;

namespace Microsoft.NodejsTools.NpmUI {
    internal class NpmSearchComparer : IComparer<IPackage> {
        private string _filterString;

        public NpmSearchComparer(string filterString) {
            _filterString = filterString;
        }

        //  TODO: keyword-based sort

        private int CompareBasedOnDescriptions(IPackage x, IPackage y) {
            string d1 = x.Description, d2 = y.Description;
            if (null == d1) {
                if (null == d2) {
                    return x.Name.CompareTo(y.Name);
                } else {
                    if (d2.Contains(_filterString)) {
                        return 1;
                    } else {
                        return x.Name.CompareTo(y.Name);
                    }
                }
            } else if (null == d2) {
                if (d1.Contains(_filterString)) {
                    return -1;
                } else {
                    return x.Name.CompareTo(y.Name);
                }
            } else {
                if (d1.Contains(_filterString)) {
                    if (d2.Contains(_filterString)) {
                        return d1.CompareTo(d2);
                    } else {
                        return -1;
                    }
                } else if (d2.Contains(_filterString)) {
                    return 1;
                } else {
                    return x.Name.CompareTo(y.Name);
                }
            }
        }

        public int Compare(IPackage x, IPackage y) {
            if (string.IsNullOrEmpty(_filterString)) {
                //  Version numbers are irrelevant, and names are good enough since they must be unique in the repo
                return x.Name.CompareTo(y.Name);
            }

            if (x.Name == _filterString) {
                if (y.Name == _filterString) {
                    return 0;
                    //  In theory should never happen since package names have to be unique
                } else {
                    return -1; //  Exact matches are always the best
                }
            } else {
                if (y.Name == _filterString) {
                    return 1;
                } else if (x.Name.StartsWith(_filterString)) {
                    if (y.Name.StartsWith(_filterString)) {
                        var result = x.Name.CompareTo(y.Name);
                        if (0 == result) {
                            return CompareBasedOnDescriptions(x, y);
                        } else {
                            return result;
                        }
                    } else {
                        return -1;
                        // Matches at the beginning are better than matches in the string
                    }
                } else if (y.Name.StartsWith(_filterString)) {
                    return 1;
                } else if (x.Name.Contains(_filterString)) {
                    if (y.Name.Contains(_filterString)) {
                        return x.Name.CompareTo(y.Name);
                    } else {
                        return -1;
                    }
                } else if (y.Name.Contains(_filterString)) {
                    return 1;
                } else {
                    return CompareBasedOnDescriptions(x, y);
                }
            }
        }
    }
}