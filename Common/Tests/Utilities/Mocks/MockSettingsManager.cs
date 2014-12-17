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
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace TestUtilities.Mocks {
    public class MockSettingsManager : IVsSettingsManager {
        public readonly MockSettingsStore ReadOnlyStore = new MockSettingsStore();

        public int GetApplicationDataFolder(uint folder, out string folderPath) {
            throw new NotImplementedException();
        }

        public int GetCollectionScopes(string collectionPath, out uint scopes) {
            throw new NotImplementedException();
        }

        public int GetCommonExtensionsSearchPaths(uint paths, string[] commonExtensionsPaths, out uint actualPaths) {
            throw new NotImplementedException();
        }

        public int GetPropertyScopes(string collectionPath, string propertyName, out uint scopes) {
            throw new NotImplementedException();
        }

        public int GetReadOnlySettingsStore(uint scope, out IVsSettingsStore store) {
            store = ReadOnlyStore;
            return VSConstants.S_OK;
        }

        public int GetWritableSettingsStore(uint scope, out IVsWritableSettingsStore writableStore) {
            throw new NotImplementedException();
        }
    }
}
