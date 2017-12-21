// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Threading;

namespace Microsoft.VisualStudioTools
{
    internal static class ExceptionExtensions
    {
        /// <summary>
        /// Returns true if an exception should not be handled by logging code.
        /// </summary>
        public static bool IsCriticalException(this Exception ex)
        {
            return ex is StackOverflowException ||
                ex is OutOfMemoryException ||
                ex is ThreadAbortException ||
                ex is AccessViolationException;
        }
    }
}
