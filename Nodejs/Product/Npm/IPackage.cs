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

using System.Collections;
using System.Collections.Generic;

namespace Microsoft.NodejsTools.Npm {
    public interface IPackage : IRootPackage {
        string PublishDateTimeString { get; }

        string RequestedVersionRange { get; }

        IEnumerable<string> Keywords { get; }

        IEnumerable<SemverVersion> AvailableVersions { get; }

        bool IsListedInParentPackageJson { get; }

        bool IsMissing { get; }

        bool IsDependency { get; }

        bool IsDevDependency { get; }

        bool IsOptionalDependency { get; }

        bool IsBundledDependency { get; }

        PackageFlags Flags { get; }
    }
}