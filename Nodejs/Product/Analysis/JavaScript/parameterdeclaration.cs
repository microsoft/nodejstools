// parameterdeclaration.cs
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

namespace Microsoft.NodejsTools.Parsing
{
    public sealed class ParameterDeclaration : Statement, INameDeclaration
    {
        public string Name
        {
            get;
            set;
        }

        public int Position { get; set; }

        public JSVariableField VariableField { get; set; }

        public Expression Initializer { get { return null; } }


        public ParameterDeclaration(IndexSpan span)
            : base(span)
        {
        }

        public override void Walk(AstVisitor visitor) {
            if (visitor.Walk(this)) {
                if (Initializer != null) {
                    Initializer.Walk(visitor);
                }
            }
            visitor.PostWalk(this);
        }


        public IndexSpan NameSpan {
            get { return Span; }
        }
    }
}
