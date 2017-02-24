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

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.NodejsTools.Jade
{
    /// <summary>
    /// Generic tokenizer
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Tokenizer")]
    internal interface ITokenizer<T> where T : ITextRange
    {
        /// <summary>
        /// Tokenize text from a given provider
        /// </summary>
        /// <param name="textProvider">Text provider</param>
        /// <param name="start">Start position</param>
        /// <param name="length">Length of fragent to tokenize</param>
        /// <param name="excludePartialTokens">True if tokenizeer should exclude partial token that may intersect end of the specified span</param>
        /// <returns>Collection of tokens</returns>
        ReadOnlyTextRangeCollection<T> Tokenize(ITextProvider textProvider, int start, int length, bool excludePartialTokens);

        /// <summary>
        /// Tokenize text from a given provider
        /// </summary>
        /// <param name="textProvider">Text provider</param>
        /// <param name="start">Start position</param>
        /// <param name="length">Length of fragent to tokenize</param>
        /// <returns>Collection of tokens</returns>
        ReadOnlyTextRangeCollection<T> Tokenize(ITextProvider textProvider, int start, int length);
    }
}
