//*********************************************************//
//    Copyright (c) Microsoft. All rights reserved.
//    
//    Apache 2.0 License
//    
//    You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//    
//    Unless required by applicable law or agreed to in writing, software 
//    distributed under the License is distributed on an "AS IS" BASIS, 
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or 
//    implied. See the License for the specific language governing 
//    permissions and limitations under the License.
//
//*********************************************************//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.NodejsTools.TestAdapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtilities;
using TestUtilities.Nodejs;

namespace TestAdapterTests {
    [TestClass]
    public class TestExecutorTests {
        [ClassInitialize]
        public static void DoDeployment(TestContext context) {
            AssertListener.Initialize();
            NodejsTestData.Deploy();
        }

        [TestMethod, Priority(0)]
        public void TestRun() {
            
            var executor = new TestExecutor();
            var recorder = new MockTestExecutionRecorder();
            var runContext = new MockRunContext();
            var expectedTests = TestInfo.TestAdapterATests.Concat(TestInfo.TestAdapterBTests).ToArray();
            var testCases = expectedTests.Select(tr => tr.TestCase);

            executor.RunTests(testCases, runContext, recorder);
            PrintTestResults(recorder.Results);

            foreach (var expectedResult in expectedTests) {
                var actualResult = recorder.Results.SingleOrDefault(tr => tr.TestCase.FullyQualifiedName == expectedResult.TestCase.FullyQualifiedName);

                Assert.IsNotNull(actualResult);
                Assert.AreEqual(expectedResult.Outcome, actualResult.Outcome, expectedResult.TestCase.FullyQualifiedName + " had incorrect result");
            }
        }

        [TestMethod, Priority(0)]
        public void TestRunAll() {
            
            var executor = new TestExecutor();
            var recorder = new MockTestExecutionRecorder();
            var runContext = new MockRunContext();
            var expectedTests = TestInfo.TestAdapterATests.Concat(TestInfo.TestAdapterBTests).ToArray();

            executor.RunTests(new[] { TestInfo.TestAdapterLibProjectFilePath, TestInfo.TestAdapterAProjectFilePath, TestInfo.TestAdapterBProjectFilePath }, runContext, recorder);
            PrintTestResults(recorder.Results);

            foreach (var expectedResult in expectedTests) {
                var actualResult = recorder.Results.SingleOrDefault(tr => tr.TestCase.FullyQualifiedName == expectedResult.TestCase.FullyQualifiedName);

                Assert.IsNotNull(actualResult, expectedResult.TestCase.FullyQualifiedName + " not found in results");
                Assert.AreEqual(expectedResult.Outcome, actualResult.Outcome, expectedResult.TestCase.FullyQualifiedName + " had incorrect result");
            }
        }

        [TestMethod, Priority(0)]
        public void TestCancel() {
            
            var executor = new TestExecutor();
            var recorder = new MockTestExecutionRecorder();
            var runContext = new MockRunContext();
            var expectedTests = TestInfo.TestAdapterATests.Union(TestInfo.TestAdapterBTests).ToArray();
            var testCases = expectedTests.Select(tr => tr.TestCase);

            var thread = new System.Threading.Thread(o => {
                executor.RunTests(testCases, runContext, recorder);
            });
            thread.Start();

            // One of the tests being run is hard coded to take 10 secs
            Assert.IsTrue(thread.IsAlive);

            System.Threading.Thread.Sleep(100);

            executor.Cancel();
            System.Threading.Thread.Sleep(100);

            // It should take less than 10 secs to cancel
            // Depending on which assemblies are loaded, it may take some time
            // to obtain the interpreters service.
            Assert.IsTrue(thread.Join(10000));

            System.Threading.Thread.Sleep(100);

            Assert.IsFalse(thread.IsAlive);

            // Canceled test cases do not get recorded
            Assert.IsTrue(recorder.Results.Count < expectedTests.Length);
        }

        private static void PrintTestResults(IEnumerable<TestResult> results) {
            foreach (var result in results) {
                Console.WriteLine("Test: " + result.TestCase.FullyQualifiedName);
                Console.WriteLine("Result: " + result.Outcome);
                foreach(var msg in result.Messages) {
                    Console.WriteLine("Message " + msg.Category + ":");
                    Console.WriteLine(msg.Text);
                }
                Console.WriteLine("");
            }
        }
    }
}
