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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Analysis {
    internal class OverloadResult : IOverloadResult {
        private readonly ParameterResult[] _parameters;
        private readonly string _name;

        public OverloadResult(string name, params ParameterResult[] parameters) {
            _parameters = parameters;
            _name = name;
        }

        public string Name {
            get { return _name; }
        }
        public virtual string Documentation {
            get { return null; }
        }
        public virtual ParameterResult[] Parameters {
            get { return _parameters; }
        }
    }

    class SimpleOverloadResult : OverloadResult {
        private readonly string _documentation;

        public SimpleOverloadResult(string name, string documentation) : base(name) {
            _documentation = documentation;
        }

        public SimpleOverloadResult(string name, string documentation, params ParameterResult[] parameters)
            : base(name, parameters) {
            _documentation = documentation;
        }

        public SimpleOverloadResult(string name, string documentation, params string[] parameters)
            : this(name, documentation, parameters.Select(x => new ParameterResult(x)).ToArray()) {
        }

        public override string Documentation {
            get {
                return _documentation;
            }
        }
    }

    class OverloadResultComparer : EqualityComparer<OverloadResult> {
        public static IEqualityComparer<OverloadResult> Instance = new OverloadResultComparer();

        public override bool Equals(OverloadResult x, OverloadResult y) {
            if (x == null | y == null) {
                return x == null & y == null;
            }

            if (x.Name != y.Name || x.Documentation != y.Documentation) {
                return false;
            }

            if (x.Parameters == null | y.Parameters == null) {
                return x.Parameters == null & y.Parameters == null;
            }

            if (x.Parameters.Length != y.Parameters.Length) {
                return false;
            }

            for (int i = 0; i < x.Parameters.Length; ++i) {
                if (!x.Parameters[i].Equals(y.Parameters[i])) {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode(OverloadResult obj) {
            // Don't use Documentation for hash code, since it changes over time
            // in some implementations of IOverloadResult.
            int hc = 552127 ^ (obj.Name != null ? obj.Name.GetHashCode() : 0);
            if (obj.Parameters != null) {
                foreach (var p in obj.Parameters) {
                    hc ^= p.GetHashCode();
                }
            }
            return hc;
        }
    }
}
