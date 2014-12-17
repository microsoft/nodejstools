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
using Microsoft.NodejsTools.Analysis.Analyzer;

namespace Microsoft.NodejsTools.Analysis.Values {
    [Serializable]
    internal class ModuleValue : ObjectValue {
        private readonly string _name;
        private readonly ModuleEnvironmentRecord _scope;

        public ModuleValue(string moduleName, ModuleEnvironmentRecord moduleRecord)
            : base(moduleRecord.ProjectEntry) {
            _name = moduleName;
            _scope = moduleRecord;
        }

        public ModuleEnvironmentRecord EnvironmentRecord {
            get {
                return _scope;
            }
        }

        public override string Name {
            get { return _name; }
        }

        public override JsMemberType MemberType {
            get {
                return JsMemberType.Module;
            }
        }

        public override string ToString() {
            return "Module " + base.ToString();
        }

        public override string ShortDescription {
            get {
                return "Node.js module " + Name;
            }
        }

        public override string ObjectDescription {
            get {
                return String.Format("module ({0})", Path.GetFileName(_name));
            }
        }

        public override string Documentation {
            get {
                return String.Empty;
            }
        }

        public override IEnumerable<LocationInfo> Locations {
            get {
                return new[] { new LocationInfo(ProjectEntry, 1, 1) };
            }
        }
    }
}
