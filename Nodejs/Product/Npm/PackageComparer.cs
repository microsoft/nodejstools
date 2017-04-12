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

namespace Microsoft.NodejsTools.Npm {
    public class PackageComparer : IComparer<IPackage> {
        public int Compare(IPackage x, IPackage y) {
            if (x == y) {
                return 0;
            } else if (null == x) {
                return -1;
            } else if (null == y) {
                return 1;
            }
            //  TODO: should take into account versions!
            return string.Compare(x.Name, y.Name, StringComparison.Ordinal);
        }
    }

    public class PackageEqualityComparer : EqualityComparer<IPackage> {
        public override bool Equals(IPackage p1, IPackage p2) {
            return p1.Name == p2.Name
                && p1.Version == p2.Version
                && p1.IsBundledDependency == p2.IsBundledDependency
                && p1.IsDevDependency == p2.IsDevDependency
                && p1.IsListedInParentPackageJson == p2.IsListedInParentPackageJson
                && p1.IsMissing == p2.IsMissing
                && p1.IsOptionalDependency == p2.IsOptionalDependency;
        }

        public override int GetHashCode(IPackage obj) {
            if (obj.Name == null || obj.Version == null)
                return obj.GetHashCode();
            return obj.Name.GetHashCode() ^ obj.Version.GetHashCode();
        }
    }
}