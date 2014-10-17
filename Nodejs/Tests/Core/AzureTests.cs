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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using Microsoft.NodejsTools;
using Microsoft.NodejsTools.Project;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtilities;

namespace NodejsTests {
    [TestClass]
    public class AzureTests {
        [TestMethod, Priority(0)]
        public void UpdateWorkerRoleServiceDefinitionTest() {
            var doc = new XmlDocument();
            doc.LoadXml(@"<?xml version=""1.0"" encoding=""utf-8""?>
<ServiceDefinition name=""Azure1"" xmlns=""http://schemas.microsoft.com/ServiceHosting/2008/10/ServiceDefinition"" schemaVersion=""2014-01.2.3"">
  <WorkerRole name=""WorkerProject"" vmsize=""Small"" />
  <WebRole name=""WebProject"" />
</ServiceDefinition>");

            NodejsProject.UpdateServiceDefinition(doc, "Worker", "WorkerProject");

            AreEqual(@"<?xml version=""1.0"" encoding=""utf-8""?>
<ServiceDefinition name=""Azure1"" xmlns=""http://schemas.microsoft.com/ServiceHosting/2008/10/ServiceDefinition"" schemaVersion=""2014-01.2.3"">
  <WorkerRole name=""WorkerProject"" vmsize=""Small"">
    <Startup>
      <Task commandLine=""setup_worker.cmd &gt; log.txt"" executionContext=""elevated"" taskType=""simple"">
        <Environment>
          <Variable name=""EMULATED"">
            <RoleInstanceValue xpath=""/RoleEnvironment/Deployment/@emulated"" />
          </Variable>
          <Variable name=""RUNTIMEID"" value=""node"" />
          <Variable name=""RUNTIMEURL"" value=""http://az413943.vo.msecnd.net/node/0.10.21.exe;http://nodertncu.blob.core.windows.net/iisnode/0.1.21.exe"" />
        </Environment>
      </Task>
    </Startup>
    <Runtime>
      <Environment>
        <Variable name=""PORT"">
          <RoleInstanceValue xpath=""/RoleEnvironment/CurrentInstance/Endpoints/Endpoint[@name='HttpIn']/@port"" />
        </Variable>
        <Variable name=""EMULATED"">
          <RoleInstanceValue xpath=""/RoleEnvironment/Deployment/@emulated"" />
        </Variable>
      </Environment>
      <EntryPoint>
        <ProgramEntryPoint commandLine=""node.cmd .\server.js"" setReadyOnProcessStart=""true"" />
      </EntryPoint>
    </Runtime>
  </WorkerRole>
  <WebRole name=""WebProject"" />
</ServiceDefinition>", doc);
        }

        [TestMethod, Priority(0)]
        public void UpdateWebRoleServiceDefinitionTest() {
            var doc = new XmlDocument();
            doc.LoadXml(@"<?xml version=""1.0"" encoding=""utf-8""?>
<ServiceDefinition name=""Azure1"" xmlns=""http://schemas.microsoft.com/ServiceHosting/2008/10/ServiceDefinition"" schemaVersion=""2014-01.2.3"">
  <WorkerRole name=""WorkerProject"" vmsize=""Small"" />
  <WebRole name=""WebProject"" />
</ServiceDefinition>");

            NodejsProject.UpdateServiceDefinition(doc, "Web", "WebProject");

            AreEqual(@"<?xml version=""1.0"" encoding=""utf-8""?>
<ServiceDefinition name=""Azure1"" xmlns=""http://schemas.microsoft.com/ServiceHosting/2008/10/ServiceDefinition"" schemaVersion=""2014-01.2.3"">
  <WorkerRole name=""WorkerProject"" vmsize=""Small"" />
  <WebRole name=""WebProject"">
    <Startup>
      <Task commandLine=""setup_web.cmd &gt; log.txt"" executionContext=""elevated"" taskType=""simple"">
        <Environment>
          <Variable name=""EMULATED"">
            <RoleInstanceValue xpath=""/RoleEnvironment/Deployment/@emulated"" />
          </Variable>
          <Variable name=""RUNTIMEID"" value=""node"" />
          <Variable name=""RUNTIMEURL"" value=""http://az413943.vo.msecnd.net/node/0.10.21.exe;http://nodertncu.blob.core.windows.net/iisnode/0.1.21.exe"" />
        </Environment>
      </Task>
    </Startup>
  </WebRole>
</ServiceDefinition>", doc);
        }

        #region Compare XmlDocuments

        public static void AreEqual(string expected, XmlDocument actual, string message = null) {
            var expectedDoc = new XmlDocument();
            expectedDoc.LoadXml(expected);
            AreEqual(expectedDoc, actual, message);
        }

        public static void AreEqual(XmlDocument expected, XmlDocument actual, string message = null) {
            Console.WriteLine(actual.OuterXml);
            var nav1 = expected.CreateNavigator();
            var nav2 = actual.CreateNavigator();

            if (string.IsNullOrEmpty(message)) {
                message = string.Empty;
            } else {
                message = " " + message;
            }

            AreXPathNavigatorsEqual(nav1, nav2, message);
        }

        private static string GetFullPath(XPathNavigator nav) {
            nav = nav.CreateNavigator();
            var names = new Stack<string>();

            names.Push(nav.Name);
            while (nav.MoveToParent()) {
                names.Push(nav.Name);
            }

            return "/" + string.Join("/", names);
        }

        private static void AreXPathNavigatorsEqual(XPathNavigator nav1, XPathNavigator nav2, string message) {
            while (true) {
                if (nav1.Name != nav2.Name) {
                    Assert.Fail("Expected element <{0}>. Actual element <{1}>.{2}", nav1.Name, nav2.Name, message);
                }
                var anav1 = nav1.CreateNavigator();
                var anav2 = nav2.CreateNavigator();
                var attr1 = new List<string>();
                var attr2 = new List<string>();

                if (anav1.MoveToFirstAttribute()) {
                    do {
                        attr1.Add(string.Format("{0}=\"{1}\"", anav1.Name, anav1.Value));
                    } while (anav1.MoveToNextAttribute());
                }
                if (anav2.MoveToFirstAttribute()) {
                    do {
                        attr2.Add(string.Format("{0}=\"{1}\"", anav2.Name, anav2.Value));
                    } while (anav2.MoveToNextAttribute());
                }

                AssertUtil.ContainsExactly(attr2, attr1);

                var cnav1 = nav1.CreateNavigator();
                var cnav2 = nav2.CreateNavigator();
                if (cnav1.MoveToFirstChild()) {
                    if (cnav2.MoveToFirstChild()) {
                        AreXPathNavigatorsEqual(cnav1, cnav2, message);
                    } else {
                        Assert.Fail("Expected element {0}.{1}", GetFullPath(cnav1), message);
                    }
                } else if (cnav2.MoveToFirstChild()) {
                    Assert.Fail("Unexpected element {0}.{1}", GetFullPath(cnav2), message);
                }

                if (nav1.MoveToNext()) {
                    if (nav2.MoveToNext()) {
                        continue;
                    } else {
                        Assert.Fail("Expected element {0}.{1}", GetFullPath(nav1), message);
                    }
                } else if (nav2.MoveToNext()) {
                    Assert.Fail("Unexpected element {0}.{1}", GetFullPath(nav2), message);
                }
                break;
            }
        }

        #endregion
    }
}
