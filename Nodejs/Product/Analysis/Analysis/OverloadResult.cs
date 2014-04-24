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
using Microsoft.NodejsTools.Interpreter;

namespace Microsoft.NodejsTools.Analysis {
    public class OverloadResult : IOverloadResult {
        private readonly ParameterResult[] _parameters;
        private readonly string _name;

        public OverloadResult(ParameterResult[] parameters, string name) {
            _parameters = parameters;
            _name = name;
        }

        public string Name {
            get { return _name; }
        }
        public virtual string Documentation {
            get { return null; }
        }
        public virtual ParameterResult[] Parameters {
            get { return _parameters; }
        }
    }

    class SimpleOverloadResult : OverloadResult {
        private readonly string _documentation;
        public SimpleOverloadResult(ParameterResult[] parameters, string name, string documentation)
            : base(parameters, name) {
            _documentation = documentation;
        }

        public override string Documentation {
            get {
                return _documentation;
            }
        }
    }

    class OverloadResultComparer : EqualityComparer<OverloadResult> {
        public static IEqualityComparer<OverloadResult> Instance = new OverloadResultComparer();

        public override bool Equals(OverloadResult x, OverloadResult y) {
            if (x == null | y == null) {
                return x == null & y == null;
            }

            if (x.Name != y.Name || x.Documentation != y.Documentation) {
                return false;
            }

            if (x.Parameters == null | y.Parameters == null) {
                return x.Parameters == null & y.Parameters == null;
            }

            if (x.Parameters.Length != y.Parameters.Length) {
                return false;
            }

            for (int i = 0; i < x.Parameters.Length; ++i) {
                if (!x.Parameters[i].Equals(y.Parameters[i])) {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode(OverloadResult obj) {
            // Don't use Documentation for hash code, since it changes over time
            // in some implementations of IOverloadResult.
            int hc = 552127 ^ obj.Name.GetHashCode();
            if (obj.Parameters != null) {
                foreach (var p in obj.Parameters) {
                    hc ^= p.GetHashCode();
                }
            }
            return hc;
        }
    }
}
