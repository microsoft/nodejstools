// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Debugger
{
    internal static class Extensions
    {
        internal static async Task<T> WaitAsync<T>(this Task<T> task, TimeSpan timeout, CancellationToken token = default(CancellationToken))
        {
            var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
            linkedTokenSource.CancelAfter(timeout);
            // If the token will time out, this await will throw TaskCanceledException, which is automatically propagated to our caller.
            await task.ContinueWith(t => { }, linkedTokenSource.Token).ConfigureAwait(false);
            // If we're still here, the token didn't time out, so the original task has completed execution (by succeeding or failing);
            // return its result or propagate the exception.
            return await task.ConfigureAwait(false);
        }

        internal static async Task WaitAsync(this Task task, TimeSpan timeout, CancellationToken token = default(CancellationToken))
        {
            // Same logic as above but for a regular Task without a return value.
            var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
            linkedTokenSource.CancelAfter(timeout);
            await task.ContinueWith(t => { }, linkedTokenSource.Token).ConfigureAwait(false);
            await task.ConfigureAwait(false);
        }
    }
}

