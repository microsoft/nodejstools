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
using Microsoft.NodejsTools.Interpreter;
using Microsoft.NodejsTools.Parsing;


namespace Microsoft.NodejsTools.Analysis.Values {
    struct ArgumentSet {
        public readonly IAnalysisSet[] Args;

        public ArgumentSet(IAnalysisSet[] args) {
            this.Args = args;
        }

        public IAnalysisSet SequenceArgs {
            get {
                return Args[Args.Length - 2];
            }
        }

        public IAnalysisSet DictArgs {
            get {
                return Args[Args.Length - 1];
            }
        }
        public int Count {
            get {
                return Args.Length - 2;
            }
        }

        public int CombinationCount {
            get {
                return Args.Take(Count).Where(y => y.Count >= 2).Aggregate(1, (x, y) => x * y.Count);
            }
        }

        public override string ToString() {
            return string.Join(", ", Args.Take(Count).Select(a => a.ToString())) +
                (SequenceArgs.Any() ? ", *" + SequenceArgs.ToString() : "") +
                (DictArgs.Any() ? ", **" + DictArgs.ToString() : "");
        }

        public static bool AreCompatible(ArgumentSet x, ArgumentSet y) {
            return x.Args.Length == y.Args.Length;
        }

        public static ArgumentSet FromArgs(FunctionObject node, AnalysisUnit unit, IAnalysisSet[] args) {
            // TODO: Warn when a keyword argument is provided and it maps to
            // something which is also a positional argument:
            // def f(a, b, c):
            //    print a, b, c
            //
            // f(2, 3, a=42)

            var seqArgs = AnalysisSet.EmptyUnion;
            var dictArgs = AnalysisSet.EmptyUnion;

            int argCount = node.ParameterDeclarations.Count;
            var newArgs = new IAnalysisSet[argCount + 2];
            for (int i = 0; i < node.ParameterDeclarations.Count; ++i)
            {
#if FALSE
                if (node.Parameters[i].Kind == ParameterKind.List) {
                    listArgsIndex = i;
                } else if (node.Parameters[i].Kind == ParameterKind.Dictionary) {
                    dictArgsIndex = i;
                }
#endif
                newArgs[i] = AnalysisSet.Empty;
            }

            var limits = unit.Analyzer.Limits;
            for (int i = 0; i < argCount; ++i) {
                newArgs[i] = ReduceArgs(newArgs[i], limits.NormalArgumentTypes);
            }
            newArgs[argCount] = ReduceArgs(seqArgs, limits.ListArgumentTypes);
            newArgs[argCount + 1] = ReduceArgs(dictArgs, limits.DictArgumentTypes);

            return new ArgumentSet(newArgs);
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
