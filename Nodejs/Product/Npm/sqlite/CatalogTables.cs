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
using SQLite;

namespace Microsoft.NodejsTools.Npm.SQLiteTables {
    public class CatalogEntry {
        public string Name { get; set; }

        public  string Description { get; set; }

        public  string Homepage { get; set; }

        public string Version { get; set; }

        public string AvailableVersions { get; set; }

        public string Author { get; set; }

        public string Keywords { get; set; }

        public string PublishDateTimeString { get; set; }
    }

    public class DbVersion {
        [PrimaryKey, NotNull]
        public int Id { get; set; }
    }

    public class RegistryFileMapping {
        [PrimaryKey, NotNull, Indexed]
        public string RegistryUrl { get; set; }

        [NotNull, Unique]
        public string DbFileLocation { get; set; }
    }

    public class RegistryInfo {
        [PrimaryKey, NotNull, Indexed]
        public string RegistryUrl { get; set; }

        [NotNull, Unique]
        public long Revision { get; set; }

        [NotNull]
        public DateTime UpdatedOn { get; set; }
    } 
}
