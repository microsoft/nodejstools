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
using System.Linq;
using System.Xml.Linq;
using Microsoft.NodejsTools;
using Microsoft.NodejsTools.Project.ImportWizard;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtilities;
using TestUtilities.Nodejs;

namespace NodejsTests {
    [TestClass]
    public class ImportWizardTests {
        [ClassInitialize]
        public static void DoDeployment(TestContext context) {
            AssertListener.Initialize();
            NodejsTestData.Deploy();
        }

        [TestMethod, Priority(0)]
        public void ImportWizardSimple() {
            var settings = new ImportSettings();

            settings.SourcePath = TestData.GetPath("TestData\\HelloWorld\\");
            settings.Filters = "*.js;*.njsproj";
            settings.ProjectPath = TestData.GetPath("TestData\\TestDestination\\Subdirectory\\ProjectName.njsproj");

            var path = settings.CreateRequestedProject();

            Assert.AreEqual(settings.ProjectPath, path);
            var proj = XDocument.Load(path);

            Assert.AreEqual("..\\..\\HelloWorld\\", proj.Descendant("ProjectHome").Value);
            AssertUtil.ContainsExactly(proj.Descendants(proj.GetName("Compile")).Select(x => x.Attribute("Include").Value),
                "server.js");
            AssertUtil.ContainsExactly(proj.Descendants(proj.GetName("Content")).Select(x => x.Attribute("Include").Value),
                "HelloWorld.njsproj");
        }

        [TestMethod, Priority(0)]
        public void ImportWizardFiltered() {
            var settings = new ImportSettings();

            settings.SourcePath = TestData.GetPath("TestData\\HelloWorld\\");
            settings.Filters = "*.js";
            settings.ProjectPath = TestData.GetPath("TestData\\TestDestination\\Subdirectory\\ProjectName.njsproj");

            var path = settings.CreateRequestedProject();

            Assert.AreEqual(settings.ProjectPath, path);
            var proj = XDocument.Load(path);

            Assert.AreEqual("..\\..\\HelloWorld\\", proj.Descendant("ProjectHome").Value);
            AssertUtil.ContainsExactly(proj.Descendants(proj.GetName("Compile")).Select(x => x.Attribute("Include").Value),
                "server.js");
            Assert.AreEqual(0, proj.Descendants(proj.GetName("Content")).Count());
        }

        [TestMethod, Priority(0)]
        public void ImportWizardFolders() {
            var settings = new ImportSettings();

            settings.SourcePath = TestData.GetPath("TestData\\HelloWorld2\\");
            settings.Filters = "*";
            settings.ProjectPath = TestData.GetPath("TestData\\TestDestination\\Subdirectory\\ProjectName.njsproj");

            var path = settings.CreateRequestedProject();

            Assert.AreEqual(settings.ProjectPath, path);
            var proj = XDocument.Load(path);

            Assert.AreEqual("..\\..\\HelloWorld2\\", proj.Descendant("ProjectHome").Value);
            AssertUtil.ContainsExactly(proj.Descendants(proj.GetName("Compile")).Select(x => x.Attribute("Include").Value),
                "server.js",
                "TestFolder\\SubItem.js",
                "TestFolder2\\SubItem.js",
                "TestFolder3\\SubItem.js");

            AssertUtil.ContainsExactly(proj.Descendants(proj.GetName("Folder")).Select(x => x.Attribute("Include").Value),
                "TestFolder",
                "TestFolder2",
                "TestFolder3");
        }

        [TestMethod, Priority(0)]
        public void ImportWizardStartupFile() {
            var settings = new ImportSettings();

            settings.SourcePath = TestData.GetPath("TestData\\HelloWorld\\");
            settings.Filters = "*.js;*.njsproj";
            settings.StartupFile = "server.js";
            settings.ProjectPath = TestData.GetPath("TestData\\TestDestination\\Subdirectory\\ProjectName.njsproj");

            var path = settings.CreateRequestedProject();

            Assert.AreEqual(settings.ProjectPath, path);
            var proj = XDocument.Load(path);

            Assert.AreEqual("server.js", proj.Descendant("StartupFile").Value);
        }

        [TestMethod, Priority(0)]
        public void ImportWizardIncludeNodeModules() {
            var settings = new ImportSettings();

            settings.SourcePath = TestData.GetPath("TestData\\HelloWorld3\\");
            settings.Filters = "*.js;*.njsproj";
            settings.StartupFile = "server.js";
            settings.ExcludeNodeModules = false;
            settings.ProjectPath = TestData.GetPath("TestData\\TestDestination\\Subdirectory\\ProjectName.njsproj");

            var path = settings.CreateRequestedProject();

            Assert.AreEqual(settings.ProjectPath, path);
            var proj = XDocument.Load(path);

            AssertUtil.ContainsExactly(proj.Descendants(proj.GetName("Compile")).Select(x => x.Attribute("Include").Value),
                "server.js",
                "node_modules\\mymod.js");
        }

        [TestMethod, Priority(0)]
        public void ImportWizardExcludeNodeModules() {
            var settings = new ImportSettings();

            settings.SourcePath = TestData.GetPath("TestData\\HelloWorld3\\");
            settings.Filters = "*.js;*.njsproj";
            settings.StartupFile = "server.js";
            settings.ExcludeNodeModules = true;
            settings.ProjectPath = TestData.GetPath("TestData\\TestDestination\\Subdirectory\\ProjectName.njsproj");

            var path = settings.CreateRequestedProject();

            Assert.AreEqual(settings.ProjectPath, path);
            var proj = XDocument.Load(path);

            AssertUtil.ContainsExactly(proj.Descendants(proj.GetName("Compile")).Select(x => x.Attribute("Include").Value),
                "server.js");
        }
    }
}
