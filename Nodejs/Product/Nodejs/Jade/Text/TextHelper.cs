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

using System.Text;

namespace Microsoft.NodejsTools.Jade {
    /// <summary>
    /// A helper class exposing various helper functions that 
    /// are used in formatting, smart indent and elsewhere else.
    /// </summary>
    static class TextHelper {
        public static string ConvertTabsToSpaces(string text, int tabSize, bool replaceNonWhitespaceChars = false) {
            var sb = new StringBuilder(text.Length);
            int charsSoFar = 0;

            for (int i = 0; i < text.Length; i++) {
                char ch = text[i];

                if (ch == '\t') {
                    var spaces = tabSize - (charsSoFar % tabSize);
                    sb.Append(' ', spaces);
                    charsSoFar = 0;
                } else if (ch == '\r' || ch == '\n') {
                    charsSoFar = 0;
                    sb.Append(ch);
                } else {
                    charsSoFar++;
                    charsSoFar = charsSoFar % tabSize;
                    if (replaceNonWhitespaceChars) {
                        sb.Append(' ');
                    } else {
                        sb.Append(ch);
                    }
                }
            }

            return sb.ToString();
        }
    }
}
