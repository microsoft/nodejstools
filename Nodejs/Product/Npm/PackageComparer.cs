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
            return x.Name.CompareTo(y.Name);
        }
    }
}