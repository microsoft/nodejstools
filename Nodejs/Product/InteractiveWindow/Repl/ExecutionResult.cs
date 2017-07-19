// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Repl
{
    /// <summary>
    /// The result of command execution.  
    /// </summary>
    public struct ExecutionResult
    {
        public static readonly ExecutionResult Success = new ExecutionResult(true);
        public static readonly ExecutionResult Failure = new ExecutionResult(false);
        public static readonly Task<ExecutionResult> Succeeded = MakeSucceeded();
        public static readonly Task<ExecutionResult> Failed = MakeFailed();

        private readonly bool _successful;

        public ExecutionResult(bool isSuccessful)
        {
            _successful = isSuccessful;
        }

        public bool IsSuccessful
        {
            get
            {
                return _successful;
            }
        }

        private static Task<ExecutionResult> MakeSucceeded()
        {
            var taskSource = new TaskCompletionSource<ExecutionResult>();
            taskSource.SetResult(Success);
            return taskSource.Task;
        }

        private static Task<ExecutionResult> MakeFailed()
        {
            var taskSource = new TaskCompletionSource<ExecutionResult>();
            taskSource.SetResult(Failure);
            return taskSource.Task;
        }
    }
}
