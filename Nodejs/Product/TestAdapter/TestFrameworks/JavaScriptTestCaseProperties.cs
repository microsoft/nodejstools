// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace Microsoft.NodejsTools.TestAdapter.TestFrameworks
{
    public static class JavaScriptTestCaseProperties
    {
        public static readonly TestProperty TestFramework = TestProperty.Register(id: "NodeTools.TestFramework", label: "TestFramework", valueType: typeof(string), owner: typeof(TestCase));
    }
}
