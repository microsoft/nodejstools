// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Automation;
using EnvDTE;
using Microsoft.NodejsTools;
using Microsoft.NodejsTools.Project;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudioTools;
using Microsoft.VisualStudioTools.VSTestHost;
using TestUtilities;
using TestUtilities.UI;
using TestUtilities.Nodejs;
using Key = System.Windows.Input.Key;
using MouseButton = System.Windows.Input.MouseButton;

namespace Microsoft.Nodejs.Tests.UI
{
    [TestClass]
    public class NodejsBasicProjectTests : NodejsProjectTest
    {
        [ClassInitialize]
        public static void DoDeployment(TestContext context)
        {
            AssertListener.Initialize();
            NodejsTestData.Deploy();
        }

        [TestMethod, Priority(0), TestCategory("Core"), TestCategory("Ignore")]
        [HostType("VSTestHost")]
        public void AddNewTypeScriptItem()
        {
            using (var solution = Project("AddNewTypeScriptItem", Compile("server")).Generate().ToVs())
            {
                var project = solution.WaitForItem("AddNewTypeScriptItem", "server.js");
                AutomationWrapper.Select(project);

                using (var newItem = solution.AddNewItem())
                {
                    newItem.FileName = "NewTSFile.ts";
                    newItem.OK();
                }

                using (AutoResetEvent buildDone = new AutoResetEvent(false))
                {
                    VSTestContext.DTE.Events.BuildEvents.OnBuildDone += (sender, args) =>
                    {
                        buildDone.Set();
                    };

                    solution.ExecuteCommand("Build.BuildSolution");
                    solution.WaitForOutputWindowText("Build", "tsc.exe");
                    Assert.IsTrue(buildDone.WaitOne(10000), "failed to wait for build)");
                }
            }
        }

        /// <summary>
        /// https://nodejstools.codeplex.com/workitem/1195
        /// </summary>
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void ExcludedErrors()
        {
            var project = Project("TestExcludedErrors",
                Compile("server", "function f(a, b, c) { }\r\n\r\n"),
                Compile("excluded", "aa bb", isExcluded: true)
            );

            using (var solution = project.Generate().ToVs())
            {
                List<IVsTaskItem> allItems = solution.WaitForErrorListItems(0);
                Assert.AreEqual(0, allItems.Count);

                var excluded = solution.WaitForItem("TestExcludedErrors", "excluded.js");
                AutomationWrapper.Select(excluded);
                solution.ExecuteCommand("Project.IncludeInProject");

                allItems = solution.WaitForErrorListItems(1);
                Assert.AreEqual(1, allItems.Count);

                excluded = solution.WaitForItem("TestExcludedErrors", "excluded.js");
                AutomationWrapper.Select(excluded);
                solution.ExecuteCommand("Project.ExcludeFromProject");

                allItems = solution.WaitForErrorListItems(0);
                Assert.AreEqual(0, allItems.Count);
            }
        }


        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void DebuggerPort()
        {
            var filename = Path.Combine(TestData.GetTempPath(), Path.GetRandomFileName());
            Console.WriteLine("Temp file is: {0}", filename);
            var code = String.Format(@"
require('fs').writeFileSync('{0}', process.debugPort);
while(true) {{
}}", filename.Replace("\\", "\\\\"));

            var project = Project("DebuggerPort",
                Compile("server", code),
                Property(NodeProjectProperty.DebuggerPort, "1234"),
                Property(CommonConstants.StartupFile, "server.js")
            );

            using (var solution = project.Generate().ToVs())
            {
                solution.ExecuteCommand("Debug.Start");
                solution.WaitForMode(dbgDebugMode.dbgRunMode);

                for (int i = 0; i < 10 && !File.Exists(filename); i++)
                {
                    System.Threading.Thread.Sleep(1000);
                }
                Assert.IsTrue(File.Exists(filename), "debugger port not written out");
                solution.ExecuteCommand("Debug.StopDebugging");

                Assert.AreEqual(
                    File.ReadAllText(filename),
                    "1234"
                );
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void EnvironmentVariables()
        {
            var filename = Path.Combine(TestData.GetTempPath(), Path.GetRandomFileName());
            Console.WriteLine("Temp file is: {0}", filename);
            var code = String.Format(@"
require('fs').writeFileSync('{0}', process.env.fob + process.env.bar + process.env.baz);
while(true) {{
}}", filename.Replace("\\", "\\\\"));

            var project = Project("EnvironmentVariables",
                Compile("server", code),
                Property(NodeProjectProperty.Environment, "fob=1\nbar=2;3\r\nbaz=4"),
                Property(CommonConstants.StartupFile, "server.js")
            );

            using (var solution = project.Generate().ToVs())
            {
                solution.ExecuteCommand("Debug.Start");
                solution.WaitForMode(dbgDebugMode.dbgRunMode);

                for (int i = 0; i < 10 && !File.Exists(filename); i++)
                {
                    System.Threading.Thread.Sleep(1000);
                }
                Assert.IsTrue(File.Exists(filename), "environment variables not written out");
                solution.ExecuteCommand("Debug.StopDebugging");

                Assert.AreEqual(
                    File.ReadAllText(filename),
                    "12;34"
                );
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void EnvironmentVariablesNoDebugging()
        {
            var filename = Path.Combine(TestData.GetTempPath(), Path.GetRandomFileName());
            Console.WriteLine("Temp file is: {0}", filename);
            var code = String.Format(@"
require('fs').writeFileSync('{0}', process.env.fob + process.env.bar + process.env.baz);
", filename.Replace("\\", "\\\\"));

            var project = Project("EnvironmentVariables",
                Compile("server", code),
                Property(NodeProjectProperty.Environment, "fob=1\nbar=2;3\r\nbaz=4"),
                Property(CommonConstants.StartupFile, "server.js")
            );

            using (var solution = project.Generate().ToVs())
            {
                solution.ExecuteCommand("Debug.StartWithoutDebugging");

                for (int i = 0; i < 10 && !File.Exists(filename); i++)
                {
                    System.Threading.Thread.Sleep(1000);
                }
                Assert.IsTrue(File.Exists(filename), "environment variables not written out");

                Assert.AreEqual(
                    File.ReadAllText(filename),
                    "12;34"
                );
            }
        }

        [TestMethod, Priority(0), TestCategory("Core"), TestCategory("Ignore")]
        [HostType("VSTestHost")]
        public void ProjectProperties()
        {
            var filename = Path.Combine(TestData.GetTempPath(), Path.GetRandomFileName());

            var project = Project("ProjectProperties",
                Compile("server"),
                Property(NodeProjectProperty.Environment, "fob=1\r\nbar=2;3\nbaz=4"),
                Property(NodeProjectProperty.DebuggerPort, "1234"),
                Property(CommonConstants.StartupFile, "server.js")
            );

            using (var solution = project.Generate().ToVs())
            {
                var projectNode = solution.WaitForItem("ProjectProperties");
                AutomationWrapper.Select(projectNode);

                solution.ExecuteCommand("ClassViewContextMenus.ClassViewMultiselectProjectReferencesItems.Properties");
                AutomationElement doc = null;
                for (int i = 0; i < 10; i++)
                {
                    doc = ((VisualStudioInstance)solution).App.GetDocumentTab("ProjectProperties");
                    if (doc != null)
                    {
                        break;
                    }
                    System.Threading.Thread.Sleep(1000);
                }
                Assert.IsNotNull(doc, "Failed to find project properties tab");

                var debuggerPort =
                    new TextBox(
                        new AutomationWrapper(doc).FindByAutomationId("_debuggerPort")
                    );
                var envVars = new TextBox(
                    new AutomationWrapper(doc).FindByAutomationId("_envVars")
                );

                Assert.AreEqual(debuggerPort.Value, "1234");
                Assert.AreEqual(envVars.Value, "fob=1\r\nbar=2;3\r\nbaz=4");

                debuggerPort.Value = "2468";

                // Multi-line text box does not support setting value via automation.
                envVars.SetFocus();
                Keyboard.ControlA();
                Keyboard.Backspace();
                Keyboard.Type("fob=0\nbar=0;0\nbaz=0");

                solution.ExecuteCommand("File.SaveAll");

                var projFile = File.ReadAllText(solution.GetProject("ProjectProperties").FullName);
                Assert.AreNotEqual(-1, projFile.IndexOf("<DebuggerPort>2468</DebuggerPort>"));
                Assert.AreNotEqual(-1, projFile.IndexOf("<Environment>fob=0\r\nbar=0;0\r\nbaz=0</Environment>"));
            }
        }
    }
}

