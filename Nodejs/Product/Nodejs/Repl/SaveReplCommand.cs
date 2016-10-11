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
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;

namespace Microsoft.NodejsTools.Repl {
    [Export(typeof(IReplCommand))]
    class SaveReplCommand : IReplCommand {
        #region IReplCommand Members

        public Task<ExecutionResult> Execute(IReplWindow window, string arguments) {
            if(String.IsNullOrWhiteSpace(arguments)) {
                window.WriteError("save requires a filename");
                return ExecutionResult.Failed;
            }else if(arguments.IndexOfAny(Path.GetInvalidPathChars()) != -1) {
                window.WriteError(String.Format("Invalid filename: {0}", arguments));
                return ExecutionResult.Failed;
            }

            StringBuilder text = new StringBuilder();

            List<KeyValuePair<int, ITextBuffer>> positions = new List<KeyValuePair<int, ITextBuffer>>();
            foreach (var buffer in window.TextView.BufferGraph.GetTextBuffers(IsJavaScriptBuffer)) {
                var target = window.TextView.BufferGraph.MapUpToBuffer(
                    new SnapshotPoint(buffer.CurrentSnapshot, 0),
                    PointTrackingMode.Positive,
                    PositionAffinity.Successor,
                    window.TextView.TextBuffer
                );

                positions.Add(new KeyValuePair<int, ITextBuffer>(target.Value, buffer));
            }

            positions.Sort((x, y) => x.Key.CompareTo(y.Key));
            foreach (var buffer in positions) {
                var bufferText = buffer.Value.CurrentSnapshot.GetText();
                if (!bufferText.StartsWith(".")) {
                    text.Append(bufferText);
                    text.Append(Environment.NewLine);
                }
            }

            try {
                File.WriteAllText(arguments, text.ToString());
                window.WriteLine(String.Format("Session saved to: {0}", arguments));
            } catch {
                window.WriteError(String.Format("Failed to save: {0}", arguments));
            }
            return ExecutionResult.Succeeded;
        }

        public string Description {
            get { return "Save the current REPL session to a file"; }
        }

        public string Command {
            get { return "save"; }
        }

        private static bool IsJavaScriptBuffer(ITextBuffer buffer) {
            return buffer.ContentType.IsOfType(NodejsConstants.JavaScript);
        }

        public object ButtonContent {
            get {
                return null;
            }
        }

        #endregion
    }
}
