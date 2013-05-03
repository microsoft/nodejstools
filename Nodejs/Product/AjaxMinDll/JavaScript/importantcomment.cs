// importantcomment.cs
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

namespace Microsoft.Ajax.Utilities
{
    public class ImportantComment : AstNode
    {
        public string Comment { get; private set; }

        public ImportantComment(Context context, JSParser parser)
            : base(context, parser)
        {
            if (parser != null && parser.Settings.OutputMode == OutputMode.SingleLine)
            {
                // if we are in single-line mode, we want to replace all CRLF pairs
                // with just the LF to save output bytes.
                Comment = Context.Code.Replace("\r\n", "\n");
            }
            else
            {
                // multi-line mode, just leave it as-is
                Comment = Context.Code;
            }
        }

        public override void Accept(IVisitor visitor)
        {
            if (visitor != null)
            {
                visitor.Visit(this);
            }
        }

        internal override bool RequiresSeparator
        {
            get
            {
                // never requires a separator because we always line-break after the comment
                return false;
            }
        }
    }
}
