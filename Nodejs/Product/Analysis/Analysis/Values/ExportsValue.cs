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
using System.IO;

namespace Microsoft.NodejsTools.Analysis.Values {
    /// <summary>
    /// The object that is created for a Node.js module's
    /// exports variable.  We create this so that we can show
    /// a different icon in intellisense for modules.

    /// </summary>
    [Serializable]
    class ExportsValue : ObjectValue {
        private readonly string _name;

        public ExportsValue(string name, ProjectEntry projectEntry)
            : base(projectEntry) {
            _name = name;
        }

        public override JsMemberType MemberType {
            get {
                return JsMemberType.Module;
            }
        }

        public override string ObjectDescription {
            get {
                return "exports from " + Path.GetFileName(_name);
            }
        }

        public override IEnumerable<LocationInfo> Locations {
            get {
                if (ProjectEntry.IsBuiltin) {
                    return new LocationInfo[0];
                }
                return new[] { new LocationInfo(ProjectEntry, 1, 1) };
            }
        }
    }
}
