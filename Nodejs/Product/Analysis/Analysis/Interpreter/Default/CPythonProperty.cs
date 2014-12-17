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
using Microsoft.NodejsTools.Analysis;

namespace Microsoft.NodejsTools.Interpreter.Default {
#if FALSE
    class CPythonProperty : IBuiltinProperty, ILocatedMember {
        private readonly string _doc;
        private IPythonType _type;
        private readonly CPythonModule _declaringModule;
        private readonly bool _hasLocation;
        private readonly int _line, _column;
        
        public CPythonProperty(ITypeDatabaseReader typeDb, Dictionary<string, object> valueDict, IMemberContainer container) {
            _declaringModule = CPythonModule.GetDeclaringModuleFromContainer(container);

            object value;
            if (valueDict.TryGetValue("doc", out value)) {
                _doc = value as string;
            }

            object type;
            valueDict.TryGetValue("type", out type);
#if FALSE
            _hasLocation = PythonTypeDatabase.TryGetLocation(valueDict, ref _line, ref _column);
#endif
            _hasLocation = false;

            typeDb.LookupType(type, typeValue => _type = typeValue);
        }

        #region IBuiltinProperty Members

        public IPythonType Type {
            get { return _type; }
        }

        public bool IsStatic {
            get { return false; }
        }

        public string Documentation {
            get { return _doc; }
        }

        public string Description {
            get {
                return "property of type " + Type.Name;
            }
        }

        #endregion

        #region IMember Members

        public PythonMemberType MemberType {
            get { return PythonMemberType.Property; }
        }

        #endregion

        #region ILocatedMember Members

        public IEnumerable<LocationInfo> Locations {
            get {
                if (_hasLocation) {
                    yield return new LocationInfo(_declaringModule, _line, _column);
                }
            }
        }

        #endregion
    }
#endif
}
