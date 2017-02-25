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
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Debugger
{
    /// <summary>
    /// Represents the result of an evaluation of an expression against a given stack frame.
    /// </summary>
    internal class NodeEvaluationResult
    {
        private readonly Regex _stringLengthExpression = new Regex(@"\.\.\. \(length: ([0-9]+)\)$", RegexOptions.Compiled);

        /// <summary>
        /// Creates an evaluation result for an expression which successfully returned a value.
        /// </summary>
        public NodeEvaluationResult(int handle, string stringValue, string hexValue, string typeName, string expression, string fullName, NodeExpressionType type, NodeStackFrame frame)
        {
            this.Handle = handle;
            this.Frame = frame;
            this.Expression = expression;
            this.StringValue = stringValue;
            this.HexValue = hexValue;
            this.TypeName = typeName;
            this.FullName = fullName;
            this.Type = type;
        }

        /// <summary>
        /// Gets the string representation of this evaluation or null if an exception was thrown.
        /// </summary>
        public string StringValue { get; set; }

        /// <summary>
        /// Gets the string representation length.
        /// </summary>
        public int StringLength => GetStringLength(this.StringValue);
        /// <summary>
        /// Gets the string representation of this evaluation in hexadecimal or null if the hex value was not computable.
        /// </summary>
        public string HexValue { get; set; }

        /// <summary>
        /// Gets the type name of the result of this evaluation or null if an exception was thrown.
        /// </summary>
        public string TypeName { get; private set; }

        /// <summary>
        /// Gets the expression text representation.
        /// </summary>
        public string Expression { get; private set; }

        /// <summary>
        /// Gets the expression which was evaluated to return this object.
        /// </summary>
        public string FullName { get; private set; }

        /// <summary>
        /// Gets a type metadata for the expression.
        /// </summary>
        public NodeExpressionType Type { get; private set; }

        /// <summary>
        /// Returns the stack frame in which this expression was evaluated.
        /// </summary>
        public NodeStackFrame Frame { get; private set; }

        /// <summary>
        /// Returns the handle for this evaluation result.
        /// </summary>
        public int Handle { get; private set; }

        /// <summary>
        /// Gets the list of children which this object contains.  The children can be either
        /// members (x.foo, x.bar) or they can be indexes (x[0], x[1], etc...).  Calling this
        /// causes the children to be determined by communicating with the debuggee.  These
        /// objects can then later be evaluated.  The names returned here are in the form of
        /// "foo" or "0" so they need additional work to append onto this expression.
        /// Returns null if the object is not expandable.
        /// </summary>
        public async Task<List<NodeEvaluationResult>> GetChildrenAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            if (!this.Type.HasFlag(NodeExpressionType.Expandable))
            {
                return null;
            }

            return await this.Frame.Process.EnumChildrenAsync(this, cancellationToken).ConfigureAwait(false);
        }

        private int GetStringLength(string stringValue)
        {
            if (string.IsNullOrEmpty(stringValue))
            {
                return 0;
            }

            var match = this._stringLengthExpression.Match(stringValue);
            if (!match.Success)
            {
                return stringValue.Length;
            }

            return int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
        }
    }
}