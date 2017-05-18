// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.


using System.ComponentModel;

using Microsoft.NodejsTools.Project;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NodejsTests
{
    [TestClass]
    public class TestFrameworkStringConverterTest
    {
        [TestMethod, Priority(0), TestCategory("Ignore")]
        public void GetStandardValues_CheckValueSequence()
        {
            //Arrange
            TestFrameworkStringConverter convert = new TestFrameworkStringConverter();
            //Act
            TypeConverter.StandardValuesCollection values = convert.GetStandardValues(null);
            //Assert
            Assert.AreEqual("ExportRunner", values[0]);
            Assert.AreEqual("mocha", values[1]);
            Assert.AreEqual(2, values.Count);
        }
    }
}

