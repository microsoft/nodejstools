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
using System.Linq;
using Microsoft.NodejsTools.Debugger.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NodejsTests.Debugger.Serialization {
    [TestClass]
    public class SequentialNumberGeneratorTests {
        [TestMethod]
        public void RetrieveFirstNumber() {
            // Arrange
            var numberGenerator = new SequentialNumberGenerator();

            // Act
            int number = numberGenerator.GetNext();

            // Assert
            Assert.AreEqual(1, number);
        }

        [TestMethod]
        public void RetrieveNumberSequence() {
            // Arrange
            var numberGenerator = new SequentialNumberGenerator();
            List<int> idealSequence = Enumerable.Range(0, 100).ToList();

            // Act
            List<int> numbers = idealSequence.Select(p => numberGenerator.GetNext()).ToList();

            // Assert
            Assert.IsTrue(idealSequence.Select(p => p == numbers[p] - 1).All(p => p));
        }

        [TestMethod]
        public void UseResetInNumberGenerator() {
            // Arrange
            var numberGenerator = new SequentialNumberGenerator();
            const int count = 20;
            var numbers = new List<int>(count);

            // Act
            for (int i = 0; i < count; i++) {
                if (i%10 == 0) {
                    numberGenerator.Reset();
                }
                numbers.Add(numberGenerator.GetNext());
            }

            // Assert
            Assert.AreEqual(20, numbers.Count);
            for (int i = 0; i < 10; i++) {
                Assert.AreEqual(numbers[i], numbers[i + 10]);
            }
        }
    }
}