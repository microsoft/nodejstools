/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.NodejsTools;
using Microsoft.VisualStudio.Text;

namespace Microsoft.VisualStudio.Repl {
#if INTERACTIVE_WINDOW
    using IReplCommand = IInteractiveWindowCommand;
    using IReplWindow = IInteractiveWindow;    
#endif

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

                if (target.Value != null) {
                    positions.Add(new KeyValuePair<int, ITextBuffer>(target.Value, buffer));
                }
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
            return buffer.ContentType.IsOfType(NodeConstants.JavaScript);
        }

        public object ButtonContent {
            get {
                return null;
            }
        }

        #endregion
    }
}
