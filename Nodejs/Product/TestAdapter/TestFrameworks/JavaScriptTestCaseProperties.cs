// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace Microsoft.NodejsTools.TestAdapter.TestFrameworks
{
    public static class JavaScriptTestCaseProperties
    {
        public static readonly TestProperty TestFramework = TestProperty.Register(id: $"NodeTools.{nameof(TestFramework)}", label: nameof(TestFramework), valueType: typeof(string), owner: typeof(TestCase));
        public static readonly TestProperty WorkingDir = TestProperty.Register(id: $"NodeTools.{nameof(WorkingDir)}", label: nameof(WorkingDir), valueType: typeof(string), owner: typeof(TestCase));
        public static readonly TestProperty NodeExePath = TestProperty.Register(id: $"NodeTools.{nameof(NodeExePath)}", label: nameof(NodeExePath), valueType: typeof(string), owner: typeof(TestCase));
    }
}
