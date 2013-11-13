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

using System.Linq;
using Microsoft.NodejsTools.Debugger.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NodejsTests.Debugger {
    [TestClass]
    public class InfrastructureTests {
        [TestMethod, Priority(0)]
        public void NaturalSortComparerTest() {
            // Arrange
            var comparer = new NaturalSortComparer();
            var source = new[] { "2name", "1name", "20name", "3name", "11name" };

            // Act
            string[] destination = source.OrderBy(p => p, comparer).ToArray();

            // Assert
            Assert.AreEqual(source.Length, destination.Length);
            Assert.AreEqual("1name", destination[0]);
            Assert.AreEqual("2name", destination[1]);
            Assert.AreEqual("3name", destination[2]);
            Assert.AreEqual("11name", destination[3]);
            Assert.AreEqual("20name", destination[4]);
        }
    }
}