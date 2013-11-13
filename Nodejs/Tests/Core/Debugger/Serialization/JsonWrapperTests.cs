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

using System.Collections.Generic;
using Microsoft.NodejsTools.Debugger.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NodejsTests.Debugger.Serialization {
    [TestClass]
    public class JsonWrapperTests {
        [TestMethod, Priority(0)]
        public void CreateJsonValue() {
            // Arrange
            var jsonObject = (Dictionary<string, object>)SerializationTestData.GetDeserializedJsonObject();
            var value = new JsonValue(jsonObject);

            // Act
            var stringValue = value.GetValue<string>("stringValue");
            var numberValue = value.GetValue<int>("numberValue");
            JsonValue objectValue = value["objectValue"];
            JsonArray arrayValue = value.GetArray("arrayValue");
            var undefinedValue = value.GetValue<int?>("undefinedValue");
            JsonValue undefinedMember = value["undefinedMember"];

            // Assert
            Assert.IsNotNull(stringValue);
            Assert.AreEqual(jsonObject["stringValue"], stringValue);

            Assert.IsNotNull(numberValue);
            Assert.AreEqual(jsonObject["numberValue"], numberValue);

            Assert.IsNotNull(objectValue);
            var sourceObjectValue = (Dictionary<string, object>)jsonObject["objectValue"];
            Assert.AreEqual(sourceObjectValue["value"], objectValue.GetValue<bool>("value"));

            Assert.IsNotNull(arrayValue);
            Assert.AreEqual(3, arrayValue.Count);
            for (int i = 0; i < 3; i++) {
                Assert.AreEqual(i, arrayValue.GetValue<int>(i));
            }

            Assert.IsNull(undefinedValue);
            Assert.IsNull(undefinedMember);
        }

        [TestMethod, Priority(0)]
        public void CreatePrimitiveJsonArray() {
            // Arrange
            var jsonArray = (object[])SerializationTestData.GetDeserializedPrimitiveJsonArray();

            // Act
            var array = new JsonArray(jsonArray);

            // Assert
            Assert.AreEqual(jsonArray.Length, array.Count);
            for (int i = 0; i < 3; i++) {
                Assert.AreEqual(i, array.GetValue<int>(i));
            }
        }

        [TestMethod, Priority(0)]
        public void CreateComplexJsonArray() {
            // Arrange
            var jsonArray = (object[])SerializationTestData.GetDeserializedComplexJsonArray();
            var array = new JsonArray(jsonArray);

            // Act
            JsonValue person = array[0];
            JsonValue tags = array[1];
            JsonArray childArray = array.GetArray(2);

            // Assert
            Assert.AreEqual(jsonArray.Length, array.Count);

            Assert.IsNotNull(person);
            var firstItem = (Dictionary<string, object>)jsonArray[0];
            Assert.AreEqual(firstItem["age"], person.GetValue<int>("age"));
            Assert.AreEqual(firstItem["fullName"], person.GetValue<string>("fullName"));

            Assert.IsNotNull(tags);
            var secondItem = (Dictionary<string, object>)jsonArray[1];
            Assert.AreEqual(secondItem["head"], tags.GetValue<string>("head"));
            Assert.AreEqual(secondItem["body"], tags.GetValue<string>("body"));

            Assert.IsNotNull(childArray);
            for (int i = 0; i < 3; i++) {
                Assert.AreEqual(i, childArray.GetValue<int>(i));
            }
        }
    }
}