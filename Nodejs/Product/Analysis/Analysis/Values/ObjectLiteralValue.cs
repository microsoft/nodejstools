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
using Microsoft.NodejsTools.Parsing;

namespace Microsoft.NodejsTools.Analysis.Values {

    internal class ObjectLiteralValue : ObjectValue {
        private readonly Node _node;

        public ObjectLiteralValue(ProjectEntry projectEntry, Node node, string description = null)
            : base(projectEntry, null, description) {
            _node = node;
            projectEntry.Analyzer.AnalysisValueCreated(typeof(ObjectLiteralValue));
        }

        public override string ToString() {
            return String.Format(
                "Object literal: {0} - {1}\r\n{2}",
                _node.GetStart(ProjectEntry.Tree),
                _node.GetEnd(ProjectEntry.Tree),
                ProjectEntry.FilePath
            );
        }

        internal override bool UnionEquals(AnalysisValue av, int strength) {
            if (strength >= MergeStrength.ToBaseClass) {
                var literal = av as ObjectLiteralValue;
                if (literal != null) {
                    return true;
                }
            }

            if (strength >= MergeStrength.ToBaseClass) {
                var literal = av as ObjectLiteralValue;
                if (literal != null) {
                    // two literals from the same node, these
                    // literals were created by independent function
                    // analysis, merge them together now.
                    return literal._node == _node;
                }
            }
            return base.UnionEquals(av, strength);
        }

        internal override int UnionHashCode(int strength) {
            if (strength >= MergeStrength.ToBaseClass) {
                return typeof(ObjectLiteralValue).GetHashCode();
            }

            if (strength >= MergeStrength.ToBaseClass) {
                return _node.GetHashCode();
            }
            return base.UnionHashCode(strength);
        }

        internal override AnalysisValue UnionMergeTypes(AnalysisValue av, int strength) {
            if (strength >= MergeStrength.ToBaseClass) {
                var literal = av as ObjectLiteralValue;
                if (literal != null) {
                    return this;
                }
            }
            
            if (strength >= MergeStrength.ToBaseClass) {
                var literal = av as ObjectLiteralValue;
                if (literal != null && literal._node == _node) {
                    return this;
                }
            }

            return base.UnionMergeTypes(av, strength);
        }
    }
}
