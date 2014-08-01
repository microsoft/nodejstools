/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.NodejsTools.Analysis {
    /// <summary>
    /// Provides a wrapper around our AnalysisValue's.  Wrappers are invalidated
    /// when new versions of projects are defined effectively removing the old
    /// values from the system.  Our various hashing primitives will wipe out
    /// the old values but the old values also won't keep a large amount of
    /// state around.
    /// </summary>
    [Serializable]
    public sealed class AnalysisProxy : IDetachableProxy, IImmutableAnalysisSet {
        private AnalysisValue _value;

        public AnalysisProxy(AnalysisValue value) {
            _value = value;
        }

        public AnalysisValue Value {
            get {
                return _value;
            }
        }

        public void NewVersion() {
            _value = null;
        }

        public bool IsAlive {
            get { return _value != null; }
        }

        IAnalysisSet IAnalysisSet.Add(AnalysisProxy item, out bool wasChanged) {
            if (((IAnalysisSet)this).Comparer.Equals(this, item)) {
                wasChanged = false;
                return this;
            }
            wasChanged = true;
            return new AnalysisSetDetails.AnalysisSetTwoObject(this, item);
        }

        IAnalysisSet IAnalysisSet.Union(IEnumerable<AnalysisProxy> items, out bool wasChanged) {
            if (items.All(av => ((IAnalysisSet)this).Comparer.Equals(this, av))) {
                wasChanged = false;
                return this;
            }
            wasChanged = true;
            return AnalysisSet.Create(items).Add(this);
        }

        IAnalysisSet IAnalysisSet.Clone() {
            return this;
        }

        bool IAnalysisSet.Contains(AnalysisProxy item) {
            return ((IAnalysisSet)this).Comparer.Equals(this, item);
        }

        int IAnalysisSet.Count {
            get {
                if (IsAlive) {
                    return 1;
                }
                return 0;
            }
        }

        IEqualityComparer<AnalysisProxy> IAnalysisSet.Comparer {
            get { return ObjectComparer.Instance; }
        }

        IEnumerator<AnalysisProxy> IEnumerable<AnalysisProxy>.GetEnumerator() {
            if (IsAlive) {
                yield return this;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return ((IEnumerable<AnalysisProxy>)this).GetEnumerator();
        }

        public override string ToString() {
            if (_value != null) {
                return _value.ToString();
            }
            return "<detached proxy>";
        }
    }
}
