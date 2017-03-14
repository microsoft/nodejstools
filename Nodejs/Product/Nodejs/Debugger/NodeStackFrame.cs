// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudioTools.Project;

namespace Microsoft.NodejsTools.Debugger
{
    internal sealed class NodeStackFrame
    {
        private readonly int _frameId;

        public NodeStackFrame(int frameId)
        {
            this._frameId = frameId;
        }

        /// <summary>
        /// The line number where the current function/class/module starts
        /// </summary>
        public int StartLine => this.Line;
        /// <summary>
        /// The line number where the current function/class/module ends.
        /// </summary>
        public int EndLine => this.Line;
        /// <summary>
        /// Gets a thread which executes stack frame.
        /// </summary>
        public NodeDebugger Process { get; set; }

        /// <summary>
        /// Gets a stack frame line number in the script.
        /// </summary>
        public int Line { get; set; }

        /// <summary>
        /// Gets a stack frame column number in the script.
        /// </summary>
        public int Column { get; set; }

        /// <summary>
        /// Gets a stack name.
        /// </summary>
        public string FunctionName { get; set; }

        /// <summary>
        /// Gets a script file name which holds a code segment of the frame.
        /// </summary>
        public string FileName => this.Module != null ? this.Module.FileName : null;
        /// <summary>
        /// Gets a script which holds a code segment of the frame.
        /// </summary>
        public NodeModule Module { get; set; }

        /// <summary>
        /// Gets the ID of the frame.  Frame 0 is the currently executing frame, 1 is the caller of the currently executing frame,
        /// etc...
        /// </summary>
        public int FrameId => this._frameId;
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
        public bool TryParseText(string text, out string errorMsg)
        {
#if NEEDS_UPDATING
            CollectingErrorSink errorSink = new CollectingErrorSink();
            Parser parser = Parser.CreateParser(new StringReader(text), _debugger.LanguageVersion, new ParserOptions() { ErrorSink = errorSink });
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
        /// Executes the given text against this stack frame. Throws on failure.
        /// </summary>
        /// <param name="text">Text expression.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public Task<NodeEvaluationResult> ExecuteTextAsync(string text, CancellationToken cancellationToken = new CancellationToken())
        {
            Utilities.CheckNotNull(this.Process);

            return this.Process.ExecuteTextAsync(this, text, cancellationToken);
        }

        /// <summary>
        /// Sets a new value for variable in this stack frame.
        /// </summary>
        /// <param name="name">Variable name.</param>
        /// <param name="value">New value.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public async Task<NodeEvaluationResult> SetVariableValueAsync(string name, string value, CancellationToken cancellationToken = new CancellationToken())
        {
            Utilities.CheckNotNull(this.Process);

            var result = await this.Process.SetVariableValueAsync(this, name, value, cancellationToken).ConfigureAwait(false);

            if (result == null)
            {
                return null;
            }

            // Update variable in locals
            for (var i = 0; i < this.Locals.Count; i++)
            {
                var evaluationResult = this.Locals[i];
                if (evaluationResult.Expression == name)
                {
                    this.Locals[i] = result;
                }
            }

            // Update variable in parameters
            for (var i = 0; i < this.Parameters.Count; i++)
            {
                var evaluationResult = this.Parameters[i];
                if (evaluationResult.Expression == name)
                {
                    this.Parameters[i] = result;
                }
            }

            return result;
        }
    }
}

