// functionobject.cs
//
// Copyright 2010 Microsoft Corporation
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace Microsoft.NodejsTools.Parsing {
    [Serializable]
    public sealed class FunctionObject : Statement, INameDeclaration {
        private Block m_body;
        private AstNodeList<ParameterDeclaration> m_parameters;
        public FunctionObject(IndexSpan functionSpan)
            : base(functionSpan) {
        }

        public Block Body {
            get { return m_body; }
            set {
                m_body.ClearParent(this);
                m_body = value;
                m_body.AssignParent(this);
            }
        }

        public AstNodeList<ParameterDeclaration> ParameterDeclarations {
            get { return m_parameters; }
            set {
                m_parameters.ClearParent(this);
                m_parameters = value;
                m_parameters.AssignParent(this);
            }
        }

        public FunctionType FunctionType { get; set; }

        public Expression Initializer { get { return null; } }

        public string Name { get; set; }

        public string NameGuess { get; set; }

        // TODO: NameIndex?
        public IndexSpan NameSpan {
            get;
            set;
        }


        public IndexSpan ParametersSpan { get; set; }
        public bool IsExpression { get; set; }

        public int ParameterStart {
            get {
                return ParametersSpan.Start;
            }
        }

        public int ParameterEnd {
            get {
                return ParametersSpan.End;
            }
        }

        public JSVariableField VariableField { get; set; }
        
        public override void Walk(AstVisitor walker) {
            if (walker.Walk(this)) {
                if (m_parameters != null) {
                    foreach (var param in m_parameters) {
                        param.Walk(walker);
                    }
                }

                if (Body != null) {
                    Body.Walk(walker);
                }
            }
            walker.PostWalk(this);
        }

        public override IEnumerable<Node> Children {
            get {
                return EnumerateNonNullNodes(ParameterDeclarations, Body);
            }
        }
    }
}