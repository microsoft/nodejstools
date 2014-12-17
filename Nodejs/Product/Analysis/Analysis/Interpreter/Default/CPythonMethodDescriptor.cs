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

namespace Microsoft.NodejsTools.Interpreter.Default {
#if FALSE
    class CPythonMethodDescriptor : IPythonMethodDescriptor {
        private readonly string _name;
        private readonly CPythonFunction _func;
        private readonly bool _isBound;

        public CPythonMethodDescriptor(ITypeDatabaseReader typeDb, string name, Dictionary<string, object> valueDict, IMemberContainer declaringType) {
            _name = name;
            _func = new CPythonFunction(typeDb, name, valueDict, declaringType, isMethod: true);
            object value;
            if (valueDict.TryGetValue("bound", out value)) {
                _isBound = (value as bool?) ?? false;
            }
        }

        #region IBuiltinMethodDescriptor Members

        public IPythonFunction Function {
            get { return _func;  }
        }

        public bool IsBound {
            get { return _isBound; }
        }

        #endregion

        #region IMember Members

        public PythonMemberType MemberType {
            get { return PythonMemberType.Method; }
        }

        #endregion
    }
#endif
}
