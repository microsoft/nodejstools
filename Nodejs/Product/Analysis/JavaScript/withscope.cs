// withscope.cs
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

using System.Reflection;

namespace Microsoft.NodejsTools.Parsing
{
    public sealed class WithScope : BlockScope
    {
        public WithScope(ActivationObject parent, IndexSpan span, ErrorSink errorSink)
            : base(parent, span, errorSink)
        {
            IsInWithScope = true;
        }

        public override JSVariableField CreateInnerField(JSVariableField outerField)
        {
            return outerField.IfNotNull(o =>
            {
                // blindly create an inner reference field for with scopes, no matter what it
                // is. globals and predefined values can be hijacked by object properties in
                // this scope.
                var withField = AddField(CreateField(outerField));

                return withField;
            });
        }

        /// <summary>
        /// Set up this scopes lexically-declared fields
        /// </summary>
        public override void DeclareScope(ResolutionVisitor resolutionVisitor)
        {
            // only bind lexical declarations
            DefineLexicalDeclarations(resolutionVisitor);
        }

        public override JSVariableField CreateField(JSVariableField outerField)
        {
            // when we create a field inside a with-scope, it's ALWAYS a with-field, no matter
            // what type the outer reference is.
            return new JSVariableField(FieldType.WithField, outerField);
        }

        public override JSVariableField CreateField(string name)
        {
            return new JSVariableField(FieldType.WithField, name);
        }
    }
}
