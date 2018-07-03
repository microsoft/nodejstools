// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Linq;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace Microsoft.NodejsTools.TestAdapter
{
    public sealed class UnitTestSettings
    {
        public readonly string TestRoot;
        public readonly string TestFrameworksLocation;

        public UnitTestSettings(IRunSettings runSettings)
        {
            var xml = XDocument.Parse(runSettings.SettingsXml);
            var jsUnitTestRoot = xml.Descendants("JavaScriptUnitTest");
            this.TestRoot = jsUnitTestRoot.Descendants("TestSource").FirstOrDefault()?.Value;
            this.TestFrameworksLocation = jsUnitTestRoot.Descendants("TestFrameworkRoot").FirstOrDefault()?.Value;
        }

        public UnitTestSettings(string testRoot, string testFrameworksPath)
        {
            this.TestRoot = testRoot;
            this.TestFrameworksLocation = testFrameworksPath;
        }
    }
}
