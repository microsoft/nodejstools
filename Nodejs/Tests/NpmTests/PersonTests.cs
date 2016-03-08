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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.NodejsTools.Npm.SPI;

namespace NpmTests {
    [TestClass]
    public class PersonTests {
        [TestMethod, TestCategory("UnitTest")]
        public void ShouldReturnEmptyPersonForNullOrEmptyInput() {
            var sources = new[] { null, "", " ", "    " };
            foreach (var emptySource in sources) {
                var person = Person.CreateFromJsonSource(null);
                Assert.IsNotNull(person);
                Assert.AreEqual("", person.Name);
                Assert.IsNull(person.Email);
                Assert.IsNull(person.Url);
            }
        }

        [TestMethod, TestCategory("UnitTest")]
        public void ShouldReturnNameForObjectWithNameOnly() {
            var name = "J Scripter";
            var sources = new[] {
                @"{{""name"": ""{0}""}}",
                @"{{""name"":""{0}""}}",
                @"{{""name"":    ""{0}""}}",
                @"{{      ""name"":     ""{0}""      }}",
            };
            foreach (var source in sources) {
                var json = string.Format(source, name);
                var person = Person.CreateFromJsonSource(json);
                Assert.IsNotNull(person);
                Assert.AreEqual(name, person.Name);
                Assert.IsNull(person.Email);
                Assert.IsNull(person.Url);
            }
        }

        [TestMethod, TestCategory("UnitTest")]
        public void ShouldSetNameEmailAndUrlForObjectWithTheseProperties() {
            var name = "J Scripter";
            var email = "j@contoso.com";
            var url = "http://contoso.com";

            var sources = new[] {
                @"{{""name"": ""{0}"", ""email"": ""{1}"", ""url"": ""{2}""}}",
                @"{{""url"": ""{2}"", ""email"": ""{1}"", ""name"": ""{0}""}}",
                // Ignore other properties
                @"{{""handle"": ""@code"", ""url"": ""{2}"", ""email"": ""{1}"", ""office"": ""1337"", ""name"": ""{0}""}}",
            };

            foreach (var source in sources) {
                var json = string.Format(source, name, email, url);
                var person = Person.CreateFromJsonSource(json);
                Assert.IsNotNull(person);
                Assert.AreEqual(name, person.Name);
                Assert.AreEqual(email, person.Email);
                Assert.AreEqual(url, person.Url);
            }
        }

        [TestMethod, TestCategory("UnitTest")]
        public void ShouldReturnInputAsNameIfInputIsNotJsonStringOrObject() {
            var name = "J Scripter";
            var person = Person.CreateFromJsonSource(name);
            Assert.IsNotNull(person);
            Assert.AreEqual(name, person.Name);
            Assert.IsNull(person.Email);
            Assert.IsNull(person.Url);
        }

        [TestMethod, TestCategory("UnitTest")]
        public void ShouldTreatStringAsName() {
            var person = Person.CreateFromJsonSource(@"""J Scripter""");
            Assert.IsNotNull(person);
            Assert.AreEqual("J Scripter", person.Name);
            Assert.IsNull(person.Email);
            Assert.IsNull(person.Url);
        }
    }
}
