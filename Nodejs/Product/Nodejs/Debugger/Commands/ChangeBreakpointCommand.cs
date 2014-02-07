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
    sealed class ChangeBreakpointCommand : DebuggerCommandBase {
        public ChangeBreakpointCommand(int id, int breakpointId, bool? enabled = null, string condition = null, int? ignoreCount = null) : base(id) {
            bool? enabled1 = enabled;
            string condition1 = condition;
            int? ignoreCount1 = ignoreCount;

            CommandName = "changebreakpoint";
            Arguments = new Dictionary<string, object> { { "breakpoint", breakpointId } };
            if (enabled1 != null) {
                Arguments["enabled"] = enabled1.Value;
            }

            if (condition1 != null) {
                Arguments["condition"] = condition1;
            }

            if (ignoreCount1 != null) {
                Arguments["ignoreCount"] = ignoreCount1.Value;
            }
        }
    }
}