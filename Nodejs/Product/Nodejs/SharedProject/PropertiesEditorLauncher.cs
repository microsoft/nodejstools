// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using ErrorHandler = Microsoft.VisualStudio.ErrorHandler;

namespace Microsoft.VisualStudioTools.Project
{
    /// <summary>
    /// This class is used to enable launching the project properties
    /// editor from the Properties Browser.
    /// </summary>

    public class PropertiesEditorLauncher : ComponentEditor
    {
        private ServiceProvider serviceProvider;

        #region ctor
        public PropertiesEditorLauncher(ServiceProvider serviceProvider)
        {
            Utilities.ArgumentNotNull(nameof(serviceProvider), serviceProvider);

            this.serviceProvider = serviceProvider;
        }
        #endregion
        #region overridden methods
        /// <summary>
        /// Launch the Project Properties Editor (properties pages)
        /// </summary>
        /// <returns>If we succeeded or not</returns>
        public override bool EditComponent(ITypeDescriptorContext context, object component)
        {
            if (component is ProjectNodeProperties)
            {
                var propertyPageFrame = (IVsPropertyPageFrame)this.serviceProvider.GetService((typeof(SVsPropertyPageFrame)));

                var hr = propertyPageFrame.ShowFrame(Guid.Empty);
                if (ErrorHandler.Succeeded(hr))
                {
                    return true;
                }
                else
                {
                    ErrorHandler.ThrowOnFailure(propertyPageFrame.ReportError(hr));
                }
            }

            return false;
        }
        #endregion

    }
}
