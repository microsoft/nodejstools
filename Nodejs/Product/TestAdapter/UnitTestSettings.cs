// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Linq;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace Microsoft.NodejsTools.TestAdapter
{
    public sealed class UnitTestSettings
    {
        public readonly string TestSource;
        public readonly string TestFrameworksLocation;
        public readonly bool IsBeingDebugged;

        public UnitTestSettings(IRunSettings runSettings, bool isBeingDebugged = false)
        {
            var xml = XDocument.Parse(runSettings.SettingsXml);
            var jsUnitTestRoot = xml.Descendants("JavaScriptUnitTest");
            this.TestSource = jsUnitTestRoot.Descendants("TestSource").FirstOrDefault()?.Value;
            this.TestFrameworksLocation = jsUnitTestRoot.Descendants("TestFrameworkRoot").FirstOrDefault()?.Value;
            this.IsBeingDebugged = isBeingDebugged;
        }

        public UnitTestSettings(string testSource, string testFrameworksPath, bool isBeingDebugged = false)
        {
            this.TestSource = testSource;
            this.TestFrameworksLocation = testFrameworksPath;
            this.IsBeingDebugged = isBeingDebugged;
        }
    }
}
