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
using System.Diagnostics;

namespace Microsoft.NodejsTools.Jade
{
    internal partial class JadeTokenizer : Tokenizer<JadeToken>
    {
        // plain text with possible #{foo} variable references
        private void OnHtml()
        {
            Debug.Assert(this._cs.CurrentChar == '<' && (this._cs.NextChar == '/' || Char.IsLetter(this._cs.NextChar)));

            int length = this._cs.NextChar == '/' ? 2 : 1;
            AddToken(JadeTokenType.AngleBracket, this._cs.Position, length);
            this._cs.Advance(length);

            var range = GetAttribute();
            if (range.Length > 0)
                AddToken(JadeTokenType.TagName, range.Start, range.Length);

            OnAttributes('>');

            if (this._cs.LookAhead(-1) == '>')
                AddToken(JadeTokenType.AngleBracket, this._cs.Position - 1, 1);
        }
    }
}
