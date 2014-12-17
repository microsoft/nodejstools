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

namespace Microsoft.VisualStudioTools.Project {
    /// <summary>
    /// Implements a publisher which handles publishing the list of files to a destination.
    /// </summary>
    public interface IProjectPublisher {
        /// <summary>
        /// Publishes the files listed in the given project to the provided URI.
        /// 
        /// This function should return when publishing is complete or throw an exception if publishing fails.
        /// </summary>
        /// <param name="project">The project to be published.</param>
        /// <param name="destination">The destination URI for the project.</param>
        void PublishFiles(IPublishProject project, Uri destination);

        /// <summary>
        /// Gets a localized description of the destination type (web site, file share, etc...)
        /// </summary>
        string DestinationDescription {
            get;
        }

        /// <summary>
        /// Gets the schema supported by this publisher - used to select which publisher will
        /// be used based upon the schema of the Uri provided by the user.
        /// </summary>
        string Schema {
            get;
        }
    }
}
