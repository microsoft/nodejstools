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

namespace Microsoft.NodejsTools.Parsing {
    internal class CollectingErrorSink : ErrorSink {
        private readonly List<ErrorResult> _errors = new List<ErrorResult>();
        private readonly List<ErrorResult> _warnings = new List<ErrorResult>();

        public override void OnError(JScriptExceptionEventArgs error) {
            var span = new SourceSpan(
                new SourceLocation(
                    error.Exception.Span.Start,
                    error.Error.StartLine,
                    error.Error.StartColumn
                ),
                new SourceLocation(
                    error.Exception.Span.End,
                    error.Error.EndLine,
                    error.Error.EndColumn
                )
            );
            var result = new ErrorResult(error.Error.Message, span, error.Error.ErrorCode);

            if (error.Error.IsError) {
                Errors.Add(result);
            } else {
                Warnings.Add(result);
            }
        }

        public List<ErrorResult> Errors {
            get {
                return _errors;
            }
        }

        public List<ErrorResult> Warnings {
            get {
                return _warnings;
            }
        }
    }
}
