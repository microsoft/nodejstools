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

namespace Microsoft.NodejsTools.Interpreter.Default {
#if FALSE
    class CPythonSequenceType : IPythonSequenceType {
        private readonly IPythonType _type;
        private readonly List<IPythonType> _indexTypes;

        public CPythonSequenceType(IPythonType baseType, ITypeDatabaseReader typeDb, List<object> indexTypes) {
            _type = baseType;

            if (indexTypes != null) {
                _indexTypes = new List<IPythonType>();

                foreach (var indexType in indexTypes) {
                    typeDb.LookupType(indexType, type => _indexTypes.Add(type));
                }
            }
        }

        public IEnumerable<IPythonType> IndexTypes {
            get {
                return _indexTypes;
            }
        }


        public IPythonFunction GetConstructors() {
            return _type.GetConstructors();
        }

        public string Name {
            get { return _type.Name; }
        }

        public string Documentation {
            get { return _type.Documentation; }
        }

        public BuiltinTypeId TypeId {
            get { return _type.TypeId; }
        }

        public IList<IPythonType> Mro {
            get { return _type.Mro; }
        }

        public IPythonModule DeclaringModule {
            get { return _type.DeclaringModule; }
        }

        public bool IsBuiltin {
            get { return _type.IsBuiltin; }
        }

        public IMember GetMember(IModuleContext context, string name) {
            return _type.GetMember(context, name);
        }

        public IEnumerable<string> GetMemberNames(IModuleContext moduleContext) {
            return _type.GetMemberNames(moduleContext);
        }

        public PythonMemberType MemberType {
            get { return _type.MemberType; }
        }

        public override string ToString() {
            return String.Format("CPythonSequenceType('{0}', '{1}')", Name, string.Join("', '", _indexTypes.Select((t => t.Name))));
        }
    }
#endif
}
