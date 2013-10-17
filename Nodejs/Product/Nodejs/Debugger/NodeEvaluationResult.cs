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
using System.Threading;

namespace Microsoft.NodejsTools.Debugger {
    /// <summary>
    /// Represents the result of an evaluation of an expression against a given stack frame.
    /// </summary>
    class NodeEvaluationResult {
        private readonly string _expression, _typeName, _exceptionText, _childText;
        private string _objRepr, _hexRepr;
        private readonly NodeStackFrame _frame;
        private readonly NodeDebugger _process;
        private readonly bool _isExpandable, _childIsIndex, _childIsEnumerate;
        private readonly int? _handle;

        /// <summary>
        /// Creates a PythonObject for an expression which successfully returned a value.
        /// </summary>
        public NodeEvaluationResult(NodeDebugger process, int? handle, string objRepr, string hexRepr, string typeName, string expression, string childText, bool childIsIndex, bool childIsEnumerate, NodeStackFrame frame, bool isExpandable) {
            _handle = handle;
            _process = process;
            _expression = expression;
            _frame = frame;
            _objRepr = objRepr;
            _hexRepr = hexRepr;
            _typeName = typeName;
            _isExpandable = isExpandable;
            _childText = childText;
            _childIsIndex = childIsIndex;
            _childIsEnumerate = childIsEnumerate;
        }

        /// <summary>
        /// Creates a PythonObject for an expression which raised an exception instead of returning a value.
        /// </summary>
        public NodeEvaluationResult(NodeDebugger process, string exceptionText, string expression, NodeStackFrame frame) {
            _process = process;
            _expression = expression;
            _frame = frame;
            _exceptionText = exceptionText;
        }

        /// <summary>
        /// Returns true if this object is expandable.  
        /// </summary>
        public bool IsExpandable {
            get {
                return _isExpandable;
            }
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
            if (!IsExpandable) {
                return null;
            }

            AutoResetEvent childrenEnumed = new AutoResetEvent(false);
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
        public string StringRepr {
            get {
                return _objRepr;
            }
            set {
                _objRepr = value;
            }
        }

        /// <summary>
        /// Gets the string representation of this evaluation in hexadecimal or null if the hex value was not computable.
        /// </summary>
        public string HexRepr {
            get {
                return _hexRepr;
            }
            set {
                _hexRepr = value;
            }
        }

        /// <summary>
        /// Gets the type name of the result of this evaluation or null if an exception was thrown.
        /// </summary>
        public string TypeName {
            get {
                return _typeName;
            }
        }

        /// <summary>
        /// Gets the text of the exception which was thrown when evaluating this expression, or null
        /// if no exception was thrown.
        /// </summary>
        public string ExceptionText {
            get {
                return _exceptionText;
            }
        }

        /// <summary>
        /// Gets the expression which was evaluated to return this object.
        /// </summary>
        public string Expression {
            get {
                if (!String.IsNullOrEmpty(_childText)) {
                    if (_childIsIndex) {
                        return _expression + _childText;
                    } else {
                        return _expression + "." + _childText;
                    }
                }

                return _expression;
            }
        }

        public string ChildText {
            get {
                return _childText;
            }
        }

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

        public bool IsArray {
            get {
                return ((_handle != null) && (string.Compare(_objRepr, "Array") == 0));
            }
        }

        public NodeDebugger Process { get { return _process; } }

        public bool ChildIsIndex { get { return _childIsIndex;  } }
    }
}
