//*********************************************************//
//    Copyright (c) Microsoft. All rights reserved.
//    
//    Apache 2.0 License
//    
//    You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//    
//    Unless required by applicable law or agreed to in writing, software 
//    distributed under the License is distributed on an "AS IS" BASIS, 
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or 
//    implied. See the License for the specific language governing 
//    permissions and limitations under the License.
//
//*********************************************************//

using System;
using System.Collections.Generic;

namespace Microsoft.NodejsTools.Analysis {
    internal class LocationInfo : IEquatable<LocationInfo> {
        private readonly int _line, _column;
        internal readonly ProjectEntry _entry;
        internal static LocationInfo[] Empty = new LocationInfo[0];

        private static readonly IEqualityComparer<LocationInfo> _fullComparer = new FullLocationComparer();

        internal LocationInfo(ProjectEntry entry, int line, int column) {
            _entry = entry;
            _line = line;
            _column = column;
        }

        public IProjectEntry ProjectEntry {
            get {
                return _entry;
            }
        }

        public string FilePath {
            get { return _entry.FilePath; }
        }

        public int Line {
            get { return _line; }
        }

        public int Column {
            get {
                return _column;
            }
        }

        public override bool Equals(object obj) {
            LocationInfo other = obj as LocationInfo;
            if (other != null) {
                return Equals(other);
            }
            return false;
        }

        public override int GetHashCode() {
            return Line.GetHashCode() ^ ProjectEntry.GetHashCode();
        }

        public bool Equals(LocationInfo other) {
            // currently we filter only to line & file - so we'll only show 1 ref per each line
            // This works nicely for get and call which can both add refs and when they're broken
            // apart you still see both refs, but when they're together you only see 1.
            return Line == other.Line &&
                ProjectEntry == other.ProjectEntry;
        }

        /// <summary>
        /// Provides an IEqualityComparer that compares line, column and project entries.  By
        /// default locations are equaitable based upon only line/project entry.
        /// </summary>
        public static IEqualityComparer<LocationInfo> FullComparer {
            get{
                return _fullComparer;
            }
        }

        sealed class FullLocationComparer : IEqualityComparer<LocationInfo> {
            public bool Equals(LocationInfo x, LocationInfo y) {
                return x.Line == y.Line &&
                    x.Column == y.Column &&
                    x.ProjectEntry == y.ProjectEntry;
            }

            public int GetHashCode(LocationInfo obj) {
                return obj.Line.GetHashCode() ^ obj.Column.GetHashCode() ^ obj.ProjectEntry.GetHashCode();
            }
        }
    }
}
