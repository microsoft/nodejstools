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
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Debugger.Communication {
    public class AsyncProducerConsumerCollection<T> {
        private readonly Queue<T> _collection = new Queue<T>();
        private readonly Queue<TaskCompletionSource<T>> _waiting = new Queue<TaskCompletionSource<T>>();

        public void Add(T item) {
            TaskCompletionSource<T> tcs = null;
            lock (_collection) {
                if (_waiting.Count > 0) {
                    tcs = _waiting.Dequeue();
                } else {
                    _collection.Enqueue(item);
                }
            }
            if (tcs != null) {
                tcs.TrySetResult(item);
            }
        }

        public Task<T> TakeAsync() {
            lock (_collection) {
                if (_collection.Count > 0) {
                    return Task.FromResult(_collection.Dequeue());
                }
                var tcs = new TaskCompletionSource<T>();
                _waiting.Enqueue(tcs);
                return tcs.Task;
            }
        }
    }
}