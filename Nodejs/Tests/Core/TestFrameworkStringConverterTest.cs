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


using System.ComponentModel;

using Microsoft.NodejsTools.Project;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudioTools.Project;

namespace NodejsTests {

    [TestClass]
    public class TestFrameworkStringConverterTest {
        [TestMethod, Priority(0)]
        public void GetStandardValues_CheckValueSequence() {
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
