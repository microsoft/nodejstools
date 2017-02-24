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

using Microsoft.VisualStudio.Text.Classification;

namespace Microsoft.NodejsTools.Jade
{
    internal class JadeToken : Token<JadeTokenType>
    {
        public readonly IClassificationType Classification;

        public JadeToken(JadeTokenType type, int start, int length)
            : base(type, start, length)
        {
        }

        public JadeToken(JadeTokenType type, IClassificationType classification, int start, int length)
            : base(type, start, length)
        {
            Classification = classification;
        }

        public override bool IsComment
        {
            get { return TokenType == JadeTokenType.Comment; }
        }

        public override bool IsString
        {
            get { return TokenType == JadeTokenType.String; }
        }
    }
}
