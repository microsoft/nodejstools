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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.NodejsTools.Analysis.Values;
using Microsoft.NodejsTools.Parsing;

namespace Microsoft.NodejsTools.Analysis.Analyzer {
    abstract class EnvironmentRecord {
        public readonly EnvironmentRecord Parent;
        private List<EnvironmentRecord> _children;

        public EnvironmentRecord(EnvironmentRecord outerScope) {
            Parent = outerScope;
        }

        public virtual IAnalysisSet ThisValue {
            get {
                // TODO: Global object?
                return AnalysisSet.Empty;
            }
        }

        public virtual IAnalysisSet MergedThisValue {
            get {
                return ThisValue;
            }
        }

        protected EnvironmentRecord(EnvironmentRecord cloned, bool isCloned) {
            Debug.Assert(isCloned);
            if (cloned.HasChildren) {
                _children = new List<EnvironmentRecord>();
                _children.AddRange(cloned.Children);
            }
        }

        public bool HasChildren {
            get {
                return _children != null && _children.Count > 0;
            }
        }

        public List<EnvironmentRecord> Children {
            get {
                if (_children == null) {
                    _children = new List<EnvironmentRecord>();
                }
                return _children;
            }
        }

        public EnvironmentRecord GlobalEnvironment {
            get {
                for (var scope = this; scope != null; scope = scope.Parent) {
                    if (scope.Parent == null) {
                        return scope;
                    }
                }
                return null;
            }
        }

        public IEnumerable<EnvironmentRecord> EnumerateTowardsGlobal {
            get {
                for (var scope = this; scope != null; scope = scope.Parent) {
                    yield return scope;
                }
            }
        }

        public IEnumerable<EnvironmentRecord> EnumerateFromGlobal {
            get {
                return EnumerateTowardsGlobal.Reverse();
            }
        }

        /// <summary>
        /// Gets the index in the file/buffer that the scope actually starts on.  This is the index where the colon
        /// is on for the start of the body if we're a function or class definition.
        /// </summary>
        public abstract int GetBodyStart(JsAst ast);

        /// <summary>
        /// Gets the index in the file/buffer that this scope starts at.  This is the index which includes
        /// the definition it's self (e.g. def fob(...) or class fob(...)).
        /// </summary>
        public abstract int GetStart(JsAst ast);

        /// <summary>
        /// Gets the index in the file/buffer that this scope ends at.
        /// </summary>
        public abstract int GetStop(JsAst ast);

        public abstract string Name {
            get;
        }

        #region Variables

        public abstract IEnumerable<KeyValuePair<string, VariableDef>> Variables {
            get;
        }

        public abstract bool TryGetVariable(string name, out VariableDef variable);

        public abstract bool ContainsVariable(string name);

        /// <summary>
        /// Assigns a variable in the given environment, creating the variable if necessary, and performing
        /// any environment specific behavior such as propagating to outer environments (is instance)
        /// 
        /// Returns true if a new type has been signed to the variable, false if the variable
        /// is left unchanged.
        /// </summary>
        public virtual bool AssignVariable(string name, Node location, AnalysisUnit unit, IAnalysisSet values) {
            var vars = CreateVariable(location, unit, name, false);

            return AssignVariableWorker(location, unit, values, vars);
        }

        /// <summary>
        /// Handles the base assignment case for assign to a variable, minus variable creation.
        /// </summary>
        protected static bool AssignVariableWorker(Node location, AnalysisUnit unit, IAnalysisSet values, VariableDef vars) {
            vars.AddAssignment(location, unit);
            vars.MakeUnionStrongerIfMoreThan(unit.Analyzer.Limits.AssignedTypes, values);
            return vars.AddTypes(unit, values);
        }

        /// <summary>
        /// Adds a variable that is associated with a particular location in code.
        /// 
        /// Goto definition on the variable can then offer the location in code.
        /// </summary>
        public VariableDef AddLocatedVariable(string name, Node location, AnalysisUnit unit/*, ParameterKind paramKind = ParameterKind.Normal*/) {
            VariableDef value;
            if (!TryGetVariable(name, out value)) {
                VariableDef def = new LocatedVariableDef(unit.DeclaringModuleEnvironment.ProjectEntry, location);
                return AddVariable(name, def);
            } else if (!(value is LocatedVariableDef)) {
                VariableDef def = new LocatedVariableDef(unit.DeclaringModuleEnvironment.ProjectEntry, location, value);
                return AddVariable(name, def);
            } else {
                ((LocatedVariableDef)value).Context = location;
                ((LocatedVariableDef)value).DeclaringVersion = unit.ProjectEntry.AnalysisVersion;
            }
            return value;
        }

        public void SetVariable(Node node, AnalysisUnit unit, string name, IAnalysisSet value, bool addRef = true) {
            var variable = CreateVariable(node, unit, name, false);

            variable.AddTypes(unit, value);
            if (addRef) {
                variable.AddAssignment(node, unit);
            }
        }

        public abstract VariableDef GetVariable(string name);
        public abstract VariableDef GetVariable(Node node, AnalysisUnit unit, string name, bool addRef = true);
        public abstract IEnumerable<KeyValuePair<string, VariableDef>> GetAllMergedVariables();
        public abstract IEnumerable<VariableDef> GetMergedVariables(string name);
        public abstract IAnalysisSet GetMergedVariableTypes(string name);
        public abstract VariableDef CreateVariable(Node node, AnalysisUnit unit, string name, bool addRef = true);
        public abstract VariableDef CreateEphemeralVariable(Node node, AnalysisUnit unit, string name, bool addRef = true);
        public abstract VariableDef GetOrAddVariable(string name);
        public abstract VariableDef AddVariable(string name, VariableDef variable = null);
        internal abstract bool RemoveVariable(string name);
        internal abstract bool RemoveVariable(string name, out VariableDef value);
        internal abstract void ClearVariables();
        public abstract void ClearLinkedVariables();
        internal abstract HashSet<VariableDef> GetLinkedVariables(string saveName);
        internal abstract HashSet<VariableDef> GetLinkedVariablesNoCreate(string saveName);

        #endregion

        #region Node Environment Records

        /// <summary>
        /// Gets the environment record associated with the specified node within
        /// the current environment.  If there is no association in the current
        /// record outer records will be searched
        /// </summary>
        internal bool TryGetNodeEnvironment(Node node, out EnvironmentRecord scope) {
            foreach (var s in EnumerateTowardsGlobal) {
                if (s.TryGetLocalNodeEnvironment(node, out scope)) {
                    return true;
                }
            }
            scope = null;
            return false;
        }
        
        /// <summary>
        /// Adds an environment record associated with the specified node.
        /// </summary>
        public abstract EnvironmentRecord AddNodeEnvironment(Node node, EnvironmentRecord scope);
        internal abstract bool RemoveNodeEnvironment(Node node);
        internal abstract void ClearNodeEnvironments();
        /// <summary>
        /// Gets the environment record associated with the specified node
        /// without recursing through outer records.
        /// </summary>
        internal abstract bool TryGetLocalNodeEnvironment(Node node, out EnvironmentRecord scope);

        /// <summary>
        /// Gets all of the associations between nodes and environment records.
        /// </summary>
        public abstract IEnumerable<KeyValuePair<Node, EnvironmentRecord>> NodeEnvironments {
            get;
        }

        #endregion

        #region Node Values

        public abstract IAnalysisSet AddNodeValue(Node node, IAnalysisSet variable);

        internal abstract bool RemoveNodeValue(Node node);

        internal abstract void ClearNodeValues();

        /// <summary>
        /// Gets the value associated with the specifed node, recursing through
        /// outer environments if it's not defined locally.
        /// </summary>
        internal bool TryGetNodeValue(Node node, out IAnalysisSet variable) {
            foreach (var s in EnumerateTowardsGlobal) {
                if (s.TryGetLocalNodeValue(node, out variable)) {
                    return true;
                }
            }
            variable = null;
            return false;
        }

        /// <summary>
        /// Gets the value associated with the specifed node within the
        /// this environment record.  Outer records are not searched.
        /// </summary>
        internal abstract bool TryGetLocalNodeValue(Node node, out IAnalysisSet variable);

        /// <summary>
        /// Cached node variables so that we don't continually create new entries for basic nodes such
        /// as sequences, lambdas, etc...
        /// </summary>
        public IAnalysisSet GetOrMakeNodeValue(Node node, Func<Node, IAnalysisSet> maker) {
            IAnalysisSet result;
            if (!TryGetNodeValue(node, out result)) {
                result = maker(node);
                AddNodeValue(node, result);
            }
            return result;
        }

        #endregion

        public abstract AnalysisValue AnalysisValue {
            get;
        }
    }
}
