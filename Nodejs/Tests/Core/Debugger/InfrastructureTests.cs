// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Linq;
using Microsoft.NodejsTools.Debugger.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NodejsTests.Debugger
{
    [TestClass]
    public class InfrastructureTests
    {
        [TestMethod, Priority(0)]
        public void NaturalSortComparerTest()
        {
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

