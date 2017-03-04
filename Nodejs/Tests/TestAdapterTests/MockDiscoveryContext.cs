// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace TestAdapterTests
{
    internal class MockDiscoveryContext : IDiscoveryContext
    {
        public IRunSettings RunSettings
        {
            get { throw new NotImplementedException(); }
        }
    }
}

