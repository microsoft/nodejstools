// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudioTools.Project
{
    internal sealed class WaitDialog : IDisposable
    {
        private readonly int _waitResult;
        private readonly IVsThreadedWaitDialog2 _waitDialog;

        public WaitDialog(string waitCaption, string waitMessage, IServiceProvider serviceProvider, int displayDelay = 1, bool isCancelable = false, bool showProgress = false)
        {
            this._waitDialog = (IVsThreadedWaitDialog2)serviceProvider.GetService(typeof(SVsThreadedWaitDialog));
            this._waitResult = this._waitDialog.StartWaitDialog(
                waitCaption,
                waitMessage,
                null,
                null,
                null,
                displayDelay,
                isCancelable,
                showProgress
            );
        }

        public void UpdateProgress(int currentSteps, int totalSteps)
        {
            bool canceled;
            this._waitDialog.UpdateProgress(
                null,
                null,
                null,
                currentSteps,
                totalSteps,
                false,
                out canceled
            );
        }

        public bool Canceled
        {
            get
            {
                bool canceled;
                ErrorHandler.ThrowOnFailure(this._waitDialog.HasCanceled(out canceled));
                return canceled;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (ErrorHandler.Succeeded(this._waitResult))
            {
                var cancelled = 0;
                this._waitDialog.EndWaitDialog(out cancelled);
            }
        }

        #endregion
    }
}
