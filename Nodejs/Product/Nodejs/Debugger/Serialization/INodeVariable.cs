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
    /// Defines an interface for a variable.
    /// </summary>
    internal interface INodeVariable
    {
        /// <summary>
        /// Gets a variable identifier.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Gets a parent variable.
        /// </summary>
        NodeEvaluationResult Parent { get; }

        /// <summary>
        /// Gets or sets a stack frame.
        /// </summary>
        NodeStackFrame StackFrame { get; }

        /// <summary>
        /// Gets a variable name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets a variable type name.
        /// </summary>
        string TypeName { get; }

        /// <summary>
        /// Gets a variable value.
        /// </summary>
        string Value { get; }

        /// <summary>
        /// Gets a variable class.
        /// </summary>
        string Class { get; }

        /// <summary>
        /// Gets a variable text.
        /// </summary>
        string Text { get; }

        /// <summary>
        /// Gets a variable attributes.
        /// </summary>
        NodePropertyAttributes Attributes { get; }

        /// <summary>
        /// Gets a variable type.
        /// </summary>
        NodePropertyType Type { get; }
    }
}