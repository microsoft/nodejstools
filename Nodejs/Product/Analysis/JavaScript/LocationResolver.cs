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

namespace Microsoft.NodejsTools.Parsing {
    [Serializable]
    internal class LocationResolver {
        private readonly List<int> _newLineLocations;
        private readonly SourceLocation _initialLocation;
        internal List<IndexSpan> _spans;

        public LocationResolver(List<int> newLineLocations, SourceLocation initialLocation) {
            _newLineLocations = newLineLocations;
            _initialLocation = initialLocation;
        }

        public SourceLocation IndexToLocation(int index) {
            int match = _newLineLocations.BinarySearch(index);
            if (match < 0) {
                // If our index = -1, it means we're on the first line.
                if (match == -1) {
                    return new SourceLocation(index + _initialLocation.Index, _initialLocation.Line, checked(index + _initialLocation.Column));
                }
                // If we couldn't find an exact match for this line number, get the nearest
                // matching line number less than this one
                match = ~match - 1;
            }

            return new SourceLocation(index + _initialLocation.Index, match + 2 + _initialLocation.Line - 1, index - _newLineLocations[match] + _initialLocation.Column);
        }

        internal int AddSpan(int start, int length) {
            if (_spans == null) {
                _spans = new List<IndexSpan>();
            }
            int res;
            res = _spans.Count | unchecked((int)0x80000000);
            _spans.Add(new IndexSpan(start, length));
            return res;
        }

    }
}
