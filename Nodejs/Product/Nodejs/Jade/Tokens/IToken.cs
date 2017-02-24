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

namespace Microsoft.NodejsTools.Jade
{
    /// <summary>
    /// Describes a parse token. Parse token is a text range
    /// with a type that describes nature of the range.
    /// Derives from <seealso cref="ITextRange"/>
    /// </summary>
    /// <typeparam name="T">Token type (typically enum)</typeparam>
    internal interface IToken<T> : ITextRange
    {
        /// <summary>
        /// Type of the token
        /// </summary>
        T TokenType { get; }
    }
}
