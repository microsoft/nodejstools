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
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.NodejsTools.Analysis.AnalysisSetDetails;
using Microsoft.NodejsTools.Analysis.Analyzer;
using Microsoft.NodejsTools.Analysis.Values;
using LinqExpr = System.Linq.Expressions.Expression;

namespace Microsoft.NodejsTools.Analysis {
    public class AnalysisSerializer {
        private readonly List<object> _memoDict = new List<object>();
        private readonly Dictionary<object, int> _reverseMemo = new Dictionary<object, int>(new ReferenceComparer<object>());
        private readonly List<Action> _postProcess = new List<Action>();
        private static Dictionary<Type, MemberInfo[]> _serializationMembers = new Dictionary<Type, MemberInfo[]>();

        private static readonly Dictionary<Type, SerializerFunction> _serializer = new Dictionary<Type, SerializerFunction>() {
            { typeof(CallDelegate), SerializeCallDelegate },
            { typeof(UnionComparer), SerializeUnionComparer },
            { typeof(ReferenceDict), SerializeReferenceDict },
            { typeof(string), SerializeString },
            { typeof(bool), SerializeBool },
            { typeof(double), SerializeDouble },
            { typeof(int), SerializeInt },
            { typeof(long), SerializeLong },
        };
        private static readonly Dictionary<Type, SerializationType> _simpleTypes = new Dictionary<Type, SerializationType>() {
            { typeof(AnalysisSetEmptyObject), SerializationType.EmptyAnalysisSet },
            { typeof(Microsoft.NodejsTools.Parsing.Missing), SerializationType.MissingValue },
            { typeof(ObjectComparer), SerializationType.ObjectComparer },
            { StringComparer.Ordinal.GetType(), SerializationType.OrdinalComparer }
        };

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


        private const int EnumInt32 = 0;

        delegate void DeserializationFunc(object value);

        private object DeserializeObject(BinaryReader reader) {
            Stack<DeserializationFunc> stack = new Stack<DeserializationFunc>();
            object res = null;
            stack.Push(value => res = value);
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
                    case SerializationType.Dictionary:
                        nextValue = DeserializeDictionary(stack, reader);
                        break;
                    case SerializationType.Array:
                        nextValue = DeserializeArray(stack, reader);
                        break;
                    case SerializationType.Bool: nextValue = reader.ReadBoolean(); break;
                    case SerializationType.Double: nextValue = reader.ReadDouble(); break;
                    case SerializationType.String: nextValue = reader.ReadString(); break;
                    case SerializationType.Int: nextValue = reader.ReadInt32(); break;
                    case SerializationType.Long: nextValue = reader.ReadInt64(); break;
                    case SerializationType.Enum:
                        switch (reader.ReadByte()) {
                            case EnumInt32:
                                nextValue = reader.ReadInt32();
                                break;
                            default:
                                throw new InvalidOperationException("unsupported enum type");
                        }
                        break;
                    case SerializationType.CallDelegate: nextValue = DeserializeCallDelegate(reader); break;
                    case SerializationType.EmptyAnalysisSet:
                        nextValue = AnalysisSetEmptyObject.Instance;
                        break;
                    case SerializationType.UnionComparer:
                        nextValue = DeserializeUnionComparer(reader);
                        break;
                    case SerializationType.ObjectComparer:
                        nextValue = ObjectComparer.Instance;
                        break;
                    case SerializationType.OrdinalComparer:
                        nextValue = StringComparer.Ordinal;
                        break;
                    case SerializationType.ObjectEqualityComparer:
                        nextValue = DeserializeClrType(stack, EqualityComparerDeserializer.Instance, reader);
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
                            var member = (FieldInfo)members[i];
                            if (i == members.Length - 1 && value is ValueType) {
                                // we can't assign the ValueType until all of its fields have been initialized,
                                // so queue up the current next assignment after our initialization
                                var tempNextAssign = nextAssign;
                                var tempValue = value;
                                stack.Push(newValue => {
                                    member.SetValue(value, newValue);
                                    tempNextAssign(tempValue);
                                });
                            } else {
                                stack.Push(newValue => member.SetValue(value, newValue));
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
                        throw new InvalidOperationException("unsupported SerializationType");
                }

                nextAssign(nextValue);
            } while (stack.Count > 0);

            return res;
        }

        private static readonly Type EqualityComparerGenericTypeDef = EqualityComparer<object>.Default.GetType().GetGenericTypeDefinition();
        private static readonly Type GenericEqualityComparerGenericTypeDef = EqualityComparer<TimeSpan>.Default.GetType().GetGenericTypeDefinition();

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
                SerializationType simpleType;
                if (_simpleTypes.TryGetValue(value.GetType(), out simpleType)) {
                    writer.Write((byte)simpleType);
                } else if (_serializer.TryGetValue(value.GetType(), out serializerFunc)) {
                    serializerFunc(value, this, stack, writer);
                } else if (value.GetType().IsEnum) {
                    SerializeEnum(value, writer);
                } else if (AnalysisSerializationSupportedTypeAttribute.AllowedTypeIndexes.ContainsKey(value.GetType())) {
                    ReverseMemoize(value);

                    writer.Write((byte)SerializationType.ClrObject);
                    writer.Write((byte)AnalysisSerializationSupportedTypeAttribute.AllowedTypeIndexes[value.GetType()]);

                    var members = GetSerializableMembers(value.GetType());
                    for (int i = members.Length - 1; i >= 0; i--) {
                        stack.Push(((FieldInfo)members[i]).GetValue(value));
                    }
                } else if (value.GetType().IsDefined(typeof(AnalysisSerializeAsNullAttribute), false)) {
                    writer.Write((byte)SerializationType.Null);
                } else if (value.GetType().IsGenericType) {
                    var gtd = value.GetType().GetGenericTypeDefinition();
                    if (gtd == typeof(List<>)) {
                        ReverseMemoize(value);

                        writer.Write((byte)SerializationType.List);
                        WriteClrType(value.GetType().GetGenericArguments()[0], writer);
                        var list = (IList)value;
                        writer.Write(list.Count);
                        for (int i = list.Count - 1; i >= 0; i--) {
                            stack.Push(list[i]);
                        }
                    } else if (gtd == typeof(Dictionary<,>)) {
                        ReverseMemoize(value);

                        writer.Write((byte)SerializationType.Dictionary);
                        WriteClrType(value.GetType().GetGenericArguments()[0], writer);
                        WriteClrType(value.GetType().GetGenericArguments()[1], writer);
                        var dictionary = (IDictionary)value;
                        writer.Write(dictionary.Count);
                        Serialize(dictionary.GetType().GetProperty("Comparer").GetValue(dictionary, new object[0]), writer);
                        foreach (var key in dictionary.Keys) {
                            stack.Push(dictionary[key]);
                            stack.Push(key);
                        }
                    } else if (gtd == typeof(HashSet<>)) {
                        ReverseMemoize(value);

                        writer.Write((byte)SerializationType.HashSet);
                        WriteClrType(value.GetType().GetGenericArguments()[0], writer);

                        var enumerable = (IEnumerable)value;                        
                        int count = 0;
                        foreach (var obj in enumerable) {
                            count++;
                        }
                        writer.Write(count);
                        Serialize(enumerable.GetType().GetProperty("Comparer").GetValue(enumerable, new object[0]), writer);
                        foreach (var obj in enumerable) {
                            stack.Push(obj);
                        }
                    } else if (gtd == EqualityComparerGenericTypeDef || gtd == GenericEqualityComparerGenericTypeDef) {
                        writer.Write((byte)SerializationType.ObjectEqualityComparer);
                        WriteClrType(value.GetType().GetGenericArguments()[0], writer);
                    } else {
                        throw new InvalidOperationException("unsupported generic type: " + value.GetType());
                    }
                } else if (value.GetType().IsArray) {
                    ReverseMemoize(value);
                    var arr = (Array)value;

                    writer.Write((byte)SerializationType.Array);
                    WriteClrType(value.GetType().GetElementType(), writer);
                    writer.Write(arr.Length);
                    for (int i = arr.Length - 1; i >= 0; i--) {
                        stack.Push(arr.GetValue(i));
                    }
                } else {
                    throw new InvalidOperationException("unsupported type: " + value.GetType());
                }
            } while (stack.Count > 0);
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

        private static void SerializeNullValue(object value, BinaryWriter writer) {
            writer.Write((byte)SerializationType.NullValue);
        }

        private static void SerializeUndefinedValue(object value, BinaryWriter writer) {
            writer.Write((byte)SerializationType.UndefinedValue);
        }

        private static void SerializeGlobalValue(object value, BinaryWriter writer) {
            writer.Write((byte)SerializationType.GlobalValue);
        }

        private static void SerializeStringValue(object value, BinaryWriter writer) {
            writer.Write((byte)SerializationType.StringValue);
            writer.Write(((StringValue)value)._value);
        }
        private static void SerializeNumberValue(object value, BinaryWriter writer) {
            writer.Write((byte)SerializationType.NumberValue);
            writer.Write(((NumberValue)value)._value);
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

        private object DeserializeReferenceDict(Stack<DeserializationFunc> stack, BinaryReader reader) {
            return DeserializeDictionary<IProjectEntry, ReferenceList>(stack, reader, new ReferenceDict(), reader.ReadInt32());
        }

        private object DeserializeCallDelegate(BinaryReader reader) {
            Type declType;
            switch((CallDelegateDeclType)reader.ReadByte()) {
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
            writer.Write((byte)SerializationType.String);
            writer.Write((string)value);
        }

        private static void SerializeDouble(object value, AnalysisSerializer serializer, Stack<object> stack, BinaryWriter writer) {
            writer.Write((byte)SerializationType.Double);
            writer.Write((double)value);
        }

        private static void SerializeBool(object value, AnalysisSerializer serializer, Stack<object> stack, BinaryWriter writer) {
            writer.Write((byte)SerializationType.Bool);
            writer.Write((bool)value);
        }

        private static void SerializeInt(object value, AnalysisSerializer serializer, Stack<object> stack, BinaryWriter writer) {
            writer.Write((byte)SerializationType.Int);
            writer.Write((int)value);
        }

        private static void SerializeLong(object value, AnalysisSerializer serializer, Stack<object> stack, BinaryWriter writer) {
            writer.Write((byte)SerializationType.Long);
            writer.Write((long)value);
        }

        private static void SerializeBooleanValue(object value, BinaryWriter writer) {
            BooleanValue boolean = (BooleanValue)value;
            if (boolean._value) {
                writer.Write((byte)SerializationType.TrueValue);
            } else {
                writer.Write((byte)SerializationType.FalseValue);
            }
        }

        class EqualityComparerDeserializer : IClrTypeDeserializer {
            public static readonly EqualityComparerDeserializer Instance = new EqualityComparerDeserializer();

            public object Deserialize<T>(Stack<DeserializationFunc> stack, BinaryReader reader) {
                return EqualityComparer<T>.Default;
            }
        }


        #region Array support

        private object DeserializeArray(Stack<DeserializationFunc> stack, BinaryReader reader) {
            return DeserializeClrType(stack, new ArrayDeserializer(this), reader);
        }

        private void SerializeArray(object graph, BinaryWriter writer) {
            var arr = (Array)graph;
            writer.Write((byte)SerializationType.Array);
            WriteClrType(graph.GetType().GetElementType(), writer);
            writer.Write(arr.Length);
            foreach (var value in arr) {
                Serialize(value, writer);
            }
        }

        class ArrayDeserializer : IClrTypeDeserializer {
            private readonly AnalysisSerializer _serializer;
            public ArrayDeserializer(AnalysisSerializer serializer) {
                _serializer = serializer;
            }

            public object Deserialize<T>(Stack<DeserializationFunc> stack, BinaryReader reader) {
                int length = reader.ReadInt32();
                var arr = new T[length];
                _serializer.Memoize(arr);
                for (int i = length - 1; i >= 0; i--) {
                    var index = i;
                    stack.Push(
                        newValue => arr[index] = (T)newValue
                    );
                }
                return arr;
            }
        }

        #endregion
        
        #region Dictionary support

        class DictionaryKeyDeserializer : IClrTypeDeserializer {
            private readonly AnalysisSerializer _serializer;
            public DictionaryKeyDeserializer(AnalysisSerializer serializer) {
                _serializer = serializer;
            }

            public object Deserialize<T>(Stack<DeserializationFunc> stack, BinaryReader reader) {
                return DeserializeClrType(stack, new DictionaryValueDeserializer<T>(_serializer), reader);
            }
        }

        class DictionaryValueDeserializer<TKey> : IClrTypeDeserializer {
            private readonly AnalysisSerializer _serializer;

            public DictionaryValueDeserializer(AnalysisSerializer serializer) {
                _serializer = serializer;
            }

            public object Deserialize<T>(Stack<DeserializationFunc> stack, BinaryReader reader) {
                return _serializer.DeserializeDictionary<TKey, T>(stack, reader);
            }
        }

        private object DeserializeDictionary(Stack<DeserializationFunc> stack, BinaryReader reader) {
            return DeserializeClrType(stack, new DictionaryKeyDeserializer(this), reader);
        }

        private IDictionary DeserializeDictionary<TKey, TValue>(Stack<DeserializationFunc> stack, BinaryReader reader) {
            int count = reader.ReadInt32();
            
            var comparer = (IEqualityComparer<TKey>)DeserializeObject(reader);
            var value = new Dictionary<TKey, TValue>(comparer);

            return DeserializeDictionary<TKey, TValue>(stack, reader, value, count);
        }

        private IDictionary DeserializeDictionary<TKey, TValue>(Stack<DeserializationFunc> stack, BinaryReader reader, Dictionary<TKey, TValue> value, int count) {            
            Memoize(value);

            for (int i = 0; i < count; i++) {
                object key = null;
                stack.Push(newValue => _postProcess.Add(() => value[(TKey)key] = (TValue)newValue));
                stack.Push(newKey => key = newKey);
            }
            return value;
        }

        #endregion

        #region List support

        private void SerializeList(IList list, BinaryWriter writer) {
            writer.Write(list.Count);
            foreach (var value in list) {
                Serialize(value, writer);
            }
        }

        class ListDeserializer : IClrTypeDeserializer {
            private readonly AnalysisSerializer _serializer;
            
            public ListDeserializer(AnalysisSerializer serializer) {
                _serializer = serializer;
            }

            public object Deserialize<T>(Stack<DeserializationFunc> stack, BinaryReader reader) {
                int count = reader.ReadInt32();
                var value = new List<T>();
                _serializer.Memoize(value);

                for (int i = count - 1; i >= 0; i--) {
                    stack.Push(newValue => value.Add((T)newValue));
                }
                return value;
            }
        }

        private object DeserializeList(Stack<DeserializationFunc> stack, BinaryReader reader) {
            return DeserializeClrType(stack, new ListDeserializer(this), reader);
        }

        #endregion

        #region HashSet support

        class HashSetDeserializer : IClrTypeDeserializer {
            private readonly AnalysisSerializer _serializer;

            public HashSetDeserializer(AnalysisSerializer serializer) {
                _serializer = serializer;
            }

            public object Deserialize<T>(Stack<DeserializationFunc> stack, BinaryReader reader) {
                return _serializer.DeserializeHashSet<T>(stack, reader);
            }
        }

        private object DeserializeHashSet(Stack<DeserializationFunc> stack, BinaryReader reader) {
            return DeserializeClrType(stack, new HashSetDeserializer(this), reader);
        }

        private HashSet<T> DeserializeHashSet<T>(Stack<DeserializationFunc> stack, BinaryReader reader) {
            int count = reader.ReadInt32();
            var comparer = (IEqualityComparer<T>)DeserializeObject(reader);
            var value = new HashSet<T>(comparer);
            Memoize(value);

            for (int i = 0; i < count; i++) {
                stack.Push(newValue => _postProcess.Add(() => value.Add((T)newValue)));
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
        private static Func<Stack<DeserializationFunc>, IClrTypeDeserializer, BinaryReader, object> CreateClrDeserializer() {
            var stack = LinqExpr.Parameter(typeof(Stack<DeserializationFunc>));
            var deserializer = LinqExpr.Parameter(typeof(IClrTypeDeserializer));
            var reader = LinqExpr.Parameter(typeof(BinaryReader));
            var valueType = LinqExpr.Parameter(typeof(int));

            var cases = new List<System.Linq.Expressions.SwitchCase>();
            foreach (var type in AnalysisSerializationSupportedTypeAttribute.AllowedTypeIndexes) {
                cases.Add(
                    LinqExpr.SwitchCase(
                        LinqExpr.Call(
                            deserializer,
                            typeof(IClrTypeDeserializer).GetMethod("Deserialize").MakeGenericMethod(type.Key),
                            stack,
                            reader
                        ),
                        LinqExpr.Constant(type.Value)
                    )
                );
            }

            return LinqExpr.Lambda<Func<Stack<DeserializationFunc>, IClrTypeDeserializer, BinaryReader, object>>(
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
                stack,
                deserializer,
                reader
            ).Compile();
        }

        private static Func<Stack<DeserializationFunc>, IClrTypeDeserializer, BinaryReader, object> ClrTypeDeserializer;

        private static object DeserializeClrType(Stack<DeserializationFunc> stack, IClrTypeDeserializer deserializer, BinaryReader reader) {
            if (ClrTypeDeserializer == null) {
                ClrTypeDeserializer = CreateClrDeserializer();
            }
            return ClrTypeDeserializer(stack, deserializer, reader);
        }

        interface IClrTypeDeserializer {
            object Deserialize<T>(Stack<DeserializationFunc> stack, BinaryReader reader);
        }

        #endregion

        #region CLR object serialization

        private void SerializeClrObject(object obj, BinaryWriter writer) {
            writer.Write((byte)SerializationType.ClrObject);
            writer.Write((byte)AnalysisSerializationSupportedTypeAttribute.AllowedTypeIndexes[obj.GetType()]);

            var members = GetSerializableMembers(obj.GetType());
            foreach (FieldInfo member in members) {
                Serialize(member.GetValue(obj), writer);
            }
        }

        private static MemberInfo[] GetSerializableMembers(Type type) {
            MemberInfo[] res;
            // we care about the order but reflection doesn't guarantee it.
            if (!_serializationMembers.TryGetValue(type, out res)) {
                res = FormatterServices.GetSerializableMembers(type);
                Array.Sort<MemberInfo>(
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
            ObjectEqualityComparer
        }

        delegate void SerializerFunction(object value, AnalysisSerializer serializer, Stack<object> stack, BinaryWriter writer);
    }

    interface IDeserializeInitialization {
        void Init();
    }

}
