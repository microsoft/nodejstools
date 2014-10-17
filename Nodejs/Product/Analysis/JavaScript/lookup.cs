// lookup.cs
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

namespace Microsoft.NodejsTools.Parsing
{
    public enum ReferenceType
    {
        Variable,
        Function,
        Constructor
    }

    [Serializable]
    internal sealed class Lookup : Expression, INameReference
    {
        public JSVariableField VariableField { get; internal set; }
        public string Name { get; set; }

        public Lookup(EncodedSpan span)
            : base(span)
        {
        }

        public override void Walk(AstVisitor visitor) {
            if (visitor.Walk(this)) {
            }
            visitor.PostWalk(this);
        }

        //code in parser relies on this.name being returned from here
        public override String ToString()
        {
            return Name;
        }
    }
}
