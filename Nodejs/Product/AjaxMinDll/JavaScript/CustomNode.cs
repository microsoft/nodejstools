// CustomNode.cs
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

namespace Microsoft.Ajax.Utilities
{
    /// <summary>
    /// This is a base-class for any custom AST nodes someone may want to implement. It allows 
    /// these nodes to be hooked into the IVisitor framework. If you wish to create custom AST nodes,
    /// derive from this class.
    /// </summary>
    public class CustomNode : AstNode
    {
        public CustomNode(Context context, JSParser parser)
            : base(context, parser)
        {
        }

        public override void Accept(IVisitor visitor)
        {
            if (visitor != null)
            {
                visitor.Visit(this);
            }
        }

        public override string ToCode()
        {
            // by default, this node produces nothing in the output.
            // the OutputVisitor will output the results of ToCode, so
            // any derived class that wants to insert code into the output
            // should override this method.
            return string.Empty;
        }
    }
}
