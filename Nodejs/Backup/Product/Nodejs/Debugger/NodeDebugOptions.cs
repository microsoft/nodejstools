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

namespace Microsoft.NodejsTools.Debugger {
    [Flags]
    enum NodeDebugOptions {
        None,
        /// <summary>
        /// Passing this flag to the debugger will cause it to wait for input on an abnormal (non-zero)
        /// exit code.
        /// </summary>
        WaitOnAbnormalExit = 0x01,
        /// <summary>
        /// Passing this flag to the debugger will cause it to wait for input on a normal (zero) exit code.
        /// </summary>
        WaitOnNormalExit = 0x02,
        /// <summary>
        /// Passing this flag will cause output to standard out to be redirected via the debugger
        /// so it can be outputted in the Visual Studio debug output window.
        /// </summary>
        RedirectOutput = 0x04,

        /// <summary>
        /// Set if you do not want to create a window
        /// </summary>
        CreateNoWindow = 0x40
    }
}
