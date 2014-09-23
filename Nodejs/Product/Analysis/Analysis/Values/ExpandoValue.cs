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
using Microsoft.NodejsTools.Analysis.Analyzer;
using Microsoft.NodejsTools.Parsing;

namespace Microsoft.NodejsTools.Analysis.Values {
    /// <summary>
    /// Base class for functions and objects.  Supports getting/setting
    /// arbitrary values on the object and indexing into the object using
    /// strings.
    /// </summary>
    [Serializable]
    class ExpandoValue : AnalysisValue, IReferenceableContainer {
        private readonly ProjectEntry _projectEntry;
        private readonly int _declVersion;
        internal readonly DependentKeyValue _keysAndValues;
        private AnalysisDictionary<string, PropertyDescriptorValue> _descriptors;
        private Dictionary<object, object> _metadata;
        private TypedDef _linkedValues;

        internal ExpandoValue(ProjectEntry projectEntry) : base(projectEntry) {
            _projectEntry = projectEntry;
            _declVersion = projectEntry.AnalysisVersion;
            _keysAndValues = new DependentKeyValue();
        }

        public ProjectEntry ProjectEntry {
            get {
                return _projectEntry;
            }
        }

        internal override ProjectEntry DeclaringModule {
            get {
                return _projectEntry;
            }
        }

        public bool TryGetMetadata<T>(object key, out T metadata) {
            object res;
            if (_metadata != null && _metadata.TryGetValue(key, out res)) {
                metadata = (T)res;
                return true;
            }
            metadata = default(T);
            return false;
        }

        public void SetMetadata<T>(object key, T value) {
            EnsureMetadata()[key] = value;
        }

        private Dictionary<object, object> EnsureMetadata() {
            if (_metadata == null) {
                _metadata = new Dictionary<object, object>();
            }
            return _metadata;
        }

        public override int DeclaringVersion {
            get {
                return _declVersion;
            }
        }

        public override IAnalysisSet Get(Node node, AnalysisUnit unit, string name, bool addRef = true) {
            var desc = GetProperty(node, unit, name);
            IAnalysisSet res;
            if (desc != null) {
                res = desc.GetValue(node, unit, ProjectEntry, SelfSet, addRef);
            } else {
                res = AnalysisSet.Empty;
            }
            
            return res;
        }

        internal override IPropertyDescriptor GetProperty(Node node, AnalysisUnit unit, string name) {
            EnsureDescriptors();
            
            PropertyDescriptorValue desc;
            if (!_descriptors.TryGetValue(name, out desc)) {
                _descriptors[name] = desc = new PropertyDescriptorValue(ProjectEntry);
            }

            if (IsMutable(name)) {
                if (desc.Values == null) {
                    desc.Values = new EphemeralVariableDef();
                }
                desc.Values.AddDependency(unit);
            }

            if (_linkedValues != null) {
                foreach (var link in _linkedValues.GetTypes(unit, DeclaringModule)) {
                    ExpandoValue expando = link.Value as ExpandoValue;
                    if (expando._descriptors != null && expando._descriptors.ContainsKey(name)) {
                        return new MergedPropertyDescriptor(
                            this,
                            desc,
                            name
                        );
                    }
                }
            }

            return desc;
        }

        [Serializable]
        internal class MergedPropertyDescriptor : IPropertyDescriptor {
            private readonly ExpandoValue _instance;
            private readonly string _name;
            private readonly PropertyDescriptorValue _propDesc;

            public MergedPropertyDescriptor(ExpandoValue instance, PropertyDescriptorValue propDesc, string name) {
                _instance = instance;
                _propDesc = propDesc;
                _name = name;
            }

            public IAnalysisSet GetValue(Node node, AnalysisUnit unit, ProjectEntry declaringScope, IAnalysisSet @this, bool addRef) {
                IAnalysisSet res = _propDesc.GetValue(node, unit, declaringScope, @this, addRef);
                foreach (var prototype in _instance._linkedValues.GetTypes(unit, declaringScope)) {
                    if (prototype.Value.Push()) {
                        try {
                            var value = prototype.Value.GetProperty(node, unit, _name);
                            if (value != null) {
                                res = res.Union(value.GetValue(node, unit, declaringScope, @this, addRef));
                            }
                        } finally {
                            prototype.Value.Pop();
                        }
                    }
                }
                return res;
            }

            public bool IsEphemeral {
                get {
                    return true;
                }
            }
        }

        public virtual bool IsMutable(string name) {
            return true;
        }

        public override void SetMember(Node node, AnalysisUnit unit, string name, IAnalysisSet value) {
            SetMemberWorker(node, unit, name, value);
        }

        protected bool SetMemberWorker(Node node, AnalysisUnit unit, string name, IAnalysisSet value) {
            PropertyDescriptorValue desc;
            if (_descriptors != null && _descriptors.TryGetValue(name, out desc)) {
                if (desc.Setter != null) {
                    desc.Setter.GetTypesNoCopy(unit, ProjectEntry).Call(
                        node,
                        unit,
                        AnalysisSet.Empty,
                        new[] { value }
                    );
                }
            }

            VariableDef varRef = GetValuesDef(name);
            varRef.AddAssignment(node, unit);

            if (IsMutable(name)) {
                varRef.MakeUnionStrongerIfMoreThan(ProjectState.Limits.InstanceMembers, value);
                return varRef.AddTypes(unit, value, declaringScope: DeclaringModule);
            }
            return false;
        }

        public override void DeleteMember(Node node, AnalysisUnit unit, string name) {
            VariableDef def = GetValuesDef(name);

            def.AddReference(node, unit);
        }

        public override IAnalysisSet GetEnumerationValues(Node node, AnalysisUnit unit) {
            var res = AnalysisSet.Empty;
            if (_descriptors != null) {
                foreach (var kvp in _descriptors) {
                    var key = kvp.Key;
                    if (key == "prototype" || key == "__proto__") {
                        // including prototype can cause things to explode, and it's not
                        // enumerable anyway...  This should be replaced w/ more general
                        // support for non-enumerable properties.
                        continue;
                    }
                    if (kvp.Value.Values != null) {
                        var types = kvp.Value.Values.GetTypesNoCopy(unit, ProjectEntry);
                        kvp.Value.Values.ClearOldValues();
                        if (kvp.Value.Values.VariableStillExists) {
                            res = res.Add(ProjectState.GetConstant(kvp.Key).Proxy);
                        }
                    }

                    if (kvp.Value.Getter != null) {
                        foreach (var value in kvp.Value.Getter.GetTypesNoCopy(unit, ProjectEntry)) {
                            res = res.Add(ProjectState.GetConstant(kvp.Key).Proxy);
                        }
                    }
                }
            }
            return res;
        }

        protected VariableDef GetValuesDef(string name) {
            var desc = GetDescriptor(name);

            VariableDef def = desc.Values;
            if (def == null) {
                desc.Values = def = new VariableDef();
            }
            return def;
        }

        public override void SetIndex(Node node, AnalysisUnit unit, IAnalysisSet index, IAnalysisSet value) {
            foreach (var type in index) {
                string strValue;
                if ((strValue = type.Value.GetStringValue()) != null) {
                    // x = {}; x['abc'] = 42; should be available as x.abc
                    SetMember(node, unit, strValue, value);
                }
            }
            AddTypes(node, unit, index, value);
        }

        public override IAnalysisSet GetIndex(Node node, AnalysisUnit unit, IAnalysisSet index) {
            _keysAndValues.AddDependency(unit);
            var res = _keysAndValues.GetValueType(index, unit, ProjectEntry);
            foreach (var value in index) {
                string strValue;
                if ((strValue = value.Value.GetStringValue()) != null) {
                    res = res.Union(Get(node, unit, strValue));
                }
            }
            return res;
        }

        internal bool AddTypes(Node node, AnalysisUnit unit, IAnalysisSet key, IAnalysisSet value, bool enqueue = true) {
            if (_keysAndValues.AddTypes(unit, key, value, enqueue)) {
#if FALSE
                if (_keysVariable != null) {
                    _keysVariable.MakeUnionStrongerIfMoreThan(ProjectState.Limits.DictKeyTypes, value);
                    if (_keysVariable.AddTypes(unit, key, enqueue)) {
                        if (_keysIter != null)
                        {
                            _keysIter.UnionType = null;
                        }
                        if (_keysList != null)
                        {
                            _keysList.UnionType = null;
                        }
                    }
                }
#endif
#if FALSE
                if (_valuesVariable != null) {
                    _valuesVariable.MakeUnionStrongerIfMoreThan(ProjectState.Limits.DictValueTypes, value);
                    if (_valuesVariable.AddTypes(unit, value, enqueue)) {
                        if (_valuesIter != null)
                        {
                            _valuesIter.UnionType = null;
                        }
                        if (_valuesList != null)
                        {
                            _valuesList.UnionType = null;
                        }
                    }
                }
#endif
#if FALSE
                if (_keyValueTuple != null)
                {
                    _keyValueTuple.IndexTypes[0].MakeUnionStrongerIfMoreThan(ProjectState.Limits.DictKeyTypes, key);
                    _keyValueTuple.IndexTypes[1].MakeUnionStrongerIfMoreThan(ProjectState.Limits.DictValueTypes, value);
                    _keyValueTuple.IndexTypes[0].AddTypes(unit, key, enqueue);
                    _keyValueTuple.IndexTypes[1].AddTypes(unit, value, enqueue);
                }
#endif
                return true;
            }
            return false;
        }

        internal override Dictionary<string, IAnalysisSet> GetAllMembers(ProjectEntry accessor) {
            return GetOwnProperties(accessor);
        }

        internal override Dictionary<string, IAnalysisSet> GetOwnProperties(ProjectEntry accessor) {
            if (_descriptors == null || _descriptors.Count == 0) {
                return new Dictionary<string, IAnalysisSet>();
            }

            var res = new Dictionary<string, IAnalysisSet>();
            foreach (var kvp in _descriptors) {
                var key = kvp.Key;
                if (kvp.Value.Values != null) {
                    //kvp.Value.Values.ClearOldValues();
                    if (kvp.Value.Values.VariableStillExists) {
                        var types = kvp.Value.Values.GetTypesNoCopy(accessor, ProjectEntry);
                        if (types.Count != 0 ||
                            kvp.Value.Values.TypesNoCopy.Count == 0) {
                            MergeTypes(res, key, types);
                        }
                    }
                }

                if (kvp.Value.Getter != null) {
                    foreach (var value in kvp.Value.Getter.GetTypesNoCopy(accessor, ProjectEntry)) {
                        FunctionValue userFunc = value.Value as FunctionValue;
                        if (userFunc != null) {
                            MergeTypes(res, key, userFunc.ReturnTypes);
                        }
                    }
                }

                if (kvp.Value.Setter != null) {
                    MergeTypes(res, key, AnalysisSet.Empty);
                }
            }

            if (_linkedValues != null) {
                foreach (var linkedValue in _linkedValues.GetTypesNoCopy(accessor, ProjectEntry)) {
                    if (linkedValue.Value.Push()) {
                        try {
                            MergeDictionaries(res, linkedValue.Value.GetAllMembers(accessor));
                        } finally {
                            linkedValue.Value.Pop();
                        }
                    }
                }
            }

            return res;
        }

        internal static void MergeDictionaries(Dictionary<string, IAnalysisSet> target, Dictionary<string, IAnalysisSet> source) {
            foreach (var keyValue in source) {
                MergeTypes(target, keyValue.Key, keyValue.Value);
            }
        }

        public JsAnalyzer ProjectState { get { return ProjectEntry.Analyzer; } }

        /// <summary>
        /// Adds a member from the built-in module.
        /// </summary>
        public virtual VariableDef Add(string name, IAnalysisSet value) {
            var desc = GetDescriptor(name);

            VariableDef def = desc.Values;
            if (def == null) {
                desc.Values = def = new VariableDef();
            }

            def.AddTypes(ProjectEntry, value, declaringScope: DeclaringModule);
            return def;
        }
        
        public void Add(AnalysisValue value) {
            Add(value.Name, value.SelfSet);
        }

        public void Add(MemberAddInfo member) {
            if (!member.IsProperty) {
                Add(member.Name, member.Value.Proxy);
            } else {
                AddProperty(member);
            }
        }

        public virtual void AddProperty(MemberAddInfo member) {
            var desc = GetDescriptor(member.Name);

            VariableDef def = desc.Getter;
            if (def == null) {
                desc.Getter = def = new VariableDef();
            }

            def.AddTypes(ProjectState._builtinEntry, new ReturningFunctionValue(ProjectEntry, member.Name, member.Value.Proxy).Proxy);
        }

        public void AddProperty(Node node, AnalysisUnit unit, string name, AnalysisValue value) {
            var desc = GetDescriptor(name);

            var descValue = value.Get(node, unit, "value", false);
            if (descValue.Count > 0) {
                if (desc.Values == null) {
                    desc.Values = new VariableDef();
                }
                desc.Values.AddTypes(unit, descValue, declaringScope: DeclaringModule);
            }

            var get = value.Get(node, unit, "get", false);
            if (get.Count > 0) {
                if (desc.Getter == null) {
                    desc.Getter = new VariableDef();
                }
                desc.Getter.AddTypes(unit, get, declaringScope: DeclaringModule);
            }

            var set = value.Get(node, unit, "set", false);
            if (set.Count > 0) {
                if (desc.Setter == null) {
                    desc.Setter = new VariableDef();
                }
                desc.Setter.AddTypes(unit, set, declaringScope: DeclaringModule);
            }
        }

        private PropertyDescriptorValue GetDescriptor(string name) {
            EnsureDescriptors();

            PropertyDescriptorValue desc;
            if (!_descriptors.TryGetValue(name, out desc)) {
                _descriptors[name] = desc = new PropertyDescriptorValue(ProjectEntry);
            }
            return desc;
        }

        private void EnsureDescriptors() {
            if (_descriptors == null) {
                _descriptors = new AnalysisDictionary<string, PropertyDescriptorValue>();
            }
        }

        protected static void MergeTypes(Dictionary<string, IAnalysisSet> res, string key, IEnumerable<AnalysisProxy> types) {
            IAnalysisSet set;
            if (!res.TryGetValue(key, out set)) {
                res[key] = set = AnalysisSet.Create(types);
            } else {
                res[key] = set.Union(types);
            }
        }

        public AnalysisDictionary<string, PropertyDescriptorValue> Descriptors {
            get {
                return _descriptors;
            }
        }

        /// <summary>
        /// Makes this analysis value include all of the source analysis
        /// values and automatically pick up new values if the source
        /// changes.
        /// </summary>
        /// <param name="source"></param>
        public void AddLinkedValue(AnalysisUnit unit, ExpandoValue source) {
            if (_linkedValues == null) {
                _linkedValues = new TypedDef();
            }
            _linkedValues.AddTypes(unit, source.SelfSet);
        }
        
        #region IReferenceableContainer Members

        public virtual IEnumerable<IReferenceable> GetDefinitions(string name) {
            PropertyDescriptorValue desc;
            if (_descriptors != null && _descriptors.TryGetValue(name, out desc)) {
                if (desc.Values != null) {
                    yield return desc.Values;
                }

                if (desc.Getter != null) {
                    foreach (var type in desc.Getter.Types) {
                        var func = type.Value as IReferenceable;
                        if (func != null) {
                            yield return func;
                        }
                    }
                } else if (desc.Setter != null) {
                    foreach (var type in desc.Setter.Types) {
                        var func = type.Value as IReferenceable;
                        if (func != null) {
                            yield return func;
                        }
                    }
                }
            }

            if (_linkedValues != null) {
                foreach (var value in _linkedValues.GetTypesNoCopy()) {
                    IReferenceableContainer refContainer = value.Value as IReferenceableContainer;
                    if (refContainer != null) {
                        foreach (var result in refContainer.GetDefinitions(name)) {
                            yield return result;
                        }
                    }
                }
            }
        }

        #endregion

        internal void DefineSetter(AnalysisUnit unit, string nameStr, IAnalysisSet analysisSet) {
            EnsureDescriptors();

            PropertyDescriptorValue propDesc;
            if (!_descriptors.TryGetValue(nameStr, out propDesc)) {
                _descriptors[nameStr] = propDesc = new PropertyDescriptorValue(ProjectEntry);
            }

            if (propDesc.Setter == null) {
                propDesc.Setter = new VariableDef();
            }
            propDesc.Setter.AddTypes(unit, analysisSet, declaringScope: DeclaringModule);
        }

        internal void DefineGetter(AnalysisUnit unit, string nameStr, IAnalysisSet analysisSet) {
            EnsureDescriptors();
            PropertyDescriptorValue propDesc;
            if (!_descriptors.TryGetValue(nameStr, out propDesc)) {
                _descriptors[nameStr] = propDesc = new PropertyDescriptorValue(ProjectEntry);
            }

            if (propDesc.Getter == null) {
                propDesc.Getter = new VariableDef();
            }
            propDesc.Getter.AddTypes(unit, analysisSet, declaringScope: DeclaringModule);
        }
    }

    interface IPropertyDescriptor {
        IAnalysisSet GetValue(Node node, AnalysisUnit unit, ProjectEntry declaringScope, IAnalysisSet @this, bool addRef);
        bool IsEphemeral {
            get;
        }
    }
}
