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

using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.NodejsTools.Jade {
    /// <summary>
    /// Exports the Jade content type and file extension
    /// </summary>
    class JadeContentTypeDefinition {
        public const string JadeLanguageName = "Jade";
        public const string JadeContentType = "jade";
        public const string JadeFileExtension = ".jade";
        public const string PugFileExtension = ".pug";

        /// <summary>
        /// Exports the Jade content type
        /// </summary>
        [Export(typeof(ContentTypeDefinition))]
        [Name(JadeContentType)]
        [BaseDefinition("text")]
        public ContentTypeDefinition IJadeContentType { get; set; }

        /// <summary>
        /// Exports the Jade file extension
        /// </summary>
        [Export(typeof(FileExtensionToContentTypeDefinition))]
        [ContentType(JadeContentType)]
        [FileExtension(JadeFileExtension)]
        public FileExtensionToContentTypeDefinition IJadeFileExtension { get; set; }

        /// <summary>
        /// Exports the Pug file extension
        /// </summary>
        [Export(typeof(FileExtensionToContentTypeDefinition))]
        [ContentType(JadeContentType)]
        [FileExtension(PugFileExtension)]
        public FileExtensionToContentTypeDefinition IPugFileExtension { get; set; }
    }
}
