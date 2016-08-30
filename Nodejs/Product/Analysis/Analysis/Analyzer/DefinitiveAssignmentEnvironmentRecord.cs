using System;
using System.Collections.Generic;
using Microsoft.NodejsTools.Parsing;

namespace Microsoft.NodejsTools.Analysis.Analyzer {
    [Serializable]
    class DefinitiveAssignmentEnvironmentRecord : StatementEnvironmentRecord {
        private readonly string _name;
        private VariableDef _variable;

        public DefinitiveAssignmentEnvironmentRecord(int startIndex, string name, EnvironmentRecord outerRecord)
            : base(startIndex, outerRecord) {
            _name = name;
            _variable = new VariableDef();
        }

        public override IEnumerable<KeyValuePair<string, VariableDef>> LocalVariables {
            get {
                return new[] { new KeyValuePair<string, VariableDef>(_name, _variable) };
            }
        }

        public override IEnumerable<KeyValuePair<string, VariableDef>> Variables {
            get {
                yield return new KeyValuePair<string, VariableDef>(_name, _variable);
                foreach(var keyValue in base.Variables) {
                    if (keyValue.Key != _name) {
                        yield return keyValue;
                    }
                }
            }
        }

        public override bool AssignVariable(string name, Node location, AnalysisUnit unit, IAnalysisSet values) {
            if (name == _name) {
                // we assign to our copy of the variable
                var res = AssignVariableWorker(location, unit, values, _variable);

                // and then assign it to our parent declarative environment so that
                // it can be read from locations where it's not definitely assigned.
                EnvironmentRecord declScope = GetDeclarativeEnvironment();
                while (declScope.Parent != null &&
                    (!declScope.ContainsVariable(name) || declScope is DeclarativeEnvironmentRecord)) {
                    declScope = declScope.Parent;
                }
                declScope.AssignVariable(name, location, unit, values);

                return res;
            }

            return base.AssignVariable(name, location, unit, values);
        }

        private DeclarativeEnvironmentRecord GetDeclarativeEnvironment() {
            var env = Parent;
            while (!(env is DeclarativeEnvironmentRecord)) {
                env = env.Parent;
            }
            return env as DeclarativeEnvironmentRecord;
        }

        internal override void ReplaceVariable(string name, VariableDef def) {
            if (_name != name) {
                Parent.ReplaceVariable(name, def);
            } else {
                _variable = def;
            }
        }

        public override bool TryGetVariable(string name, out VariableDef variable) {
            if (name == _name) {
                variable = _variable;
                return true;
            }
            return base.TryGetVariable(name, out variable);
        }

        public override bool ContainsVariable(string name) {
            if (name == _name) {
                return true;
            }
            return base.ContainsVariable(name);
        }

        public override VariableDef GetVariable(string name) {
            if (name == _name) {
                return _variable;
            }
            return base.GetVariable(name);
        }

        public override VariableDef GetVariable(Node node, AnalysisUnit unit, string name, bool addRef = true) {
            if (name == _name) {
                if (addRef) {
                    _variable.AddReference(node, unit);
                }
                return _variable;
            }
            return base.GetVariable(node, unit, name, addRef);
        }

        public override VariableDef CreateVariable(Node node, AnalysisUnit unit, string name, bool addRef = true) {
            if (name == _name) {
                if (addRef) {
                    _variable.AddReference(node, unit);
                }
                return _variable;
            }
            return base.CreateVariable(node, unit, name, addRef);
        }

        public override VariableDef CreateEphemeralVariable(Node node, AnalysisUnit unit, string name, bool addRef = true) {
            if (name == _name) {
                if (addRef) {
                    _variable.AddReference(node, unit);
                }
                return _variable;
            }
            return base.CreateEphemeralVariable(node, unit, name, addRef);
        }

        public override VariableDef GetOrAddVariable(string name) {
            if (name == _name) {
                return _variable;
            }
            return base.GetOrAddVariable(name);
        }

        public override VariableDef AddVariable(string name, VariableDef variable = null) {
            if (name == _name) {
                return _variable;
            }
            return base.AddVariable(name, variable);
        }
    }
}
