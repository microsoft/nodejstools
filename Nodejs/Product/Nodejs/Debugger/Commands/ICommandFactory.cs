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

namespace Microsoft.NodejsTools.Debugger.Commands {
    interface ICommandFactory {
        BacktraceCommand CreateBacktraceCommand(int fromFrame, int toFrame, NodeThread thread = null, Dictionary<int, NodeModule> modules = null);
        ChangeBreakpointCommand CreateChangeBreakpointCommand(int breakpointId, bool? enabled = null, string condition = null, int? ignoreCount = null);
        ClearBreakpointCommand CreateClearBreakpointsCommand(int breakpointId);
        ContinueCommand CreateContinueCommand(SteppingKind stepping, int stepCount = 1);
        DisconnectCommand CreateDisconnectCommand();
        EvaluateCommand CreateEvaluateCommand(string expression, NodeStackFrame stackFrame = null);
        EvaluateCommand CreateEvaluateCommand(int variableId, NodeStackFrame stackFrame = null);
        ListBreakpointsCommand CreateListBreakpointsCommand();
        LookupCommand CreateLookupCommand(List<NodeEvaluationResult> parents);
        LookupCommand CreateLookupCommand(int[] handles);
        ScriptsCommand CreateScriptsCommand(bool includeSource = false, int? moduleId = null);
        SetBreakpointCommand CreateSetBreakpointCommand(NodeModule module, NodeBreakpoint breakpoint, bool withoutPredicate = false);
        SetExceptionBreakCommand CreateSetExceptionBreakCommand(bool uncaughtExceptions, bool enabled);
        SuspendCommand CreateSuspendCommand();
    }
}