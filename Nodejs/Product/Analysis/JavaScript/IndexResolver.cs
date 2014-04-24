using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.NodejsTools.Parsing;

namespace Microsoft.NodejsTools.Parsing {
    class IndexResolver {
        private readonly List<int> _newLineLocations;
        private readonly SourceLocation _initialLocation;

        public IndexResolver(List<int> newLineLocations, SourceLocation initialLocation) {
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
    }
}
