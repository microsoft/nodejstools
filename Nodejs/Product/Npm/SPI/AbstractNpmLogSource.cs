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

namespace Microsoft.NodejsTools.Npm.SPI{
    internal abstract class AbstractNpmLogSource : INpmLogSource{
        public event EventHandler CommandStarted;

        protected void OnCommandStarted(){
            var handlers = CommandStarted;
            if (null != handlers){
                handlers(this, EventArgs.Empty);
            }
        }

        protected void FireNpmLogEvent(string logText, EventHandler<NpmLogEventArgs> handlers){
            if (null != handlers && !string.IsNullOrEmpty(logText)){
                handlers(this, new NpmLogEventArgs(logText));
            }
        }

        public event EventHandler<NpmLogEventArgs> OutputLogged;

        protected void OnOutputLogged(string logText){
            FireNpmLogEvent(logText, OutputLogged);
        }

        public event EventHandler<NpmLogEventArgs> ErrorLogged;

        protected void OnErrorLogged(string logText){
            FireNpmLogEvent(logText, ErrorLogged);
        }

        public event EventHandler<NpmExceptionEventArgs> ExceptionLogged;

        protected void OnExceptionLogged(Exception e){
            var handlers = ExceptionLogged;
            if (null != handlers){
                handlers(this, new NpmExceptionEventArgs(e));
            }
        }

        public event EventHandler<NpmCommandCompletedEventArgs> CommandCompleted;

        protected void OnCommandCompleted(
            string arguments,
            bool withErrors,
            bool cancelled){
            var handlers = CommandCompleted;
            if (null != handlers){
                handlers(this, new NpmCommandCompletedEventArgs(arguments, withErrors, cancelled));
            }
        }
    }
}
