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

using System.Collections.Generic;
using Microsoft.NodejsTools.Analysis;
using Microsoft.NodejsTools.Analysis.Values;
using Microsoft.NodejsTools.Interpreter;
using Microsoft.NodejsTools.Parsing;
using System;

namespace Microsoft.NodejsTools.PyAnalysis {
#if FALSE
    internal interface IKnownPythonTypes {
        IPythonType this[BuiltinTypeId id] { get; }
    }

#if FALSE
    internal interface IKnownClasses {
        BuiltinClassInfo this[BuiltinTypeId id] { get; }
    }
#endif

    internal class KnownTypes : IKnownPythonTypes/*, IKnownClasses */{
        internal readonly IPythonType[] _types;
        //internal readonly BuiltinClassInfo[] _classInfos;

        public KnownTypes(JavaScriptAnalyzer state) {
            int count = (int)BuiltinTypeIdExtensions.LastTypeId + 1;
            _types = new IPythonType[count];
            //_classInfos = new BuiltinClassInfo[count];
            // primitives
            _types[(int)BuiltinTypeId.Null] = new BuiltinType("null", BuiltinTypeId.Null);
            _types[(int)BuiltinTypeId.Number] = new BuiltinType("number", BuiltinTypeId.Number);
            _types[(int)BuiltinTypeId.String] = new BuiltinType("string", BuiltinTypeId.String);
            _types[(int)BuiltinTypeId.Boolean] = new BuiltinType("boolean", BuiltinTypeId.Boolean);
            _types[(int)BuiltinTypeId.Undefined] = new BuiltinType("undefined", BuiltinTypeId.Undefined);
            
            _types[(int)BuiltinTypeId.Function] = new BuiltinType("function", BuiltinTypeId.Function);

            _types[(int)BuiltinTypeId.Object] = new BuiltinType("object", BuiltinTypeId.Object);
            // TODO: Specialize __proto__, Object.getPrototypeOf, constructor, etc...
            _types[(int)BuiltinTypeId.Array] = new BuiltinType("object", BuiltinTypeId.Array);

#if FALSE
            for (int i = 0; i < count; i++)
            {
                if (_types[i] != null)
                {
                    _classInfos[i] = new BuiltinClassInfo(_types[i], state);
                }
            }
            ITypeDatabaseReader fallbackDb = null;
            IBuiltinPythonModule fallback = null;

            var interpreter = state.Interpreter;

            for (int value = 0; value < count; ++value) {
                try {
                    _types[value] = interpreter.GetBuiltinType((BuiltinTypeId)value);
                } catch (KeyNotFoundException) {
                    if (fallback == null) {

                        var tempDb = PythonTypeDatabase.CreateDefaultTypeDatabase(state.LanguageVersion.ToVersion());
                        fallbackDb = (ITypeDatabaseReader)tempDb;
                        fallback = tempDb.BuiltinModule;
                    }
                    
                    _types[value] = fallback.GetAnyMember(fallbackDb.GetBuiltinTypeName((BuiltinTypeId)value)) as IPythonType;
                }
                if (_types[value] != null) {
                    _classInfos[value] = state.GetBuiltinType(_types[value]);
                }
            }
#endif
        }

        class BuiltinType : IPythonType
        {
            private readonly string _name;
            private readonly BuiltinTypeId _id;

            public BuiltinType(string name, BuiltinTypeId id)
            {
                _name = name;
                _id = id;
            }

            public string Name
            {
                get { return _name; }
            }

            public string Documentation
            {
                get { return ""; }
            }

            public BuiltinTypeId TypeId
            {
                get { return _id; }
            }

            public IPythonModule DeclaringModule
            {
                get { throw new NotImplementedException(); }
            }

            public bool IsBuiltin
            {
                get { return true; }
            }

            public IMember GetMember(IModuleContext context, string name)
            {
                return null;
            }

            public IEnumerable<string> GetMemberNames(IModuleContext moduleContext)
            {
                return new string[0];
            }

            public PythonMemberType MemberType
            {
                get { return PythonMemberType.Class; }
            }
        }

        IPythonType IKnownPythonTypes.this[BuiltinTypeId id] {
            get {
                return _types[(int)id];
            }
        }
#if FALSE
        BuiltinClassInfo IKnownClasses.this[BuiltinTypeId id] {
            get {
                return _classInfos[(int)id];
            }
        }
#endif
    }
#endif
}
