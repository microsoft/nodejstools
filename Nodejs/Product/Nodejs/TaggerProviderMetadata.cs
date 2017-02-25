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

namespace Microsoft.NodejsTools
{
    /// <summary>
    /// Just used for our MEF import to get the metadata in a strongly
    /// typed way.
    /// </summary>
    internal sealed class TaggerProviderMetadata
    {
        public readonly IEnumerable<string> ContentTypes;
        public readonly IEnumerable<Type> TagTypes;

        public TaggerProviderMetadata(IDictionary<string, object> values)
        {
            this.ContentTypes = (IEnumerable<string>)values["ContentTypes"];
            this.TagTypes = (IEnumerable<Type>)values["TagTypes"];
        }
    }
}
