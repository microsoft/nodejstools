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

namespace Microsoft.NodejsTools.Debugger.Serialization
{
    /// <summary>
    /// Defines a node property type.
    /// </summary>
    internal enum NodePropertyType
    {
        Normal = 0,
        Field = 1,
        Constant = 2,
        Callbacks = 3,
        Handler = 4,
        Interceptor = 5,
        Transition = 6,
        Nonexistent = 7
    }
}