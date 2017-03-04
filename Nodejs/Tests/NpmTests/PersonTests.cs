// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.


using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.NodejsTools.Npm.SPI;

namespace NpmTests
{
    [TestClass]
    public class PersonTests
    {
        [TestMethod, TestCategory("NpmIntegration")]
        public void ShouldReturnEmptyPersonForNullOrEmptyInput()
        {
            var sources = new[] { null, "", " ", "    " };
            foreach (var emptySource in sources)
            {
                var person = Person.CreateFromJsonSource(null);
                Assert.IsNotNull(person);
                Assert.AreEqual("", person.Name);
                Assert.IsNull(person.Email);
                Assert.IsNull(person.Url);
            }
        }

        [TestMethod, TestCategory("NpmIntegration")]
        public void ShouldReturnNameForObjectWithNameOnly()
        {
            var name = "J Scripter";
            var sources = new[] {
                @"{{""name"": ""{0}""}}",
                @"{{""name"":""{0}""}}",
                @"{{""name"":    ""{0}""}}",
                @"{{      ""name"":     ""{0}""      }}",
            };
            foreach (var source in sources)
            {
                var json = string.Format(source, name);
                var person = Person.CreateFromJsonSource(json);
                Assert.IsNotNull(person);
                Assert.AreEqual(name, person.Name);
                Assert.IsNull(person.Email);
                Assert.IsNull(person.Url);
            }
        }

        [TestMethod, TestCategory("NpmIntegration")]
        public void ShouldSetNameEmailAndUrlForObjectWithTheseProperties()
        {
            var name = "J Scripter";
            var email = "j@contoso.com";
            var url = "http://contoso.com";

            var sources = new[] {
                @"{{""name"": ""{0}"", ""email"": ""{1}"", ""url"": ""{2}""}}",
                @"{{""url"": ""{2}"", ""email"": ""{1}"", ""name"": ""{0}""}}",
                // Ignore other properties
                @"{{""handle"": ""@code"", ""url"": ""{2}"", ""email"": ""{1}"", ""office"": ""1337"", ""name"": ""{0}""}}",
            };

            foreach (var source in sources)
            {
                var json = string.Format(source, name, email, url);
                var person = Person.CreateFromJsonSource(json);
                Assert.IsNotNull(person);
                Assert.AreEqual(name, person.Name);
                Assert.AreEqual(email, person.Email);
                Assert.AreEqual(url, person.Url);
            }
        }

        [TestMethod, TestCategory("NpmIntegration")]
        public void ShouldReturnInputAsNameIfInputIsObject()
        {
            var name = "J Scripter";
            var person = Person.CreateFromJsonSource(name);
            Assert.IsNotNull(person);
            Assert.AreEqual(name, person.Name);
            Assert.IsNull(person.Email);
            Assert.IsNull(person.Url);
        }
    }
}

