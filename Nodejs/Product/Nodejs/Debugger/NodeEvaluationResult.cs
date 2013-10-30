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
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;

namespace Microsoft.NodejsTools.Debugger {
    /// <summary>
    /// Represents the result of an evaluation of an expression against a given stack frame.
    /// </summary>
    class NodeEvaluationResult {
        private readonly NodeStackFrame _frame;
        private readonly NodeDebugger _process;
        private readonly int? _handle;
        private readonly Regex _stringLengthExpression = new Regex(@"\.\.\. \(length: ([0-9]+)\)$", RegexOptions.Compiled);

        /// <summary>
        /// Creates an evaluation result for an expression which successfully returned a value.
        /// </summary>
        public NodeEvaluationResult(NodeDebugger process, int? handle, string stringValue, string hexValue, string typeName, string expression, string fullName, NodeExpressionType type,
            NodeStackFrame frame) {
            _handle = handle;
            _process = process;
            _frame = frame;
            Expression = expression;
            StringValue = stringValue;
            HexValue = hexValue;
            TypeName = typeName;
            FullName = fullName;
            Type = type;
        }

        /// <summary>
        /// Creates an evaluation result for an expression which raised an exception instead of returning a value.
        /// </summary>
        public NodeEvaluationResult(NodeDebugger process, string exceptionText, string expression, NodeStackFrame frame) {
            _process = process;
            _frame = frame;
            Expression = expression;
            ExceptionText = exceptionText;
        }

        /// <summary>
        /// Gets the list of children which this object contains.  The children can be either
        /// members (x.foo, x.bar) or they can be indexes (x[0], x[1], etc...).  Calling this
        /// causes the children to be determined by communicating with the debuggee.  These
        /// objects can then later be evaluated.  The names returned here are in the form of
        /// "foo" or "0" so they need additional work to append onto this expression.
        /// 
        /// Returns null if the object is not expandable.
        /// </summary>
        public NodeEvaluationResult[] GetChildren(int timeOut) {
            if (!Type.HasFlag(NodeExpressionType.Expandable)) {
                return null;
            }

            var childrenEnumed = new AutoResetEvent(false);
            NodeEvaluationResult[] res = null;

            Debug.Assert(Handle.HasValue);
            _process.EnumChildren(this, children => {
                res = children;
                childrenEnumed.Set();
            });

            while (!_frame.Thread.Process.HasExited && !childrenEnumed.WaitOne(Math.Min(timeOut, 100))) {
                if (timeOut <= 100) {
                    break;
                }
                timeOut -= 100;
            }

            return res;
        }

        /// <summary>
        /// Gets the string representation of this evaluation or null if an exception was thrown.
        /// </summary>
        public string StringValue { get; private set; }

        /// <summary>
        /// Gets the string representation length.
        /// </summary>
        public int StringLength {
            get { return GetStringLength(StringValue); }
        }

        /// <summary>
        /// Gets the string representation of this evaluation in hexadecimal or null if the hex value was not computable.
        /// </summary>
        public string HexValue { get; private set; }

        /// <summary>
        /// Gets the type name of the result of this evaluation or null if an exception was thrown.
        /// </summary>
        public string TypeName { get; private set; }

        /// <summary>
        /// Gets the text of the exception which was thrown when evaluating this expression, or null
        /// if no exception was thrown.
        /// </summary>
        public string ExceptionText { get; private set; }

        /// <summary>
        /// Gets the expression text representation.
        /// </summary>
        public string Expression { get; private set; }

        /// <summary>
        /// Gets the expression which was evaluated to return this object.
        /// </summary>
        public string FullName { get; private set; }

        /// <summary>
        /// Gets a type metadata for the expression.
        /// </summary>
        public NodeExpressionType Type { get; set; }

        /// <summary>
        /// Returns the stack frame in which this expression was evaluated.
        /// </summary>
        public NodeStackFrame Frame {
            get {
                return _frame;
            }
        }

        /// <summary>
        /// Returns the handle for this evaluation result.
        /// </summary>
        public int? Handle {
            get {
                return _handle;
            }
        }

        public NodeDebugger Process {
            get { return _process; }
        }

        private int GetStringLength(string stringValue) {
            if (string.IsNullOrEmpty(stringValue)) {
                return 0;
            }

            Match match = _stringLengthExpression.Match(stringValue);
            if (!match.Success) {
                return stringValue.Length;
            }

            return int.Parse(match.Groups[1].Value);
        }
    }
}
