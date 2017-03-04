// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Windows.Automation;

namespace TestUtilities.UI
{
    public class AutomationDialog : AutomationWrapper, IDisposable
    {
        private bool _isDisposed;

        public VisualStudioApp App { get; private set; }
        public TimeSpan DefaultTimeout { get; set; }

        public AutomationDialog(VisualStudioApp app, AutomationElement element)
            : base(element)
        {
            App = app;
            DefaultTimeout = TimeSpan.FromSeconds(10.0);
        }

        public static AutomationDialog FromDte(VisualStudioApp app, string commandName, string commandArgs = "")
        {
            return new AutomationDialog(
                app,
                AutomationElement.FromHandle(app.OpenDialogWithDteExecuteCommand(commandName, commandArgs))
            );
        }

        public static AutomationDialog WaitForDialog(VisualStudioApp app)
        {
            return new AutomationDialog(app, AutomationElement.FromHandle(app.WaitForDialog()));
        }

        #region IDisposable Members

        ~AutomationDialog()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    try
                    {
                        Element.GetWindowPattern().Close();
                    }
                    catch (InvalidOperationException)
                    {
                    }
                    catch (ElementNotAvailableException)
                    {
                    }
                }
                _isDisposed = true;
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        public void ClickButtonAndClose(string buttonName, bool nameIsAutomationId = false)
        {
            WaitForInputIdle();
            if (nameIsAutomationId)
            {
                WaitForClosed(DefaultTimeout, () => ClickButtonByAutomationId(buttonName));
            }
            else
            {
                WaitForClosed(DefaultTimeout, () => ClickButtonByName(buttonName));
            }
        }

        public virtual void OK()
        {
            ClickButtonAndClose("OK");
        }

        public virtual void Cancel()
        {
            ClickButtonAndClose("Cancel");
        }
    }
}

