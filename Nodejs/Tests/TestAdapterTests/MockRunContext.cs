// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace TestAdapterTests
{
    internal class MockRunContext : IRunContext
    {
        public ITestCaseFilterExpression GetTestCaseFilter(IEnumerable<string> supportedProperties, Func<string, TestProperty> propertyProvider)
        {
            throw new NotImplementedException();
        }

        public bool InIsolation
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsBeingDebugged
        {
            get { return false; }
        }

        public bool IsDataCollectionEnabled
        {
            get { throw new NotImplementedException(); }
        }

        public bool KeepAlive
        {
            get { throw new NotImplementedException(); }
        }

        public string SolutionDirectory
        {
            get { throw new NotImplementedException(); }
        }

        public string TestRunDirectory
        {
            get { throw new NotImplementedException(); }
        }

        public IRunSettings RunSettings
        {
            get { throw new NotImplementedException(); }
        }
    }
}

