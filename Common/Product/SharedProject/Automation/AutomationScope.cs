//*********************************************************//
//    Copyright (c) Microsoft. All rights reserved.
//    
//    Apache 2.0 License
//    
//    You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//    
//    Unless required by applicable law or agreed to in writing, software 
//    distributed under the License is distributed on an "AS IS" BASIS, 
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or 
//    implied. See the License for the specific language governing 
//    permissions and limitations under the License.
//
//*********************************************************//

using System;
using Microsoft.VisualStudio.Shell.Interop;
using ErrorHandler = Microsoft.VisualStudio.ErrorHandler;

namespace Microsoft.VisualStudioTools.Project.Automation
{
    /// <summary>
    /// Helper class that handle the scope of an automation function.
    /// It should be used inside a "using" directive to define the scope of the
    /// automation function and make sure that the ExitAutomation method is called.
    /// </summary>
    internal sealed class AutomationScope : IDisposable
    {
        private IVsExtensibility3 extensibility;
        private bool inAutomation;
        private bool isDisposed;

        /// <summary>
        /// Defines the beginning of the scope of an automation function. This constuctor
        /// calls EnterAutomationFunction to signal the Shell that the current function is
        /// changing the status of the automation objects.
        /// </summary>
        public AutomationScope(IServiceProvider provider)
        {
            Utilities.ArgumentNotNull("provider", provider);

            extensibility = provider.GetService(typeof(EnvDTE.IVsExtensibility)) as IVsExtensibility3;
            if (null == extensibility)
            {
                throw new InvalidOperationException();
            }
            ErrorHandler.ThrowOnFailure(extensibility.EnterAutomationFunction());
            inAutomation = true;
        }

        /// <summary>
        /// Ends the scope of the automation function. This function is also called by the
        /// Dispose method.
        /// </summary>
        public void ExitAutomation()
        {
            if (inAutomation)
            {
                ErrorHandler.ThrowOnFailure(extensibility.ExitAutomationFunction());
                inAutomation = false;
            }
        }

        /// <summary>
        /// Gets the IVsExtensibility3 interface used in the automation function.
        /// </summary>
        public IVsExtensibility3 Extensibility
        {
            get { return extensibility; }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #region IDisposable Members
        private void Dispose(bool disposing)
        {
            if (!this.isDisposed)
            {
                if (disposing)
                {
                    ExitAutomation();
                }

                this.isDisposed = true;
            }
        }
        #endregion
    }
}
