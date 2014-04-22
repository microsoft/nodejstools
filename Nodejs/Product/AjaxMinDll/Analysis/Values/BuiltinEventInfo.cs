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

using Microsoft.NodejsTools.Analysis.Analyzer;
using Microsoft.NodejsTools.Interpreter;
using Microsoft.NodejsTools.Parsing;


namespace Microsoft.NodejsTools.Analysis.Values {
#if FALSE
    internal class BuiltinEventInfo : BuiltinNamespace<IPythonType> {
        private readonly IPythonEvent _value;
        private string _doc;

        public BuiltinEventInfo(IPythonEvent value, JavaScriptAnalyzer projectState)
            : base(value.EventHandlerType, projectState) {
            _value = value;
            _doc = null;
        }

        public override void AugmentAssign(BinaryOperator node, AnalysisUnit unit, IAnalysisSet value)
        {
            base.AugmentAssign(node, unit, value);
            var args = GetEventInvokeArgs(ProjectState);
            foreach (var r in value) {
                r.Call(node, unit, args);
            }
        }

        internal IAnalysisSet[] GetEventInvokeArgs(JavaScriptAnalyzer state) {
            var p = _value.GetEventParameterTypes();

            var args = new IAnalysisSet[p.Count];
            for (int i = 0; i < p.Count; i++) {
                args[i] = state.GetInstance(p[i]).SelfSet;
            }
            return args;
        }

        public override string Description {
            get {
                return "event of type " + _value.EventHandlerType.Name;
            }
        }

        public override PythonMemberType MemberType {
            get {
                return _value.MemberType;
            }
        }

        public override string Documentation {
            get {
                if (_doc == null) {
                    _doc = Utils.StripDocumentation(_value.Documentation);
                }
                return _doc;
            }
        }

        public override ILocatedMember GetLocatedMember() {
            return _value as ILocatedMember;
        }
    }
#endif
}
