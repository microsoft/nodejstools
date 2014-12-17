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

namespace Microsoft.VisualStudioTools.Project {
    public interface IPublishProject {
        /// <summary>
        /// Gets the list of files which need to be published.
        /// </summary>
        IList<IPublishFile> Files {
            get;
        }

        /// <summary>
        /// Gets the root directory of the project.
        /// </summary>
        string ProjectDir {
            get;
        }

        /// <summary>
        /// Gets or sets the progress of the publishing.
        /// </summary>
        int Progress {
            get;
            set;
        }
    }
}
