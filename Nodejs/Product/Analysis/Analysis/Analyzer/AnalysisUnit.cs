//*********************************************************//
//    Copyright (c) Microsoft. All rights reserved.
//    
//    Apache 2.0 License
//    
//    You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//    
//    Unless required by applicable law or agreed to in writing, software 
//    distributed under the License is distributed on an "AS IS" BASIS, 
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or 
//    implied. See the License for the specific language governing 
//    permissions and limitations under the License.
//
//*********************************************************//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.NodejsTools.Analysis.Analyzer;
using Microsoft.NodejsTools.Parsing;


namespace Microsoft.NodejsTools.Analysis {
    /// <summary>
    /// Encapsulates a single piece of code which can be analyzed.  Currently this could be a top-level module, a class definition, 
    /// a function definition, or a comprehension scope (generator, dict, set, or list on 3.x).  AnalysisUnit holds onto both the 
    /// AST of the code which is to be analyzed along with the scope in which the object is declared.
    /// 
    /// Our dependency tracking scheme works by tracking analysis units - when we add a dependency it is the current
    /// AnalysisUnit which is dependent upon the variable.  If the value of a variable changes then all of the dependent
    /// AnalysisUnit's will be re-enqueued.  This proceeds until we reach a fixed point.
    /// 
    /// Note that AnalysisUnit contructors / methods are not threadsafe because we expect there to be many AnalysisUnits,
    /// and we want to keep things as performant as possible (which means minimizing locking behavior and whatnot.)
    /// </summary>
    [Serializable]
    internal class AnalysisUnit : ISet<AnalysisUnit> {
        /// <summary>
        /// The AST which will be analyzed when this node is analyzed
        /// </summary>
        public readonly Statement Ast;
        public readonly JsAst Tree;
        internal EnvironmentRecord _env;
        private ModuleEnvironmentRecord _declaringModuleEnv;
        /// <summary>
        /// True if this analysis unit is currently in the queue.
        /// </summary>
        public bool IsInQueue;
        private int _id;
        private static int _idCount;
#if DEBUG
        private long _analysisTime;
        private long _analysisCount;
        private static Stopwatch _sw = new Stopwatch();

        static AnalysisUnit() {
            _sw.Start();
        }
#endif

        internal AnalysisUnit(JsAst ast, EnvironmentRecord environment)
            : this(ast, ast, environment) {
        }

        internal AnalysisUnit(Statement ast, JsAst tree, EnvironmentRecord environment) {
            Ast = ast;
            Tree = tree;
            _env = environment;
            _id = Interlocked.Increment(ref _idCount);

            if (environment != null && !ForEval) {
                ProjectEntry.Analyzer.Log.NewUnit(this);
            }
        }

        internal int Id {
            get {
                return _id;
            }
        }

        /// <summary>
        /// True if this analysis unit is being used to evaluate the result of the analysis.  In this
        /// mode we don't track references or re-queue items.
        /// </summary>
        public bool ForEval {
            get {
                return this is EvalAnalysisUnit;
            }
        }

        internal virtual ModuleEnvironmentRecord GetDeclaringModuleEnvironment() {
            if (_env != null) {
                var env = _env.EnumerateTowardsGlobal.OfType<ModuleEnvironmentRecord>().FirstOrDefault();
                if (env != null) {
                    return env;
                }
            }
            return null;
        }

        /// <summary>
        /// The global scope that the code associated with this analysis unit is declared within.
        /// </summary>
        internal ModuleEnvironmentRecord DeclaringModuleEnvironment {
            get {
                if (_declaringModuleEnv == null) {
                    _declaringModuleEnv = GetDeclaringModuleEnvironment();
                }
                return _declaringModuleEnv;
            }
        }

        /// <summary>
        /// Looks up a sequence of types associated with the name using the
        /// normal JavaScript semantics.
        /// 
        /// This function is only safe to call during analysis. After analysis
        /// has completed, use a <see cref="ModuleAnalysis"/> instance.
        /// </summary>
        /// <param name="node">The node to associate with the lookup.</param>
        /// <param name="name">The full name of the value to find.</param>
        /// <returns>
        /// All values matching the provided name, or null if the name could not
        /// be resolved.
        /// 
        /// An empty sequence is returned if the name is found but currently has
        /// no values.
        /// </returns>
        /// <remarks>
        /// Calling this function will associate this unit with the requested
        /// variable. Future updates to the variable may result in the unit
        /// being reanalyzed.
        /// </remarks>
        public IAnalysisSet FindAnalysisValueByName(Node node, string name) {
            foreach (var env in Environment.EnumerateTowardsGlobal) {
                var refs = env.GetVariable(node, this, name, true);
                if (refs != null) {
                    var linkedVars = env.GetLinkedVariablesNoCreate(name);
                    if (linkedVars != null) {
                        foreach (var linkedVar in linkedVars) {
                            linkedVar.AddReference(node, this);
                        }
                    }
                    return refs.GetTypes(this);
                }
            }

            return AnalysisSet.Empty;
        }

        internal ProjectEntry ProjectEntry {
            get { return DeclaringModuleEnvironment.ProjectEntry; }
        }

        public JsAnalyzer Analyzer {
            get { return DeclaringModuleEnvironment.ProjectEntry.Analyzer; }
        }

        public void Enqueue() {
            if (!ForEval && !IsInQueue) {
                Analyzer.Queue.Append(this);
                Analyzer.Log.Enqueue(Analyzer.Queue, this);
                this.IsInQueue = true;
            }
        }

        internal void Analyze(DDG ddg, CancellationToken cancel) {
#if DEBUG
            long startTime = _sw.ElapsedMilliseconds;
            try {
                _analysisCount += 1;
#endif
                if (cancel.IsCancellationRequested) {
                    return;
                }
                AnalyzeWorker(ddg, cancel);
#if DEBUG
            } finally {
                long endTime = _sw.ElapsedMilliseconds;
                var thisTime = endTime - startTime;
                _analysisTime += thisTime;
                if (thisTime >= 500 || (_analysisTime / _analysisCount) > 500) {
                    Trace.TraceWarning("Analyzed: {0} {1} ({2} count, {3}ms total, {4}ms mean)", this, thisTime, _analysisCount, _analysisTime, (double)_analysisTime / _analysisCount);
                }
            }
#endif
        }

        internal virtual void AnalyzeWorker(DDG ddg, CancellationToken cancel) {
            Debug.Assert(Ast != null, "Ast has unexpected null value");
            Debug.Assert(ProjectEntry != null, "ProjectEntry has unexpected null value");

            if (Ast == null || ProjectEntry == null || Tree != ProjectEntry.Tree) {
                // analysis unit properties are invalid or we were enqueued and a new version became available
                // don't re-analyze against the old version.
                return;
            }

            DeclaringModuleEnvironment.ClearLinkedVariables();

            ddg.SetCurrentUnit(this);
            Ast.Walk(ddg);

            var toRemove = new List<KeyValuePair<string,VariableDef>>();

            foreach (var variableInfo in DeclaringModuleEnvironment.Variables) {
                variableInfo.Value.ClearOldValues(ProjectEntry);
                if (variableInfo.Value._dependencies.Count == 0 &&
                    variableInfo.Value.TypesNoCopy.Count == 0) {
                    toRemove.Add(variableInfo);
                }
            }

            foreach (var nameValue in toRemove) {
                DeclaringModuleEnvironment.GlobalEnvironment.RemoveVariable(nameValue.Key);

                // if anyone read this value it could now be gone (e.g. user 
                // deletes a class definition) so anyone dependent upon it
                // needs to be updated.
                nameValue.Value.EnqueueDependents();
            }
        }

        /// <summary>
        /// The chain of scopes in which this analysis is defined.
        /// </summary>
        internal EnvironmentRecord Environment {
            get { return _env; }
        }

        public override string ToString() {
            return String.Format(
                "<{3}: Name={0} {4} ({1}), NodeType={2}>",
                FullName,
                GetHashCode(),
                Ast != null ? Ast.GetType().Name : "<unknown>",
                GetType().Name,
                Ast != null && ProjectEntry != null && ProjectEntry.Tree != null ?
                    Ast.GetStart(ProjectEntry.Tree.LocationResolver) : SourceLocation.Invalid
            );
        }

        /// <summary>
        /// Returns the fully qualified name of the analysis unit's scope
        /// including all outer scopes.
        /// </summary>
        internal string FullName {
            get {
                if (Environment != null) {
                    return string.Join(".", Environment.EnumerateFromGlobal.Select(s => s.Name));
                } else {
                    return "<Unnamed unit>";
                }
            }
        }

        #region SelfSet

        bool ISet<AnalysisUnit>.Add(AnalysisUnit item) {
            throw new NotImplementedException();
        }

        void ISet<AnalysisUnit>.ExceptWith(IEnumerable<AnalysisUnit> other) {
            throw new NotImplementedException();
        }

        void ISet<AnalysisUnit>.IntersectWith(IEnumerable<AnalysisUnit> other) {
            throw new NotImplementedException();
        }

        bool ISet<AnalysisUnit>.IsProperSubsetOf(IEnumerable<AnalysisUnit> other) {
            throw new NotImplementedException();
        }

        bool ISet<AnalysisUnit>.IsProperSupersetOf(IEnumerable<AnalysisUnit> other) {
            throw new NotImplementedException();
        }

        bool ISet<AnalysisUnit>.IsSubsetOf(IEnumerable<AnalysisUnit> other) {
            throw new NotImplementedException();
        }

        bool ISet<AnalysisUnit>.IsSupersetOf(IEnumerable<AnalysisUnit> other) {
            throw new NotImplementedException();
        }

        bool ISet<AnalysisUnit>.Overlaps(IEnumerable<AnalysisUnit> other) {
            throw new NotImplementedException();
        }

        bool ISet<AnalysisUnit>.SetEquals(IEnumerable<AnalysisUnit> other) {
            var enumerator = other.GetEnumerator();
            if (enumerator.MoveNext()) {
                if (((ISet<AnalysisUnit>)this).Contains(enumerator.Current)) {
                    return !enumerator.MoveNext();
                }
            }
            return false;
        }

        void ISet<AnalysisUnit>.SymmetricExceptWith(IEnumerable<AnalysisUnit> other) {
            throw new NotImplementedException();
        }

        void ISet<AnalysisUnit>.UnionWith(IEnumerable<AnalysisUnit> other) {
            throw new NotImplementedException();
        }

        void ICollection<AnalysisUnit>.Add(AnalysisUnit item) {
            throw new InvalidOperationException();
        }

        void ICollection<AnalysisUnit>.Clear() {
            throw new InvalidOperationException();
        }

        bool ICollection<AnalysisUnit>.Contains(AnalysisUnit item) {
            return item == this;
        }

        void ICollection<AnalysisUnit>.CopyTo(AnalysisUnit[] array, int arrayIndex) {
            throw new InvalidOperationException();
        }

        int ICollection<AnalysisUnit>.Count {
            get { return 1; }
        }

        bool ICollection<AnalysisUnit>.IsReadOnly {
            get { return true; }
        }

        bool ICollection<AnalysisUnit>.Remove(AnalysisUnit item) {
            throw new InvalidOperationException();
        }

        IEnumerator<AnalysisUnit> IEnumerable<AnalysisUnit>.GetEnumerator() {
            return new SetOfOneEnumerator<AnalysisUnit>(this);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            yield return this;
        }

        #endregion
    }
}
