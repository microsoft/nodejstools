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

using Microsoft.VisualStudio.Text;

namespace Microsoft.NodejsTools.Jade
{
    internal partial class JadeTokenizer : Tokenizer<JadeToken>
    {
        private void OnScript(int blockIndent)
        {
            if (_jsTagger != null)
            {
                int start = _cs.Position;

                SkipToEndOfBlock(blockIndent, text: false);

                int end = _cs.Position;
                int length = end - start;

                if (length > 0)
                {
                    _jsBuffer.Replace(
                        new Span(0, _jsBuffer.CurrentSnapshot.Length),
                        _cs.Text.GetText(new TextRange(start, length))
                    );

                    var tokens = _jsTagger.GetTags(new NormalizedSnapshotSpanCollection(new SnapshotSpan(_jsBuffer.CurrentSnapshot, 0, _jsBuffer.CurrentSnapshot.Length)));

                    foreach (var t in tokens)
                    {
                        AddToken(t.Tag.ClassificationType, t.Span.Start.Position + start, t.Span.Length);
                    }
                }
            }
        }
    }
}