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
using Microsoft.NodejsTools.Analysis.Values;
using Microsoft.NodejsTools.Parsing;

namespace Microsoft.NodejsTools.Analysis.Analyzer {
    sealed class ModuleEnvironmentRecord : DeclarativeEnvironmentRecord {
        private readonly ProjectEntry _projectEntry;
        private readonly ModuleValue _module;   // the object which corresponds to "module" in the users module
        private readonly Dictionary<Node, EnvironmentRecord> _nodeScopes;
        private readonly Dictionary<NodeEnvironmentKey, IAnalysisSet> _nodeValues;

        public ModuleEnvironmentRecord(ModuleValue module, ProjectEntry projectEntry)
            : base(null) {
            _module = module;
            _projectEntry = projectEntry;
            _nodeScopes = new Dictionary<Node, EnvironmentRecord>();
            _nodeValues = new Dictionary<NodeEnvironmentKey, IAnalysisSet>();
        }

        private ModuleEnvironmentRecord(ModuleEnvironmentRecord scope)
            : base(scope, true) {
            _module = scope.Module;
            _projectEntry = scope.ProjectEntry;
            _nodeScopes = scope._nodeScopes;
            _nodeValues = scope._nodeValues;
        }

        public ModuleValue Module { get { return _module; } }

        public override AnalysisValue AnalysisValue {
            get {
                return _module;
            }
        }

        public override string Name {
            get { return _projectEntry.FilePath; }
        }

        public ModuleEnvironmentRecord CloneForPublish() {
            return new ModuleEnvironmentRecord(this);
        }

        public ProjectEntry ProjectEntry {
            get {
                return _projectEntry;
            }
        }

    
        #region Node Environments

        public IEnumerable<KeyValuePair<Node, EnvironmentRecord>> NodeEnvironments {
            get {
                return _nodeScopes;
            }
        }

        internal bool TryGetNodeEnvironment(Node node, out EnvironmentRecord scope) {
            return _nodeScopes.TryGetValue(node, out scope);
        }

        public EnvironmentRecord AddNodeEnvironment(Node node, EnvironmentRecord scope) {
            return _nodeScopes[node] = scope;
        }

        internal bool RemoveNodeEnvironment(Node node) {
            return _nodeScopes.Remove(node);
        }

        internal void ClearNodeEnvironments() {
            _nodeScopes.Clear();
        }

        #endregion

        #region Node Values


        public IAnalysisSet AddNodeValue(NodeEnvironmentKind kind, object node, IAnalysisSet variable) {
            return _nodeValues[new NodeEnvironmentKey(kind, node)] = variable;
        }

        internal bool RemoveNodeValue(NodeEnvironmentKind kind, object node) {
            return _nodeValues.Remove(new NodeEnvironmentKey(kind, node));
        }

        internal void ClearNodeValues() {
            _nodeValues.Clear();
        }

        internal bool TryGetNodeValue(NodeEnvironmentKind kind, object node, out IAnalysisSet variable) {
            return _nodeValues.TryGetValue(new NodeEnvironmentKey(kind, node), out variable);
        }

        /// <summary>
        /// Cached node variables so that we don't continually create new entries for basic nodes such
        /// as sequences, lambdas, etc...
        /// </summary>
        public IAnalysisSet GetOrMakeNodeValue(NodeEnvironmentKind kind, object node, Func<object, IAnalysisSet> maker) {
            IAnalysisSet result;
            if (!TryGetNodeValue(kind, node, out result)) {
                result = maker(node);
                AddNodeValue(kind, node, result);
            }
            return result;
        }

        struct NodeEnvironmentKey : IEquatable<NodeEnvironmentKey> {
            public readonly NodeEnvironmentKind Kind;
            public readonly object Key;

            public NodeEnvironmentKey(NodeEnvironmentKind kind, object key) {
                Kind = kind;
                Key = key;
            }

            public override bool Equals(object obj) {
                if (obj is NodeEnvironmentKey) {
                    return ((NodeEnvironmentKey)obj).Equals(this);
                }
                return false;
            }

            public override int GetHashCode() {
                return Kind.GetHashCode() ^ Key.GetHashCode();
            }

            public bool Equals(NodeEnvironmentKey other) {
                return Kind == other.Kind &&
                    Key.Equals(other.Key);
            }
        }

        #endregion

    }

    enum NodeEnvironmentKind {
        None,
        /// <summary>
        /// The value stored is a ObjectLiteralValue instance.
        /// 
        /// This is used to key unique values for object literals.
        /// </summary>
        ObjectLiteralValue,
        /// <summary>
        /// The value stored is an ArrayValue instance.
        /// 
        /// This is used to key unique values for array literals.
        /// </summary>
        ArrayValue,
        /// <summary>
        /// The value stored is a Function.
        /// 
        /// This is used to key unique values for function objects.
        /// </summary>
        UserFunctionValue,
        /// <summary>
        /// The value stored is a InheritsPrototypeValue
        /// 
        /// This is used to key unique values for util.inherits calls
        /// </summary>
        InheritsPrototypeValue
    }
}
