// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.NodejsTools.Npm;
using Newtonsoft.Json;

namespace NpmTests
{
    internal class MockPackageJsonSource : IPackageJsonSource
    {
        public MockPackageJsonSource(string packageJsonString)
        {
            Package = JsonConvert.DeserializeObject(packageJsonString);
        }

        public dynamic Package { get; private set; }
    }
}

