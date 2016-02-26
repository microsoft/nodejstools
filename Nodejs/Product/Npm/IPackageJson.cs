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
    public interface IPackageJson {
        string Name { get; }
        SemverVersion Version { get; }
        IScripts Scripts { get; }
        IPerson Author { get; }
        string Description { get; }
        IKeywords Keywords { get; }
        IHomepages Homepages { get; }
        IBugs Bugs { get; }
        ILicenses Licenses { get; }
        IFiles Files { get; }
        IMan Man { get; }
        IDependencies Dependencies { get; }
        IDependencies DevDependencies { get; }
        IBundledDependencies BundledDependencies { get; }
        IDependencies OptionalDependencies { get; }
        IDependencies AllDependencies { get; }
        IEnumerable<string> RequiredBy { get; }
    }
}