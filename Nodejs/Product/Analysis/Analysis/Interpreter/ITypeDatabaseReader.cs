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

namespace Microsoft.NodejsTools.Interpreter {
#if FALSE
    /// <summary>
    /// Common internal interface shared between SharedDatabaseState and PythonTypeDatabase.
    /// 
    /// This interface enables splitting of the type database into two portions.  The first is our cached
    /// type database for an interpreter, its standard library, and all of site-packages.  The second
    /// portion is per-project cached intellisense - currently only used for caching the intellisense
    /// against a referenced extension module (.pyd).
    /// 
    /// When 
    /// </summary>
    interface ITypeDatabaseReader {
        void ReadMember(string memberName, Dictionary<string, object> memberValue, Action<string, IMember> assign, IMemberContainer container);
        void LookupType(object type, Action<IPythonType> assign);
        string GetBuiltinTypeName(BuiltinTypeId id);
        void OnDatabaseCorrupt();

        bool BeginModuleLoad(IPythonModule module, int millisecondsTimeout);
        void EndModuleLoad(IPythonModule module);
    }
#endif
}
