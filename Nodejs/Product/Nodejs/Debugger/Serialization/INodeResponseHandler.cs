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

namespace Microsoft.NodejsTools.Debugger.Serialization {
    /// <summary>
    /// Defines an interface of node message handler.
    /// </summary>
    interface INodeResponseHandler {
        /// <summary>
        /// Handles backtrace response message.
        /// </summary>
        /// <param name="thread">Thread.</param>
        /// <param name="message">Message.</param>
        /// <returns>Array of stack frames.</returns>
        void ProcessBacktrace(NodeThread thread, JsonValue message, Action<NodeStackFrame[]> successHandler);

        /// <summary>
        /// Handles evaluate response message.
        /// </summary>
        /// <param name="stackFrame">Stack frame.</param>
        /// <param name="expression">Expression.</param>
        /// <param name="message">Message.</param>
        /// <returns>Evaluation result.</returns>
        NodeEvaluationResult ProcessEvaluate(NodeStackFrame stackFrame, string expression, JsonValue message);

        /// <summary>
        /// Handles lookup response message.
        /// </summary>
        /// <param name="parent">Parent variable.</param>
        /// <param name="message">Message.</param>
        /// <returns>Array of evaluation results.</returns>
        NodeEvaluationResult[] ProcessLookup(NodeEvaluationResult parent, JsonValue message);
    }
}