// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.PythonTools.Interpreter;
using Microsoft.TC.TestHostAdapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestUtilities.UI {
    public class DefaultInterpreterSetter : IDisposable {
        public readonly object OriginalInterpreter, OriginalVersion;
        private bool _isDisposed;

        public DefaultInterpreterSetter(IPythonInterpreterFactory factory) {
            var props = VsIdeTestHostContext.Dte.get_Properties("Python Tools", "Interpreters");
            Assert.IsNotNull(props);

            OriginalInterpreter = props.Item("DefaultInterpreter").Value;
            OriginalVersion = props.Item("DefaultInterpreterVersion").Value;

            props.Item("DefaultInterpreter").Value = factory.Id;
            props.Item("DefaultInterpreterVersion").Value = string.Format("{0}.{1}", factory.Configuration.Version.Major, factory.Configuration.Version.Minor);
        }

        public void Dispose() {
            if (!_isDisposed) {
                _isDisposed = true;

                var props = VsIdeTestHostContext.Dte.get_Properties("Python Tools", "Interpreters");
                Assert.IsNotNull(props);

                props.Item("DefaultInterpreter").Value = OriginalInterpreter;
                props.Item("DefaultInterpreterVersion").Value = OriginalVersion;
            }
        }
    }
}

