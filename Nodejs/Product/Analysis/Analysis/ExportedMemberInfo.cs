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


namespace Microsoft.NodejsTools.Analysis {
    /// <summary>
    /// Provides information about a value exported from a module.
    /// </summary>
    internal struct ExportedMemberInfo {
        private readonly string _name;
        private readonly bool _isDefinedInModule;

        internal ExportedMemberInfo(string name, bool isDefinedInModule) {
            _name = name;
            _isDefinedInModule = isDefinedInModule;
        }

        /// <summary>
        /// The name of the value being exported, fully qualified with the module/package name.
        /// </summary>
        public string Name {
            get {
                return _name;
            }
        }

        /// <summary>
        /// True if this was defined in the module or false if this was defined in another module
        /// but imported in the module that we're getting members from.
        /// </summary>
        public bool IsDefinedInModule {
            get {
                return _isDefinedInModule;
            }
        }
    }
}
