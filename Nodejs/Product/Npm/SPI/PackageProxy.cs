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

namespace Microsoft.NodejsTools.Npm.SPI {
    internal class PackageProxy : IPackage {
        public INodeModules Modules { get; internal set; }

        public IPackageJson PackageJson { get; internal set; }

        public bool HasPackageJson { get; internal set; }

        public string Name { get; internal set; }

        public SemverVersion Version { get; internal set; }

        public IEnumerable<SemverVersion> AvailableVersions { get; internal set; }

        public IPerson Author { get; internal set; }

        public string Description { get; internal set; }

        public IEnumerable<string> Homepages { get; internal set; }

        public string Path { get; internal set; }

        public string PublishDateTimeString { get; internal set; }

        public string RequestedVersionRange { get; internal set; }

        public IEnumerable<string> Keywords { get; internal set; }

        public bool IsListedInParentPackageJson { get; internal set; }

        public bool IsMissing { get; internal set; }

        public bool IsDependency { get; internal set; }

        public bool IsDevDependency { get; internal set; }

        public bool IsOptionalDependency { get; internal set; }

        public bool IsBundledDependency { get; internal set; }

        public PackageFlags Flags { get; internal set; }
    }
}