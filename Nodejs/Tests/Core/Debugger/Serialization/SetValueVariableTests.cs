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

using Microsoft.NodejsTools.Debugger;
using Microsoft.NodejsTools.Debugger.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace NodejsTests.Debugger.Serialization {
    [TestClass]
    public class SetValueVariableTests {
        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void CreateSetVariableValue() {
            // Arrange
            JObject json = SerializationTestData.GetSetVariableValueResponse();
            const int frameId = 3;
            var stackFrame = new NodeStackFrame(frameId);
            const string name = "name";

            // Act
            var result = new NodeSetValueVariable(stackFrame, name, json);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(NodePropertyAttributes.None, result.Attributes);
            Assert.IsNull(result.Class);
            Assert.AreEqual(44, result.Id);
            Assert.AreEqual(name, result.Name);
            Assert.IsNull(result.Parent);
            Assert.AreEqual(stackFrame, result.StackFrame);
            Assert.AreEqual("55", result.Text);
            Assert.AreEqual(NodePropertyType.Normal, result.Type);
            Assert.AreEqual("number", result.TypeName);
            Assert.AreEqual("55", result.Value);
        }
    }
}