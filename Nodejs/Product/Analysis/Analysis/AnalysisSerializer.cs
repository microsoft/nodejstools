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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using Microsoft.NodejsTools.Analysis.AnalysisSetDetails;
using Microsoft.NodejsTools.Analysis.Analyzer;
using Microsoft.NodejsTools.Analysis.Values;
using Microsoft.NodejsTools.Parsing;
using LinqExpr = System.Linq.Expressions.Expression;

namespace Microsoft.NodejsTools.Analysis {
    internal class AnalysisSerializer {
        private readonly List<object> _memoDict = new List<object>();
        private readonly Dictionary<object, int> _reverseMemo = new Dictionary<object, int>(new ReferenceComparer<object>());
        private readonly Dictionary<string, int> _stringMemo = new Dictionary<string, int>();
        private readonly List<Action> _postProcess = new List<Action>();
        
        private static readonly Dictionary<Type, FieldInfo[]> _serializationMembers = new Dictionary<Type, FieldInfo[]>();
        private static readonly Type EqualityComparerGenericTypeDef = EqualityComparer<object>.Default.GetType().GetGenericTypeDefinition();
        private static readonly Type GenericEqualityComparerGenericTypeDef = EqualityComparer<TimeSpan>.Default.GetType().GetGenericTypeDefinition();
        private static readonly Dictionary<FieldInfo, Func<object, object>> _getField = new Dictionary<FieldInfo, Func<object, object>>();
        private static readonly Dictionary<FieldInfo, Action<object, object>> _setField = new Dictionary<FieldInfo, Action<object, object>>();
        private static readonly Dictionary<FieldInfo, Action<object, IndexSpan>> _setIndexSpan = new Dictionary<FieldInfo, Action<object, IndexSpan>>();
        private static readonly object _true = true, _false = false;
        private static Func<AnalysisSerializer, Stack<DeserializationFrame>, IClrTypeDeserializer, BinaryReader, object> ClrTypeDeserializer;
        private static Func<AnalysisSerializer, Stack<DeserializationFrame>, IClrClassTypeDeserializer, BinaryReader, object> ClrClassTypeDeserializer;
        private static readonly Dictionary<Type, SerializerFunction> _serializer = MakeSerializationTable();

        private static object[] _cachedInts = new object[200];
        private const int _smallestNegative = 2;
        private const int EnumInt32 = 0;


        public AnalysisSerializer() {
        }

        public object Deserialize(Stream serializationStream) {
            BinaryReader reader = new BinaryReader(serializationStream);

            var res = DeserializeObject(reader);

            foreach (var action in _postProcess) {
                action();
            }
            _postProcess.Clear();

            return res;
        }

        /// <summary>
        /// Serialized the specified value to the provided stream.
        /// 
        /// Multiple calls of Serialize can be called against the same serializer and 
        /// references across the different object graphs will be shared.
        /// </summary>
        public void Serialize(Stream serializationStream, object graph) {
            BinaryWriter writer = new BinaryWriter(serializationStream);

            Serialize(graph, writer);
        }


        delegate void DeserializationFunc(object value);

        internal static object BoxInt(int intValue) {
            if (intValue > -_smallestNegative &&
                intValue < _cachedInts.Length - (_smallestNegative + 1)) {
                int index = (int)intValue + _smallestNegative;
                object value;
                if ((value = _cachedInts[index]) == null) {
                    value = _cachedInts[index] = (object)intValue;
                }
                return value;
            }
            return intValue;
        }

        /// <summary>
        /// Stores our state for each object being deserialized.  This is a 
        /// struct so that we can avoid allocating individual DeserialzationFrame
        /// objects.
        /// 
        /// For each object that we need to store we're either storing it into a 
        /// field or storing it somewhere else (an array, list, dict, etc...).  For
        /// the field case because it's so common we have fields here for the field
        /// and instance.  For the other cases we use a delegate which is the 
        /// DeserializationFunc.
        /// 
        /// When initializing a value type we have to store it into it's parent after
        /// all of the fields have been initialized.  For that we use both the field/instance
        /// as well as the deserialization func which does  the chained assignment.
        /// </summary>
        struct DeserializationFrame {
            public readonly FieldInfo Field;
            public readonly object Instance;
            public readonly DeserializationFunc Func;

            public DeserializationFrame(DeserializationFunc func) {
                Func = func;
                Field = null;
                Instance = null;
            }

            public DeserializationFrame(FieldInfo field, object instance, DeserializationFunc func = null) {
                Field = field;
                Instance = instance;
                Func = func;
            }

            public void Assign(object value) {
                if (Field != null) {
                    SetField(Field, Instance, value);
                    if (Func != null) {
                        Func(Instance);
                    }
                } else if (Func != null) {
                    Func(value);
                }
            }

            public void Assign(IndexSpan span) {
                if (Field != null) {
                    SetField(Field, Instance, span);
                    if (Func != null) {
                        Func(Instance);
                    }
                } else if (Func != null) {
                    Func(span);
                }
            }

            public DeserializationFunc ChainFunc() {
                if (Field == null) {
                    return Func;
                }

                return Assign;
            }
        }

        private object DeserializeObject(BinaryReader reader) {
            Stack<DeserializationFrame> stack = new Stack<DeserializationFrame>();
            object res = null;
            stack.Push(new DeserializationFrame(value => res = value));
            do {
                var nextAssign = stack.Pop();

                var objType = (SerializationType)reader.ReadByte();
                object nextValue;
                switch (objType) {
                    case SerializationType.Null: nextValue = null; break;
                    case SerializationType.ObjectReference:
                        int objId = reader.ReadInt32();
                        if (objId >= _memoDict.Count) {
                            throw new InvalidOperationException("unknown object reference: " + objId);
                        }
                        nextValue = _memoDict[objId];
                        break;
                    case SerializationType.HashSet:
                        nextValue = DeserializeHashSet(stack, reader);
                        break;
                    case SerializationType.List:
                        nextValue = DeserializeList(stack, reader);
                        break;
                    case SerializationType.AnalysisHashSet:
                        nextValue = DeserializeAnalysisHashSet(stack, reader);
                        break;
                    case SerializationType.AnalysisDictionary:
                        nextValue = DeserializeAnalysisDictionary(stack, reader);
                        break;
                    case SerializationType.Dictionary:
                        nextValue = DeserializeDictionary(stack, reader);
                        break;
                    case SerializationType.Array:
                        nextValue = DeserializeArray(stack, reader);
                        break;
                    case SerializationType.IndexSpan:
                        nextAssign.Assign(new IndexSpan(reader.ReadInt32(), reader.ReadInt32()));
                        continue;
                    case SerializationType.EncodedSpan:
                        nextAssign.Assign(new EncodedSpan(reader.ReadInt32()));
                        continue;
                    case SerializationType.True: nextValue = _true; break;
                    case SerializationType.False: nextValue = _false; break;
                    case SerializationType.Double:
                        nextValue = JSParser.BoxDouble(reader.ReadDouble()); break;
                    case SerializationType.String:
                        nextValue = reader.ReadString();
                        Memoize(nextValue);
                        break;
                    case SerializationType.Int:
                        nextValue = BoxInt(reader.ReadInt32()); break;
                    case SerializationType.Long: nextValue = reader.ReadInt64(); break;
                    case SerializationType.Enum:
                        switch (reader.ReadByte()) {
                            case EnumInt32:
                                nextValue = BoxInt(reader.ReadInt32());
                                break;
                            default:
                                throw new InvalidOperationException("unsupported enum type");
                        }
                        break;
                    case SerializationType.CallDelegate: nextValue = DeserializeCallDelegate(reader); break;
                    case SerializationType.EmptyAnalysisSet:
                        nextValue = AnalysisSetEmptyObject.Instance;
                        break;
                    case SerializationType.ClrObject:
                        int typeIndex = reader.ReadByte();
                        if (typeIndex < 0 || typeIndex >= AnalysisSerializationSupportedTypeAttribute.AllowedTypes.Length) {
                            throw new InvalidOperationException();
                        }

                        Type clrType = AnalysisSerializationSupportedTypeAttribute.AllowedTypes[typeIndex];
                        object value = FormatterServices.GetUninitializedObject(clrType);

                        IDeserializeInitialization init = value as IDeserializeInitialization;
                        if (init != null) {
                            init.Init();
                        }
                        Memoize(value);

                        var members = GetSerializableMembers(value.GetType());
                        for (int i = members.Length - 1; i >= 0; i--) {
                            var member = members[i];
                            if (i == members.Length - 1 && value is ValueType) {
                                // we can't assign the ValueType until all of its fields have been initialized,
                                // so queue up the current next assignment after our initialization
                                var tempNextAssign = nextAssign;
                                var tempValue = value;
                                stack.Push(
                                    new DeserializationFrame(
                                        member,
                                        value,
                                        tempNextAssign.ChainFunc()
                                    )
                                );
                            } else {
                                stack.Push(new DeserializationFrame(member, value));
                            }
                        }

                        if (value is ValueType && members.Length > 0) {
                            // skip the assignment, we'll handle it after field initialization
                            continue;
                        }

                        nextValue = value;

                        break;
                    case SerializationType.ReferenceDict: nextValue = DeserializeReferenceDict(stack, reader); break;
                    case SerializationType.MissingValue: nextValue = Microsoft.NodejsTools.Parsing.Missing.Value; break;
                    default:
                        nextValue = DeserializeComparer(objType, reader);
                        break;
                }

                nextAssign.Assign(nextValue);
            } while (stack.Count > 0);

            return res;
        }

        private object DeserializeComparer(BinaryReader reader) {
            return DeserializeComparer((SerializationType)reader.ReadByte(), reader);
        }

        private object DeserializeComparer(SerializationType type, BinaryReader reader) {
            switch(type) {
                case SerializationType.ObjectEqualityComparer:
                    return DeserializeClrType(this, null, EqualityComparerDeserializer.Instance, reader);
                case SerializationType.UnionComparer:
                    return DeserializeUnionComparer(reader);
                case SerializationType.ObjectComparer:
                    return ObjectComparer.Instance;
                case SerializationType.OrdinalComparer:
                    return StringComparer.Ordinal;
                default: 
                    throw new InvalidOperationException("unsupported SerializationType");
            }
        }

        private static object GetField(FieldInfo fi, object instance) {
            Func<object, object> getter;
            if (!_getField.TryGetValue(fi, out getter)) {
                var param = LinqExpr.Parameter(typeof(object));
                _getField[fi] = getter = LinqExpr.Lambda<Func<object, object>>(
                    LinqExpr.Convert(
                        LinqExpr.Field(
                            ConvertOrUnbox(param, fi.DeclaringType),
                            fi
                        ),
                        typeof(object)
                    ),
                    param
                ).Compile();
            }
            return getter(instance);
        }

        private static LinqExpr ConvertOrUnbox(LinqExpr expr, Type type) {
            if (type.IsValueType) {
                return LinqExpr.Unbox(expr, type);
            }
            return LinqExpr.Convert(expr, type);
        }

        private static void SetField(FieldInfo fi, object instance, IndexSpan value) {
            if ((fi.Attributes & FieldAttributes.InitOnly) != 0) {
                fi.SetValue(instance, value);
                return;
            }

            Action<object, IndexSpan> setter;
            if (!_setIndexSpan.TryGetValue(fi, out setter)) {
                var instParam = LinqExpr.Parameter(typeof(object));
                var valueParam = LinqExpr.Parameter(typeof(IndexSpan));
                _setIndexSpan[fi] = setter = LinqExpr.Lambda<Action<object, IndexSpan>>(
                    LinqExpr.Assign(
                        LinqExpr.Field(ConvertOrUnbox(instParam, fi.DeclaringType), fi),
                        LinqExpr.Convert(valueParam, fi.FieldType)
                    ),
                    instParam,
                    valueParam
                ).Compile();
            }
            setter(instance, value);
        }

        private static void SetField(FieldInfo fi, object instance, object value) {
            if (value == null) {
                // everything is zero inited...
                return;
            }

            if ((fi.Attributes & FieldAttributes.InitOnly) != 0) {
                // can't use linq to init read only fields... 
                fi.SetValue(instance, value);
                return;
            }

            Action<object, object> setter;
            if (!_setField.TryGetValue(fi, out setter)) {
                var instParam = LinqExpr.Parameter(typeof(object));
                var valueParam = LinqExpr.Parameter(typeof(object));
                _setField[fi] = setter = LinqExpr.Lambda<Action<object, object>>(
                    LinqExpr.Assign(
                        LinqExpr.Field(ConvertOrUnbox(instParam, fi.DeclaringType), fi),
                        LinqExpr.Convert(valueParam, fi.FieldType)
                    ),
                    instParam,
                    valueParam
                ).Compile();
            }
            setter(instance, value);
        }

        private void Serialize(object graph, BinaryWriter writer) {
            Stack<object> stack = new Stack<object>();
            stack.Push(graph);
            do {
                var value = stack.Pop();

                if (value == null) {
                    writer.Write((byte)SerializationType.Null);
                    continue;
                }

                int memoId;
                if (_reverseMemo.TryGetValue(value, out memoId)) {
                    writer.Write((byte)SerializationType.ObjectReference);
                    writer.Write(memoId);
                    continue;
                }

                SerializerFunction serializerFunc;
                if (value.GetType().IsArray) {
                    ReverseMemoize(value);
                    var arr = (Array)value;

                    writer.Write((byte)SerializationType.Array);
                    WriteClrType(value.GetType().GetElementType(), writer);
                    writer.Write(arr.Length);
                    for (int i = arr.Length - 1; i >= 0; i--) {
                        stack.Push(arr.GetValue(i));
                    }
                } else if (value.GetType().IsEnum) {
                    SerializeEnum(value, writer);
                } else if (value is IAnalysisSerializeAsNull) {
                    writer.Write((byte)SerializationType.Null);
                } else if (_serializer.TryGetValue(value.GetType(), out serializerFunc)) {
                    serializerFunc(value, this, stack, writer);
                } else if (value.GetType().IsGenericType) {
                    // add a specialized serializer for this specific instantiation and invoke it...

                    var gtd = value.GetType().GetGenericTypeDefinition();
                    string method;
                    if (gtd == typeof(List<>)) {
                        method = "SerializeList";
                    } else if (gtd == typeof(AnalysisDictionary<,>)) {
                        method = "SerializeAnalysisDictionary";
                    } else if (gtd == typeof(Dictionary<,>)) {
                        method = "SerializeDictionary";
                    } else if (gtd == typeof(HashSet<>)) {
                        method = "SerializeHashSet";
                    } else if (gtd == EqualityComparerGenericTypeDef || gtd == GenericEqualityComparerGenericTypeDef) {
                        method = "SerializeEqualityComparer";
                    } else {
                        throw new InvalidOperationException("unsupported generic type: " + value.GetType());
                    }

                    SerializerFunction func;
                    _serializer[value.GetType()] = func = (SerializerFunction)Delegate.CreateDelegate(
                        typeof(SerializerFunction),
                        typeof(AnalysisSerializer)
                            .GetMethod(method, BindingFlags.NonPublic | BindingFlags.Static)
                            .MakeGenericMethod(value.GetType().GetGenericArguments())
                    );
                    func(value, this, stack, writer);
                } else {
                    throw new InvalidOperationException("unsupported type: " + value.GetType());
                }
            } while (stack.Count > 0);
        }

        private static void SerializeEqualityComparer<T>(object value, AnalysisSerializer serializer, Stack<object> stack, BinaryWriter writer) {
            writer.Write((byte)SerializationType.ObjectEqualityComparer);
            serializer.WriteClrType(typeof(T), writer);
        }

        private static void SerializeList<T>(object value, AnalysisSerializer serializer, Stack<object> stack, BinaryWriter writer) {
            serializer.ReverseMemoize(value);

            var list = (List<T>)value;
            writer.Write((byte)SerializationType.List);
            serializer.WriteClrType(typeof(T), writer);
            writer.Write(list.Count);
            for (int i = list.Count - 1; i >= 0; i--) {
                stack.Push(list[i]);
            }
        }

        private static void SerializeHashSet<T>(object value, AnalysisSerializer serializer, Stack<object> stack, BinaryWriter writer) {
            serializer.ReverseMemoize(value);

            var hs = (HashSet<T>)value;
            writer.Write((byte)SerializationType.HashSet);
            serializer.WriteClrType(typeof(T), writer);

            writer.Write(hs.Count);
            foreach (var obj in hs) {
                stack.Push(obj);
            }
            stack.Push(hs.Comparer);
        }

        private static void SerializeAnalysisHashSet(object value, AnalysisSerializer serializer, Stack<object> stack, BinaryWriter writer) {
            serializer.ReverseMemoize(value);

            var hs = (AnalysisHashSet)value;
            writer.Write((byte)SerializationType.AnalysisHashSet);

            var items = new List<AnalysisProxy>(hs);
            writer.Write(items.Count);
            foreach (var obj in items) {
                stack.Push(obj);
            }
            stack.Push(hs.Comparer);
        }

        private static void SerializeDictionary<TKey, TValue>(object value, AnalysisSerializer serializer, Stack<object> stack, BinaryWriter writer) {
            serializer.ReverseMemoize(value);

            var dict = (Dictionary<TKey, TValue>)value;
            writer.Write((byte)SerializationType.Dictionary);
            serializer.WriteClrType(typeof(TKey), writer);
            serializer.WriteClrType(typeof(TValue), writer);
            writer.Write(dict.Count);
            foreach (var key in dict.Keys) {
                stack.Push(dict[key]);
                stack.Push(key);
            }
            stack.Push(dict.Comparer);
        }

        private static void SerializeAnalysisDictionary<TKey, TValue>(object value, AnalysisSerializer serializer, Stack<object> stack, BinaryWriter writer)
            where TKey : class
            where TValue : class {
            serializer.ReverseMemoize(value);

            var dict = (AnalysisDictionary<TKey, TValue>)value;
            writer.Write((byte)SerializationType.AnalysisDictionary);
            serializer.WriteClrType(typeof(TKey), writer);
            serializer.WriteClrType(typeof(TValue), writer);
            writer.Write(dict.Count);
            foreach (var key in dict) {
                stack.Push(key.Value);
                stack.Push(key.Key);
            }
            stack.Push(dict.Comparer);
        }

        private static void SerializeEnum(object graph, BinaryWriter writer) {
            writer.Write((byte)SerializationType.Enum);
            switch (Type.GetTypeCode(graph.GetType().UnderlyingSystemType)) {
                case TypeCode.Int32:
                    writer.Write((byte)EnumInt32);
                    writer.Write((int)graph);
                    break;
                default:
                    throw new InvalidOperationException("Unsupported enum type: " + graph.GetType().UnderlyingSystemType.Name);
            }
        }

        private object Memoize(object value) {
            var res = value;
            _memoDict.Add(res);
            return res;
        }

        private void ReverseMemoize(object graph) {
            _reverseMemo[graph] = _reverseMemo.Count;
        }

        private void WriteClrType(Type type, BinaryWriter writer) {
            int typeIndex;
            if (!AnalysisSerializationSupportedTypeAttribute.AllowedTypeIndexes.TryGetValue(type, out typeIndex)) {
                throw new InvalidOperationException("unsupported clr type: " + type);
            }

            writer.Write((byte)typeIndex);
        }

        enum CallDelegateDeclType {
            None,
            GlobalBuilder,
            OverviewWalker,
            NodejsModuleBuilder
        }

        private static void SerializeCallDelegate(object value, AnalysisSerializer serializer, Stack<object> stack, BinaryWriter writer) {
            writer.Write((byte)SerializationType.CallDelegate);
            CallDelegate cd = (CallDelegate)value;
            if (cd.Method.DeclaringType == typeof(GlobalBuilder)) {
                writer.Write((byte)CallDelegateDeclType.GlobalBuilder);
            } else if (cd.Method.DeclaringType == typeof(OverviewWalker)) {
                writer.Write((byte)CallDelegateDeclType.OverviewWalker);
            } else if (cd.Method.DeclaringType == typeof(NodejsModuleBuilder)) {
                writer.Write((byte)CallDelegateDeclType.NodejsModuleBuilder);
            } else {
                throw new InvalidOperationException("unsupported CallDelegate type: " + cd.Method.DeclaringType);
            }
            Debug.Assert(cd.Method.IsStatic);
            writer.Write(cd.Method.Name);
        }

        private object DeserializeUnionComparer(BinaryReader reader) {
            int strength = reader.ReadByte();
            return UnionComparer.Instances[strength];
        }

        private static void SerializeUnionComparer(object value, AnalysisSerializer serializer, Stack<object> stack, BinaryWriter writer) {
            writer.Write((byte)SerializationType.UnionComparer);
            writer.Write((byte)((UnionComparer)value).Strength);
        }

        private static void SerializeReferenceDict(object value, AnalysisSerializer serializer, Stack<object> stack, BinaryWriter writer) {
            serializer.ReverseMemoize(value);

            writer.Write((byte)SerializationType.ReferenceDict);
            var dictionary = (ReferenceDict)value;
            writer.Write(dictionary.Count);
            foreach (var key in dictionary.Keys) {
                stack.Push(dictionary[key]);
                stack.Push(key);
            }
        }

        private object DeserializeReferenceDict(Stack<DeserializationFrame> stack, BinaryReader reader) {
            return DeserializeDictionary<ProjectEntry, ReferenceList>(stack, reader, new ReferenceDict(), reader.ReadInt32());
        }

        private object DeserializeCallDelegate(BinaryReader reader) {
            Type declType;
            switch ((CallDelegateDeclType)reader.ReadByte()) {
                case CallDelegateDeclType.GlobalBuilder: declType = typeof(GlobalBuilder); break;
                case CallDelegateDeclType.OverviewWalker: declType = typeof(OverviewWalker); break;
                case CallDelegateDeclType.NodejsModuleBuilder: declType = typeof(NodejsModuleBuilder); break;
                default:
                    throw new InvalidOperationException("unsupported CallDelegate type");
            }
            string methodName = reader.ReadString();
            var method = declType.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null) {
                throw new InvalidOperationException("unsupported method: " + methodName);
            }

            return Delegate.CreateDelegate(typeof(CallDelegate), method);
        }

        private static void SerializeString(object value, AnalysisSerializer serializer, Stack<object> stack, BinaryWriter writer) {
            string str = (string)value;
            int memoId;
            if (!serializer._stringMemo.TryGetValue(str, out memoId)) {
                // save the string in the normal memo so deserialization is simplified
                memoId = serializer._stringMemo[str] = serializer._reverseMemo[str] = serializer._reverseMemo.Count;
                writer.Write((byte)SerializationType.String);
                writer.Write((string)value);
            } else {
                writer.Write((byte)SerializationType.ObjectReference);
                writer.Write(memoId);
            }
        }

        private static void SerializeDouble(object value, AnalysisSerializer serializer, Stack<object> stack, BinaryWriter writer) {
            writer.Write((byte)SerializationType.Double);
            writer.Write((double)value);
        }

        private static void SerializeBool(object value, AnalysisSerializer serializer, Stack<object> stack, BinaryWriter writer) {
            if ((bool)value) {
                writer.Write((byte)SerializationType.True);
            } else {
                writer.Write((byte)SerializationType.False);
            }
        }

        private static void SerializeInt(object value, AnalysisSerializer serializer, Stack<object> stack, BinaryWriter writer) {
            writer.Write((byte)SerializationType.Int);
            writer.Write((int)value);
        }

        private static void SerializeLong(object value, AnalysisSerializer serializer, Stack<object> stack, BinaryWriter writer) {
            writer.Write((byte)SerializationType.Long);
            writer.Write((long)value);
        }

        private static void SerializeIndexSpan(object value, AnalysisSerializer serializer, Stack<object> stack, BinaryWriter writer) {
            writer.Write((byte)SerializationType.IndexSpan);
            writer.Write(((IndexSpan)value).Start);
            writer.Write(((IndexSpan)value).Length);
        }

        private static void SerializeEncodedSpan(object value, AnalysisSerializer serializer, Stack<object> stack, BinaryWriter writer) {
            writer.Write((byte)SerializationType.EncodedSpan);
            writer.Write(((EncodedSpan)value).Span);
        }

        private static void SerializeBooleanValue(object value, BinaryWriter writer) {
            BooleanValue boolean = (BooleanValue)value;
            if (boolean._value) {
                writer.Write((byte)SerializationType.TrueValue);
            } else {
                writer.Write((byte)SerializationType.FalseValue);
            }
        }

        private static Dictionary<Type, SerializerFunction> MakeSerializationTable() {
            var res = new Dictionary<Type, SerializerFunction>() {
                { typeof(CallDelegate), SerializeCallDelegate },
                { typeof(UnionComparer), SerializeUnionComparer },
                { typeof(ReferenceDict), SerializeReferenceDict },
                { typeof(string), SerializeString },
                { typeof(bool), SerializeBool },
                { typeof(double), SerializeDouble },
                { typeof(int), SerializeInt },
                { typeof(long), SerializeLong },
                { typeof(IndexSpan), SerializeIndexSpan },
                { typeof(EncodedSpan), SerializeEncodedSpan },
                { typeof(HashSet<string>), SerializeHashSet<string> },
                { typeof(AnalysisHashSet), SerializeAnalysisHashSet },
                { typeof(HashSet<EncodedSpan>), SerializeHashSet<EncodedSpan> },
                { typeof(AnalysisSetEmptyObject), new SimpleTypeSerializer(SerializationType.EmptyAnalysisSet).Serialize },
                { typeof(Microsoft.NodejsTools.Parsing.Missing), new SimpleTypeSerializer(SerializationType.MissingValue).Serialize },
                { typeof(ObjectComparer), new SimpleTypeSerializer(SerializationType.ObjectComparer).Serialize },
                { StringComparer.Ordinal.GetType(), new SimpleTypeSerializer(SerializationType.OrdinalComparer).Serialize }
            };
                
            foreach(var type in AnalysisSerializationSupportedTypeAttribute.AllowedTypeIndexes) {
                // we don't want to replace something that we have a more special form of
                if (!res.ContainsKey(type.Key)) {
                    res[type.Key] = new ClrSerializer(type.Value, GetSerializableMembers(type.Key)).Serialize;
                }
            }
                    
            return res;
        }

        class ClrSerializer {
            private readonly int _typeIndex;
            private readonly FieldInfo[] _fields;

            public ClrSerializer(int typeIndex, FieldInfo[] fields) {
                _typeIndex = typeIndex;
                _fields = fields;
            }

            public void Serialize(object value, AnalysisSerializer serializer, Stack<object> stack, BinaryWriter writer) {
                serializer.ReverseMemoize(value);

                writer.Write((byte)SerializationType.ClrObject);
                writer.Write((byte)_typeIndex);

                var members = _fields;
                for (int i = members.Length - 1; i >= 0; i--) {
                    stack.Push(GetField(members[i], value));
                }
            }
        }

        class SimpleTypeSerializer {
            private readonly SerializationType _type;

            public SimpleTypeSerializer(SerializationType type) {
                _type = type;
            }

            public void Serialize(object value, AnalysisSerializer serializer, Stack<object> stack, BinaryWriter writer) {
                writer.Write((byte)_type);
            }
        }


        class EqualityComparerDeserializer : IClrTypeDeserializer {
            public static readonly EqualityComparerDeserializer Instance = new EqualityComparerDeserializer();

            public object Deserialize<T>(AnalysisSerializer serializer, Stack<DeserializationFrame> stack, BinaryReader reader) {
                return EqualityComparer<T>.Default;
            }
        }


        #region Array support

        private object DeserializeArray(Stack<DeserializationFrame> stack, BinaryReader reader) {
            return DeserializeClrType(this, stack, ArrayDeserializer.Instance, reader);
        }        

        class ArrayDeserializer : IClrTypeDeserializer {
            public static readonly ArrayDeserializer Instance = new ArrayDeserializer();

            public object Deserialize<T>(AnalysisSerializer serializer, Stack<DeserializationFrame> stack, BinaryReader reader) {
                int length = reader.ReadInt32();
                var arr = new T[length];
                serializer.Memoize(arr);
                for (int i = length - 1; i >= 0; i--) {
                    var index = i;
                    stack.Push(
                        new DeserializationFrame(newValue => arr[index] = (T)newValue)
                    );
                }
                return arr;
            }
        }

        #endregion

        #region Dictionary support

        class DictionaryKeyDeserializer : IClrTypeDeserializer {
            public static readonly DictionaryKeyDeserializer Instance = new DictionaryKeyDeserializer();

            public object Deserialize<T>(AnalysisSerializer serializer, Stack<DeserializationFrame> stack, BinaryReader reader) {
                return DeserializeClrType(serializer, stack, DictionaryValueDeserializer<T>.Instance, reader);
            }
        }

        class DictionaryValueDeserializer<TKey> : IClrTypeDeserializer {
            public static readonly DictionaryValueDeserializer<TKey> Instance = new DictionaryValueDeserializer<TKey>();

            public object Deserialize<T>(AnalysisSerializer serializer, Stack<DeserializationFrame> stack, BinaryReader reader) {
                return serializer.DeserializeDictionary<TKey, T>(stack, reader);
            }
        }

        class AnalysisDictionaryKeyDeserializer : IClrClassTypeDeserializer {
            public static readonly AnalysisDictionaryKeyDeserializer Instance = new AnalysisDictionaryKeyDeserializer();

            public object Deserialize<T>(AnalysisSerializer serializer, Stack<DeserializationFrame> stack, BinaryReader reader) where T : class {
                return DeserializeClrType(serializer, stack, AnalysisDictionaryValueDeserializer<T>.Instance, reader);
            }
        }

        class AnalysisDictionaryValueDeserializer<TKey> : IClrClassTypeDeserializer where TKey : class {
            public static readonly AnalysisDictionaryValueDeserializer<TKey> Instance = new AnalysisDictionaryValueDeserializer<TKey>();

            public object Deserialize<T>(AnalysisSerializer serializer, Stack<DeserializationFrame> stack, BinaryReader reader) where T : class {
                return serializer.DeserializeAnalysisDictionary<TKey, T>(stack, reader);
            }
        }

        private object DeserializeDictionary(Stack<DeserializationFrame> stack, BinaryReader reader) {
            return DeserializeClrType(this, stack, DictionaryKeyDeserializer.Instance, reader);
        }

        private IDictionary DeserializeDictionary<TKey, TValue>(Stack<DeserializationFrame> stack, BinaryReader reader) {
            int count = reader.ReadInt32();

            var comparer = (IEqualityComparer<TKey>)DeserializeComparer(reader);
            var value = new Dictionary<TKey, TValue>(count, comparer);

            return DeserializeDictionary<TKey, TValue>(stack, reader, value, count);
        }

        private IDictionary DeserializeDictionary<TKey, TValue>(Stack<DeserializationFrame> stack, BinaryReader reader, Dictionary<TKey, TValue> value, int count) {
            Memoize(value);

            for (int i = 0; i < count; i++) {
                object key = null;
                stack.Push(new DeserializationFrame(newValue => _postProcess.Add(() => value[(TKey)key] = (TValue)newValue)));
                stack.Push(new DeserializationFrame(newKey => key = newKey));
            }
            return value;
        }

        private object DeserializeAnalysisDictionary(Stack<DeserializationFrame> stack, BinaryReader reader) {
            return DeserializeClrType(this, stack, AnalysisDictionaryKeyDeserializer.Instance, reader);
        }

        private object DeserializeAnalysisDictionary<TKey, TValue>(Stack<DeserializationFrame> stack, BinaryReader reader)
            where TKey : class
            where TValue : class {
            int count = reader.ReadInt32();

            var comparer = (IEqualityComparer<TKey>)DeserializeComparer(reader);
            var value = new AnalysisDictionary<TKey, TValue>(count, comparer);

            return DeserializeAnalysisDictionary<TKey, TValue>(stack, reader, value, count);
        }

        private object DeserializeAnalysisDictionary<TKey, TValue>(Stack<DeserializationFrame> stack, BinaryReader reader, AnalysisDictionary<TKey, TValue> value, int count)
            where TKey : class
            where TValue : class {
            Memoize(value);

            for (int i = 0; i < count; i++) {
                object key = null;
                stack.Push(new DeserializationFrame(newValue => _postProcess.Add(() => value[(TKey)key] = (TValue)newValue)));
                stack.Push(new DeserializationFrame(newKey => key = newKey));
            }
            return value;
        }

        private object DeserializeAnalysisHashSet(Stack<DeserializationFrame> stack, BinaryReader reader) {
            int count = reader.ReadInt32();
            var comparer = (IEqualityComparer<AnalysisProxy>)DeserializeComparer(reader);
            var value = new AnalysisHashSet(count, comparer);
            Memoize(value);

            DeserializationFunc adder = newValue => _postProcess.Add(() => value.Add((AnalysisProxy)newValue));
            for (int i = 0; i < count; i++) {
                stack.Push(new DeserializationFrame(adder));
            }
            return value;
        }

        #endregion

        #region List support
        
        class ListDeserializer : IClrTypeDeserializer {
            public static readonly ListDeserializer Instance = new ListDeserializer();

            public object Deserialize<T>(AnalysisSerializer serializer, Stack<DeserializationFrame> stack, BinaryReader reader) {
                int count = reader.ReadInt32();
                var value = new List<T>();
                serializer.Memoize(value);

                DeserializationFunc adder = newValue => value.Add((T)newValue);
                for (int i = count - 1; i >= 0; i--) {
                    stack.Push(new DeserializationFrame(adder));
                }
                return value;
            }
        }

        private object DeserializeList(Stack<DeserializationFrame> stack, BinaryReader reader) {
            return DeserializeClrType(this, stack, ListDeserializer.Instance, reader);
        }

        #endregion

        #region HashSet support

        class HashSetDeserializer : IClrTypeDeserializer {
            public static readonly HashSetDeserializer Instance = new HashSetDeserializer();

            public HashSetDeserializer() {
            }

            public object Deserialize<T>(AnalysisSerializer serializer, Stack<DeserializationFrame> stack, BinaryReader reader) {
                return serializer.DeserializeHashSet<T>(stack, reader);
            }
        }

        private object DeserializeHashSet(Stack<DeserializationFrame> stack, BinaryReader reader) {
            return DeserializeClrType(this, stack, HashSetDeserializer.Instance, reader);
        }

        private HashSet<T> DeserializeHashSet<T>(Stack<DeserializationFrame> stack, BinaryReader reader) {
            int count = reader.ReadInt32();
            var comparer = (IEqualityComparer<T>)DeserializeComparer(reader);
            var value = new HashSet<T>(comparer);            
            Memoize(value);

            DeserializationFunc adder = newValue => _postProcess.Add(() => value.Add((T)newValue));
            for (int i = 0; i < count; i++) {
                stack.Push(new DeserializationFrame(adder));
            }
            return value;
        }

        #endregion

        #region Generic type deserialization

        /// <summary>
        /// Creates a function which will handle the IClrTypeDeserializer interface.  This allows us to
        /// callback with a generic type parameter so that we can create generic instances of lists,
        /// dicts, arrays, etc...
        /// </summary>
        private static Func<AnalysisSerializer, Stack<DeserializationFrame>, TInterface, BinaryReader, object> CreateClrDeserializer<TInterface>() {
            var stack = LinqExpr.Parameter(typeof(Stack<DeserializationFrame>));
            var deserializer = LinqExpr.Parameter(typeof(TInterface));
            var serializer = LinqExpr.Parameter(typeof(AnalysisSerializer));
            var reader = LinqExpr.Parameter(typeof(BinaryReader));
            var valueType = LinqExpr.Parameter(typeof(int));

            var cases = new List<System.Linq.Expressions.SwitchCase>();
            foreach (var type in AnalysisSerializationSupportedTypeAttribute.AllowedTypeIndexes) {
                MethodInfo method = typeof(TInterface).GetMethod("Deserialize");
                var args = method.GetGenericArguments();
                if ((args[0].GenericParameterAttributes & GenericParameterAttributes.ReferenceTypeConstraint) != 0 &&
                    type.Key.IsValueType) {
                    continue;
                }

                cases.Add(
                    LinqExpr.SwitchCase(
                        LinqExpr.Call(
                            deserializer,
                            method.MakeGenericMethod(type.Key),
                            serializer,
                            stack,
                            reader
                        ),
                        LinqExpr.Constant(type.Value)
                    )
                );
            }

            return LinqExpr.Lambda<Func<AnalysisSerializer, Stack<DeserializationFrame>, TInterface, BinaryReader, object>>(
                LinqExpr.Block(
                    new[] { valueType },

                    // var valueType = (ClrType)reader.ReadByte();
                    LinqExpr.Assign(
                        valueType,
                        LinqExpr.Convert(
                            LinqExpr.Call(
                                reader,
                                typeof(BinaryReader).GetMethod("ReadByte")
                            ),
                            typeof(int)
                        )
                    ),
                    LinqExpr.Switch(
                        typeof(object),
                        valueType,
                        LinqExpr.Throw(     // default
                            LinqExpr.New(typeof(InvalidOperationException)),
                            typeof(object)
                        ),
                        null,   // comparison
                        cases.ToArray()
                    )
                ),
                serializer,
                stack,
                deserializer,
                reader
            ).Compile();
        }

        private static object DeserializeClrType(AnalysisSerializer serializer, Stack<DeserializationFrame> stack, IClrTypeDeserializer deserializer, BinaryReader reader) {
            if (ClrTypeDeserializer == null) {
                ClrTypeDeserializer = CreateClrDeserializer<IClrTypeDeserializer>();
            }
            return ClrTypeDeserializer(serializer, stack, deserializer, reader);
        }

        private static object DeserializeClrType(AnalysisSerializer serializer, Stack<DeserializationFrame> stack, IClrClassTypeDeserializer deserializer, BinaryReader reader) {
            if (ClrClassTypeDeserializer == null) {
                ClrClassTypeDeserializer = CreateClrDeserializer<IClrClassTypeDeserializer>();
            }
            return ClrClassTypeDeserializer(serializer, stack, deserializer, reader);
        }

        interface IClrTypeDeserializer {
            object Deserialize<T>(AnalysisSerializer serializer, Stack<DeserializationFrame> stack, BinaryReader reader);
        }

        interface IClrClassTypeDeserializer {
            object Deserialize<T>(AnalysisSerializer serializer, Stack<DeserializationFrame> stack, BinaryReader reader) where T : class;
        }

        #endregion

        #region CLR object serialization

        internal static FieldInfo[] GetSerializableMembers(Type type) {
            FieldInfo[] res;
            // we care about the order but reflection doesn't guarantee it.
            if (!_serializationMembers.TryGetValue(type, out res)) {
                List<FieldInfo> fields = new List<FieldInfo>();
                Type curType = type;
                while (curType != null && curType != typeof(object)) {
                    foreach (var field in curType.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)) {
                        if ((field.Attributes & FieldAttributes.NotSerialized) == 0) {
                            fields.Add(field);
                        }
                    }

                    curType = curType.BaseType;
                }
                res = fields.ToArray();
                Array.Sort<FieldInfo>(
                    res,
                    (x, y) => String.CompareOrdinal(x.DeclaringType.FullName + ":" + x.Name, y.DeclaringType.FullName + ":" + y.Name)
                );
                _serializationMembers[type] = res;
            }
            return res;
        }

        #endregion

        /// <summary>
        /// Indicates the type of object being serialized in our stream.  These
        /// values get written to the stream. 
        /// </summary>
        enum SerializationType : byte {
            None,

            /// <summary>
            /// Null value
            /// </summary>
            Null,

            /// <summary>
            /// An object serialized using reflection
            /// </summary>
            ClrObject,

            // Reference to an object in the graph
            ObjectReference,

            // CLR Primitives
            String,
            Bool,
            True,
            False,
            Double,
            Int,
            Long,
            Enum,

            // CLR types we care about...
            HashSet,
            Dictionary,
            List,
            Array,

            // Fixed analysis instances
            NullValue,
            TrueValue,
            FalseValue,
            UndefinedValue,
            GlobalValue,

            AnalysisDictionary,
            AnalysisHashSet,

            // Variable analysis instances
            StringValue,
            NumberValue,
            UnionComparer,

            EmptyAnalysisSet,
            CallDelegate,
            ReferenceDict,
            MissingValue,
            ObjectComparer,
            OrdinalComparer,
            ObjectEqualityComparer,
            IndexSpan,
            EncodedSpan,
        }

        delegate void SerializerFunction(object value, AnalysisSerializer serializer, Stack<object> stack, BinaryWriter writer);
    }

    interface IDeserializeInitialization {
        void Init();
    }

}
