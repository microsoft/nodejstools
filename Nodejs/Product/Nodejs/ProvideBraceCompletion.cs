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
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.NodejsTools {
    class ProvideBraceCompletionAttribute : RegistrationAttribute {
        private readonly string _languageName;

        public ProvideBraceCompletionAttribute(string languageName) {
            _languageName = languageName;
        }

        public override void Register(RegistrationAttribute.RegistrationContext context) {
            using (Key serviceKey = context.CreateKey(LanguageServicesKeyName)) {
                serviceKey.SetValue("ShowBraceCompletion", (int)1);
            }
        }

        public override void Unregister(RegistrationAttribute.RegistrationContext context) {
        }

        private string LanguageServicesKeyName {
            get {
                return string.Format(CultureInfo.InvariantCulture,
                                     "{0}\\{1}",
                                     "Languages\\Language Services",
                                     _languageName);
            }
        }
    }
}
