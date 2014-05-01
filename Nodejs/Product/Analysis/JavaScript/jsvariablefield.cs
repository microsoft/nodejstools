// jsvariablefield.cs
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
using System.Diagnostics;
using System.Reflection;

namespace Microsoft.NodejsTools.Parsing
{
    /// <summary>
    /// Field type enumeration
    /// </summary>
    public enum FieldType
    {
        Local,
        Predefined,
        Global,
        Arguments,
        Argument,
        WithField,
        CatchError,
        GhostCatch,
        GhostFunction,
        UndefinedGlobal,
    }

    public class JSVariableField
    {
        private Statement _scope;
        public IndexSpan OriginalSpan { get; set; }
        public string Name { get; private set; }
        public FieldType FieldType { get; set; }
        public JSVariableField OuterField { get; set; }

        public JSVariableField(FieldType fieldType, string name)
        {
            FieldType = fieldType;
            Name = name;
        }

        internal JSVariableField(FieldType fieldType, JSVariableField outerField)
        {
            if (outerField == null)
            {
                throw new ArgumentNullException("outerField");
            }

            // set values based on the outer field
            OuterField = outerField;

            Name = outerField.Name;
        }

        public Statement Scope {
            get {
                // but the get -- if we are an inner field, we always
                // want to get the owning scope of the outer field
                return OuterField == null ? _scope : OuterField.Scope;
            }
            set {
                // simple set -- should always point to the scope in whose
                // name table this field has been added, which isn't necessarily
                // the owning scope, because this may be an inner field. But keep
                // this value in case we ever break the link to the outer field.
                _scope = value;
            }
        }

        public virtual int Position {
            get {
                return -1;
            }
        }

        public override string ToString()
        {
            return Name;
        }


        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }

    class JSArgumentField : JSVariableField {
        private readonly int _position;

        public JSArgumentField(FieldType fieldType, string name, int position)
            : base(fieldType, name) {
                _position = position;
        }

        public override int Position {
            get {
                return _position;
            }
        }
    }
}