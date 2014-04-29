// directiveprologue.cs
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
using System.Text;

namespace Microsoft.NodejsTools.Parsing
{
    public class DirectivePrologue : ConstantWrapper
    {
        public DirectivePrologue(string value, TokenWithSpan context, JSParser parser)
            : base(value, PrimitiveType.String, context, parser)
        {
            // this is a "use strict" directive if the source context is EXACTLY "use strict"
            // don't consider the quotes so it can be " or ' delimiters
            UseStrict = string.Equals(value, "use strict", StringComparison.Ordinal);
        }

        public bool UseStrict { get; private set; }
        public bool IsRedundant { get; set; }

        public override void Walk(AstVisitor visitor) {
            if (visitor.Walk(this)) {
            }
            visitor.PostWalk(this);
        }
    }
}
