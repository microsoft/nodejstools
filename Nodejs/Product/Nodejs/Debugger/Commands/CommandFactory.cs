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

using System.Collections.Generic;
using Microsoft.NodejsTools.Debugger.Serialization;

namespace Microsoft.NodejsTools.Debugger.Commands {
    sealed class CommandFactory : ICommandFactory {
        private readonly INumberGenerator _numberGenerator;
        private readonly IEvaluationResultFactory _resultFactory;

        public CommandFactory(INumberGenerator numberGenerator, IEvaluationResultFactory resultFactory) {
            _numberGenerator = numberGenerator;
            _resultFactory = resultFactory;
        }

        public BacktraceCommand CreateBacktraceCommand(int fromFrame, int toFrame, NodeThread thread = null, Dictionary<int, NodeModule> modules = null) {
            return new BacktraceCommand(_numberGenerator.GetNext(), _resultFactory, fromFrame, toFrame, thread, modules);
        }

        public ChangeBreakpointCommand CreateChangeBreakpointCommand(int breakpointId, bool? enabled = null, string condition = null, int? ignoreCount = null) {
            return new ChangeBreakpointCommand(_numberGenerator.GetNext(), breakpointId, enabled, condition, ignoreCount);
        }

        public ClearBreakpointCommand CreateClearBreakpointsCommand(int breakpointId) {
            return new ClearBreakpointCommand(_numberGenerator.GetNext(), breakpointId);
        }

        public ContinueCommand CreateContinueCommand(SteppingKind stepping, int stepCount = 1) {
            return new ContinueCommand(_numberGenerator.GetNext(), stepping, stepCount);
        }

        public DisconnectCommand CreateDisconnectCommand() {
            return new DisconnectCommand(_numberGenerator.GetNext());
        }

        public EvaluateCommand CreateEvaluateCommand(string expression, NodeStackFrame stackFrame = null) {
            return new EvaluateCommand(_numberGenerator.GetNext(), _resultFactory, expression, stackFrame);
        }

        public ListBreakpointsCommand CreateListBreakpointsCommand() {
            return new ListBreakpointsCommand(_numberGenerator.GetNext());
        }

        public LookupCommand CreateLookupCommand(List<NodeEvaluationResult> parents) {
            return new LookupCommand(_numberGenerator.GetNext(), _resultFactory, parents);
        }

        public LookupCommand CreateLookupCommand(int[] handles) {
            return new LookupCommand(_numberGenerator.GetNext(), _resultFactory, handles);
        }

        public ScriptsCommand CreateScriptsCommand(bool includeSource = false, int? moduleId = null) {
            return new ScriptsCommand(_numberGenerator.GetNext(), includeSource, moduleId);
        }

        public SetBreakpointCommand CreateSetBreakpointCommand(NodeModule module, NodeBreakpoint breakpoint, bool withoutPredicate = false) {
            return new SetBreakpointCommand(_numberGenerator.GetNext(), module, breakpoint, withoutPredicate);
        }

        public SetExceptionBreakCommand CreateSetExceptionBreakCommand(bool uncaughtExceptions, bool enabled) {
            return new SetExceptionBreakCommand(_numberGenerator.GetNext(), uncaughtExceptions, enabled);
        }

        public SuspendCommand CreateSuspendCommand() {
            return new SuspendCommand(_numberGenerator.GetNext());
        }
    }
}