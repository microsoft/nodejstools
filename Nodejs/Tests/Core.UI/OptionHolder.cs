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
using Microsoft.TC.TestHostAdapters;

namespace Microsoft.Nodejs.Tests.UI {
    class OptionHolder : IDisposable {
        private readonly string _category, _page, _option;
        private readonly object _oldValue;

        public OptionHolder(string category, string page, string option, object newValue) {
            _category = category;
            _page = page;
            _option = option;            
            var props = VsIdeTestHostContext.Dte.get_Properties(category, page);
            _oldValue = props.Item(option).Value;
            props.Item(option).Value = newValue;
        }

        public void Dispose() {
            var props = VsIdeTestHostContext.Dte.get_Properties(_category, _page);
            props.Item(_option).Value = _oldValue;
        }
    }
}
