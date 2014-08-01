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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.NodejsTools.Analysis;
using Microsoft.NodejsTools.Analysis.AnalysisSetDetails;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AnalysisTests {
    [TestClass]
    public class AnalysisHashSetTests {
        [TestMethod, Priority(0)]
        public void TestEmpty() {
            TestEmptySet(AnalysisSet.Empty);
            TestObjectSet(AnalysisSet.Empty);
            TestEmptySet(AnalysisSet.EmptyUnion);
        }

        private static void TestEmptySet(IAnalysisSet emptySet) {
            Assert.AreEqual(0, emptySet.Count);
            TestImmutableSet(emptySet);
        }

        [TestMethod, Priority(0)]
        public void TestWrapperSelfSet() {
            var projectEntry = CreateProjectEntry();
            var value = new TestAnalysisValue(projectEntry);

            Assert.IsTrue(((IAnalysisSet)value.Proxy).Contains(value.Proxy));
            TestImmutableSet(value.Proxy);
        }

        [TestMethod, Priority(0)]
        public void TestSetOfOne() {
            var projectEntry = CreateProjectEntry();
            var value = new TestAnalysisValue(projectEntry);
            
            TestImmutableSet(value.Proxy);
        }

        [TestMethod, Priority(0)]
        public void TestSetOfTwo() {
            var projectEntry = CreateProjectEntry();
            var value1 = new TestAnalysisValue(projectEntry);
            var value2 = new TestAnalysisValue(projectEntry);

            TestImmutableSet(new AnalysisSetTwoObject(value1.Proxy, value2.Proxy));
        }

        [TestMethod, Priority(0)]
        public void TestHashSet() {
            var projectEntry = CreateProjectEntry();
            List<AnalysisProxy> values = new List<AnalysisProxy>();
            for (int i = 0; i < 10; i++) {
                values.Add(new TestAnalysisValue(projectEntry).Proxy);
            }

            var set = new AnalysisHashSet();
            for (int i = 0; i < values.Count; i++) {
                Assert.IsFalse(set.Contains(values[i]));
                Assert.AreEqual(set, set.Add(values[i]));
                Assert.AreEqual(i + 1, set.Count);
                Assert.IsTrue(set.Contains(values[i]));

                for (int j = 0; j <= i; j++) {
                    Assert.AreEqual(set, set.Add(values[j]));
                    Assert.AreEqual(i + 1, set.Count);
                }
            }

            foreach (var enumMaker in EnumMaker) {
                set = new AnalysisHashSet();
                for (int i = 0; i < values.Count; i++) {
                    Assert.IsFalse(set.Contains(values[i]));
                    Assert.AreEqual(set, set.Union(enumMaker(values[i])));
                    Assert.AreEqual(i + 1, set.Count);
                    Assert.IsTrue(set.Contains(values[i]));

                    for (int j = 0; j <= i; j++) {
                        Assert.AreEqual(set, set.Union(enumMaker(values[j])));
                        Assert.AreEqual(i + 1, set.Count);
                    }
                }
            }

            set = new AnalysisHashSet();
            for (int i = 0; i < values.Count; i++) {
                Assert.IsFalse(set.Contains(values[i]));
                bool wasChanged;
                Assert.AreEqual(set, set.Add(values[i], out wasChanged));
                Assert.AreEqual(true, wasChanged);
                Assert.AreEqual(i + 1, set.Count);
                Assert.IsTrue(set.Contains(values[i]));

                for (int j = 0; j <= i; j++) {                    
                    Assert.AreEqual(set, set.Add(values[j], out wasChanged));
                    Assert.IsFalse(wasChanged);
                    Assert.AreEqual(i + 1, set.Count);
                }
            }

            foreach (var enumMaker in EnumMaker) {
                set = new AnalysisHashSet();
                for (int i = 0; i < values.Count; i++) {
                    Assert.IsFalse(set.Contains(values[i]));
                    bool wasChanged;
                    Assert.AreEqual(set, set.Union(enumMaker(values[i]), out wasChanged));
                    Assert.IsTrue(wasChanged);
                    Assert.AreEqual(i + 1, set.Count);

                    for (int j = 0; j <= i; j++) {
                        Assert.AreEqual(set, set.Union(enumMaker(values[j]), out wasChanged));
                        Assert.IsFalse(wasChanged);
                        Assert.AreEqual(i + 1, set.Count);
                    }
                }
            }
        }

        private static Func<AnalysisProxy, IEnumerable<AnalysisProxy>>[] EnumMaker = new Func<AnalysisProxy, IEnumerable<AnalysisProxy>>[] {
            x => x,
            x => new[] { x }
        };

        private static void TestObjectSet(IAnalysisSet set) {
            Assert.AreEqual(ObjectComparer.Instance, set.Comparer);
        }

        private static void TestUnionSet(IAnalysisSet set) {
            Assert.IsTrue(set.Comparer is UnionComparer);
        }

        private static void TestImmutableSet(IAnalysisSet emptySet) {
            int count = emptySet.Count;

            var projectEntry = CreateProjectEntry();
            var value = new TestAnalysisValue(projectEntry);

            var newSet = emptySet.Add(value.Proxy);
            Assert.AreNotEqual(emptySet, newSet);
            Assert.AreEqual(count, emptySet.Count);
            Assert.AreEqual(count + 1, newSet.Count);

            bool wasChanged;
            newSet = emptySet.Add(value.Proxy, out wasChanged);
            Assert.AreNotEqual(emptySet, newSet);
            Assert.IsTrue(wasChanged);
            Assert.AreEqual(count, emptySet.Count);
            Assert.AreEqual(count + 1, newSet.Count);

            newSet = emptySet.Union(new[] { value.Proxy });
            Assert.AreNotEqual(emptySet, newSet);
            Assert.AreEqual(count, emptySet.Count);
            Assert.AreEqual(count + 1, newSet.Count);

            newSet = emptySet.Union(new[] { value.Proxy }, out wasChanged);
            Assert.IsTrue(wasChanged);
            Assert.AreNotEqual(emptySet, newSet);
            Assert.AreEqual(count, emptySet.Count);
            Assert.AreEqual(count + 1, newSet.Count);

            Assert.AreEqual(emptySet, emptySet.Clone());

            Assert.IsFalse(emptySet.Contains(value.Proxy));
        }

        private static ProjectEntry CreateProjectEntry() {
            var analyzer = new JsAnalyzer();
            return (ProjectEntry)analyzer.AddModule("test.js");            
        }
        
        class TestAnalysisValue : AnalysisValue {
            public TestAnalysisValue(ProjectEntry project)
                : base(project) {
            }
        }
    }
}
