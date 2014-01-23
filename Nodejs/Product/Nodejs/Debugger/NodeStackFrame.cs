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

namespace Microsoft.NodejsTools.Debugger {
    class NodeStackFrame {
        private readonly string _frameName;
        private readonly NodeThread _thread;
        private readonly NodeModule _module;
        private readonly int _startLine, _endLine, _lineNo;

        public NodeStackFrame(NodeThread thread, NodeModule module, string frameName, int startLine, int endLine, int lineNo, int frameId) {
            _thread = thread;
            _module = module;
            _frameName = frameName;
            _lineNo = lineNo;
            FrameId = frameId;
            _startLine = startLine;
            _endLine = endLine;
        }

        /// <summary>
        /// The line nubmer where the current function/class/module starts
        /// </summary>
        public int StartLine {
            get {
                return MapLineNo(_startLine);
            }
        }

        /// <summary>
        /// The line number where the current function/class/module ends.
        /// </summary>
        public int EndLine {
            get {
                return MapLineNo(_endLine);
            }
        }

        /// <summary>
        /// Maps a line number from JavaScript to the original source code.
        /// 
        /// Line numbers are 1 based.
        /// </summary>
        /// <param name="lineNo"></param>
        /// <returns></returns>
        private int MapLineNo(int lineNo) {
            var mapping = Thread.Process.MapToOriginal(Module.JavaScriptFileName, lineNo - 1);
            if (mapping != null) {
                return mapping.Line + 1;
            }
            return lineNo;
        }

        /// <summary>
        /// Gets a thread which executes stack frame.
        /// </summary>
        public NodeThread Thread {
            get { return _thread; }
        }

        /// <summary>
        /// Gets a stack frame line number in the script.
        /// </summary>
        public int LineNo {
            get {
                return MapLineNo(_lineNo);
            }
        }

        /// <summary>
        /// Gets a stack name.
        /// </summary>
        public string FunctionName {
            get {
                var mapping = Thread.Process.MapToOriginal(Module.JavaScriptFileName, _lineNo - 1);
                if (mapping != null) {
                    return mapping.Name;
                }
                return _frameName;
            }
        }

        /// <summary>
        /// Gets a script file name which holds a code segment of the frame.
        /// </summary>
        public string FileName {
            get {
                return _module.FileName;
            }
        }

        /// <summary>
        /// Gets a script which holds a code segment of the frame.
        /// </summary>
        public NodeModule Module {
            get {
                return _module;
            }
        }

        /// <summary>
        /// Gets the ID of the frame.  Frame 0 is the currently executing frame, 1 is the caller of the currently executing frame, etc...
        /// </summary>
        public int FrameId {
            get; private set;
        }

        /// <summary>
        /// Gets or sets a local variables of the frame.
        /// </summary>
        public IList<NodeEvaluationResult> Locals { get; set; }

        /// <summary>
        /// Gets or sets an arguments of the frame.
        /// </summary>
        public IList<NodeEvaluationResult> Parameters { get; set; }

        /// <summary>
        /// Attempts to parse the given text.  Returns true if the text is a valid expression.  Returns false if the text is not
        /// a valid expression and assigns the error messages produced to errorMsg.
        /// </summary>
        public virtual bool TryParseText(string text, out string errorMsg) {
#if NEEDS_UPDATING
            CollectingErrorSink errorSink = new CollectingErrorSink();
            Parser parser = Parser.CreateParser(new StringReader(text), _thread.Process.LanguageVersion, new ParserOptions() { ErrorSink = errorSink });
            var ast = parser.ParseSingleStatement();
            if (errorSink.Errors.Count > 0) {
                StringBuilder msg = new StringBuilder();
                foreach (var error in errorSink.Errors) {
                    msg.Append(error.Message);
                    msg.Append(Environment.NewLine);
                }

                errorMsg = msg.ToString();
                return false;
            }
#endif

            errorMsg = null;
            return true;
        }

        /// <summary>
        /// Executes the given text against this stack frame.
        /// </summary>
        /// <param name="text">Text expression.</param>
        /// <param name="completion">Completion callback.</param>
        public virtual void ExecuteText(string text, Action<NodeEvaluationResult> completion) {
            _thread.Process.ExecuteText(text, this, completion);
        }

        /// <summary>
        /// Sets the line number that this current frame is executing.  Returns true
        /// if the line was successfully set or false if the line number cannot be changed
        /// to this line.
        /// </summary>
        public bool SetLineNumber(int lineNo) {
            return _thread.Process.SetLineNumber(this, lineNo);
        }
    }
}
