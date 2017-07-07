// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.NodejsTools.TestFrameworks;

namespace Microsoft.NodejsTools.TestAdapter.TestFrameworks
{
    internal class FrameworkDiscover
    {
        private readonly Dictionary<String, TestFramework> _frameworks = new Dictionary<string, TestFramework>(StringComparer.OrdinalIgnoreCase);
        public FrameworkDiscover()
        {
            var directoryLoader = new TestFrameworkDirectories();
            var testFrameworkDirectories = directoryLoader.GetFrameworkDirectories();

            foreach (var directory in testFrameworkDirectories)
            {
                var fx = new TestFramework(directory);
                _frameworks.Add(fx.Name, fx);
            }
        }

        public TestFramework Get(string frameworkName)
        {
            TestFramework testFX = null;
            _frameworks.TryGetValue(frameworkName, out testFX);
            return testFX;
        }
    }
}
