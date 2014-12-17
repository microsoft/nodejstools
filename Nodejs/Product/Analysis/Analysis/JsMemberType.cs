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

namespace Microsoft.NodejsTools.Analysis {
    /// <summary>
    /// Indicates the type of a variable result lookup.
    /// 
    /// These types are generally closely tied to the types which exist
    /// in the JavaScript type system, but we can introduce new types 
    /// which may onto higher level concepts.  For example we have a 
    /// concept of a module type which we use for indicating that
    /// the object is a Node.js module.  It can also include concepts
    /// such as keywords which we include in completions.
    /// </summary>
    public enum JsMemberType {
        Unknown,
        Object,
        Undefined,
        Null,
        Boolean,
        Number,
        String,
        Function,
        /// <summary>
        /// An instance of a built-in or user defined module
        /// </summary>
        Module,
        Multiple,
        Keyword,
    }
}
