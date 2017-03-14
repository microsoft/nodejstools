// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Diagnostics;

namespace Microsoft.VisualStudioTools
{
    internal struct DebugTimer : IDisposable
    {
#if DEBUG
        internal static Stopwatch _timer = MakeStopwatch();
        private readonly long _start, _minReportTime;
        private readonly string _description;

        private static Stopwatch MakeStopwatch()
        {
            var res = new Stopwatch();
            res.Start();
            return res;
        }
#endif

        /// <summary>
        /// Creates a new DebugTimer which logs timing information from when it's created
        /// to when it's disposed.
        /// </summary>
        /// <param name="description">The message which is logged in addition to the timing information</param>
        /// <param name="minReportTime">The minimum amount of time (in milliseconds) which needs to elapse for a message to be logged</param>
        public DebugTimer(string description, long minReportTime = 0)
        {
#if DEBUG
            this._start = _timer.ElapsedMilliseconds;
            this._description = description;
            this._minReportTime = minReportTime;
#endif
        }

        #region IDisposable Members

        public void Dispose()
        {
#if DEBUG
            var elapsed = _timer.ElapsedMilliseconds - this._start;
            if (elapsed >= this._minReportTime)
            {
                Debug.WriteLine(String.Format("{0}: {1}ms elapsed", this._description, elapsed));
                Console.WriteLine(String.Format("{0}: {1}ms elapsed", this._description, elapsed));
            }
#endif
        }

        #endregion
    }
}

