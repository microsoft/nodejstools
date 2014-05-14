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
using System.Linq;
using Microsoft.NodejsTools.Parsing;


namespace Microsoft.NodejsTools.Analysis.Values {
    struct ArgumentSet {
        public readonly IAnalysisSet[] Args;

        public ArgumentSet(IAnalysisSet[] args) {
            Args = args;
        }

        public int Count {
            get {
                return Args.Length;
            }
        }

        public int CombinationCount {
            get {
                return Args.Take(Count).Where(y => y.Count >= 0).Aggregate(1, (x, y) => x * y.Count);
            }
        }

        public override string ToString() {
            return string.Join(", ", Args.Take(Count).Select(a => a.ToString()));
        }

        public static bool AreCompatible(ArgumentSet x, ArgumentSet y) {
            return x.Args.Length == y.Args.Length;
        }

        public static ArgumentSet FromArgs(FunctionObject node, AnalysisUnit unit, IAnalysisSet[] args) {
            var limits = unit.Analyzer.Limits;
            for (int i = 0; i < args.Length; i++) {
                args[i] = ReduceArgs(args[i], limits.NormalArgumentTypes);
            }

            return new ArgumentSet(args);
        }

        private static IAnalysisSet ReduceArgs(IAnalysisSet args, int limit) {
            for (int j = 0; j <= UnionComparer.MAX_STRENGTH; ++j) {
                if (args.Count > limit) {
                    args = args.AsUnion(j);
                } else {
                    break;
                }
            }
            return args;
        }
    }
}
