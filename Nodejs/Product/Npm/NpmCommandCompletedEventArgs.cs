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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Npm {

    /// <summary>
    /// Fired when an attempt to execute an npm command is completed, whether
    /// successfully or not.
    /// </summary>
    public class NpmCommandCompletedEventArgs : EventArgs {
        public NpmCommandCompletedEventArgs(string arguments, bool withErrors, bool cancelled) {
            Arguments = arguments;
            WithErrors = withErrors;
            Cancelled = cancelled;
        }

        public string Arguments { get; private set; }

        public string CommandText {
            get { return string.IsNullOrEmpty(Arguments) ? "npm" : string.Format("npm {0}", Arguments); }
        }

        /// <summary>
        /// Indicates whether or not there were errors whilst executing npm.
        /// </summary>
        public bool WithErrors { get; private set; }

        /// <summary>
        /// Indicates whether or not the command was cancelled, with or without errors.
        /// </summary>
        public bool Cancelled { get; private set; }
    }
}
