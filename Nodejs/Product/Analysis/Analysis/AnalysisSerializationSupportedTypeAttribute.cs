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
using System.Linq;
using Microsoft.NodejsTools.Analysis;
using Microsoft.NodejsTools.Analysis.AnalysisSetDetails;
using Microsoft.NodejsTools.Analysis.Analyzer;
using Microsoft.NodejsTools.Analysis.Values;
using Microsoft.NodejsTools.Parsing;

[assembly: AnalysisSerializationSupportedType(typeof(object))]
[assembly: AnalysisSerializationSupportedType(typeof(int))]
[assembly: AnalysisSerializationSupportedType(typeof(string))]
[assembly: AnalysisSerializationSupportedType(typeof(DictionaryEntry))]
[assembly: AnalysisSerializationSupportedType(typeof(Deque<AnalysisUnit>))]
[assembly: AnalysisSerializationSupportedType(typeof(ProjectEntry))]
[assembly: AnalysisSerializationSupportedType(typeof(JsAnalyzer))]
[assembly: AnalysisSerializationSupportedType(typeof(JsAnalyzer.TreeUpdateAnalysis))]
[assembly: AnalysisSerializationSupportedType(typeof(ModuleTable))]
[assembly: AnalysisSerializationSupportedType(typeof(ModuleTree))]
[assembly: AnalysisSerializationSupportedType(typeof(FunctionValue))]
[assembly: AnalysisSerializationSupportedType(typeof(UserFunctionValue))]
[assembly: AnalysisSerializationSupportedType(typeof(UserFunctionValue.CallArgs))]
[assembly: AnalysisSerializationSupportedType(typeof(UserFunctionValue.CallInfo))]
[assembly: AnalysisSerializationSupportedType(typeof(ExpandoValue))]
[assembly: AnalysisSerializationSupportedType(typeof(ObjectValue))]
[assembly: AnalysisSerializationSupportedType(typeof(NullValue))]
[assembly: AnalysisSerializationSupportedType(typeof(BooleanValue))]
[assembly: AnalysisSerializationSupportedType(typeof(UndefinedValue))]
[assembly: AnalysisSerializationSupportedType(typeof(GlobalValue))]
[assembly: AnalysisSerializationSupportedType(typeof(StringValue))]
[assembly: AnalysisSerializationSupportedType(typeof(NumberValue))]
[assembly: AnalysisSerializationSupportedType(typeof(ArrayValue))]
[assembly: AnalysisSerializationSupportedType(typeof(InheritsPrototypeValue))]
[assembly: AnalysisSerializationSupportedType(typeof(ObjectLiteralValue))]
[assembly: AnalysisSerializationSupportedType(typeof(OverviewWalker.ObjectLiteralKey))]
[assembly: AnalysisSerializationSupportedType(typeof(InstanceValue))]
[assembly: AnalysisSerializationSupportedType(typeof(ImmutableObjectValue))]
[assembly: AnalysisSerializationSupportedType(typeof(ExportsValue))]
[assembly: AnalysisSerializationSupportedType(typeof(PartiallyImmutableBuiltinFunctionValue))]
[assembly: AnalysisSerializationSupportedType(typeof(BuiltinFunctionValue))]
[assembly: AnalysisSerializationSupportedType(typeof(ModuleValue))]
[assembly: AnalysisSerializationSupportedType(typeof(ModuleEnvironmentRecord.NodeEnvironmentKey))]
[assembly: AnalysisSerializationSupportedType(typeof(IAnalysisSet))]
[assembly: AnalysisSerializationSupportedType(typeof(IAnalyzable))]
[assembly: AnalysisSerializationSupportedType(typeof(IGroupableAnalysisProject))]
[assembly: AnalysisSerializationSupportedType(typeof(FileCookie))]
[assembly: AnalysisSerializationSupportedType(typeof(AnalysisValue))]
[assembly: AnalysisSerializationSupportedType(typeof(AnalysisProxy))]
[assembly: AnalysisSerializationSupportedType(typeof(ExpandoValue.LinkedAnalysisList))]
[assembly: AnalysisSerializationSupportedType(typeof(List<IAnalyzable>))]
[assembly: AnalysisSerializationSupportedType(typeof(PropertyDescriptor))]
[assembly: AnalysisSerializationSupportedType(typeof(ReturningConstructingFunctionValue))]
[assembly: AnalysisSerializationSupportedType(typeof(ReturningFunctionValue))]
[assembly: AnalysisSerializationSupportedType(typeof(SpecializedFunctionValue))]
[assembly: AnalysisSerializationSupportedType(typeof(SpecializedUserFunctionValue))]
[assembly: AnalysisSerializationSupportedType(typeof(PrototypeValue))]
[assembly: AnalysisSerializationSupportedType(typeof(EnvironmentRecord))]
[assembly: AnalysisSerializationSupportedType(typeof(ModuleEnvironmentRecord))]
[assembly: AnalysisSerializationSupportedType(typeof(DefinitiveAssignmentEnvironmentRecord))]
[assembly: AnalysisSerializationSupportedType(typeof(FunctionEnvironmentRecord))]
[assembly: AnalysisSerializationSupportedType(typeof(AnalysisSetTwoObject))]
[assembly: AnalysisSerializationSupportedType(typeof(AnalysisHashSet))]
[assembly: AnalysisSerializationSupportedType(typeof(AnalysisSetEmptyUnion))]
[assembly: AnalysisSerializationSupportedType(typeof(AnalysisSetOneUnion))]
[assembly: AnalysisSerializationSupportedType(typeof(AnalysisSetTwoUnion))]
[assembly: AnalysisSerializationSupportedType(typeof(AnalysisUnit))]
[assembly: AnalysisSerializationSupportedType(typeof(EvalAnalysisUnit))]
[assembly: AnalysisSerializationSupportedType(typeof(FunctionAnalysisUnit))]
[assembly: AnalysisSerializationSupportedType(typeof(CartesianProductFunctionAnalysisUnit))]
[assembly: AnalysisSerializationSupportedType(typeof(CartesianProductFunctionAnalysisUnit.CartesianLocalVariable))]
[assembly: AnalysisSerializationSupportedType(typeof(ReferenceDict))]
[assembly: AnalysisSerializationSupportedType(typeof(ReferenceList))]
[assembly: AnalysisSerializationSupportedType(typeof(DependentKeyValue))]
[assembly: AnalysisSerializationSupportedType(typeof(VariableDef))]
[assembly: AnalysisSerializationSupportedType(typeof(TypedDef))]
[assembly: AnalysisSerializationSupportedType(typeof(DependentData))]
[assembly: AnalysisSerializationSupportedType(typeof(ParameterResult))]
[assembly: AnalysisSerializationSupportedType(typeof(TypedDependencyInfo))]
[assembly: AnalysisSerializationSupportedType(typeof(ReferenceableDependencyInfo))]
[assembly: AnalysisSerializationSupportedType(typeof(SingleDict<ProjectEntry, KeyValueDependencyInfo>))]
[assembly: AnalysisSerializationSupportedType(typeof(SingleDict<ProjectEntry, KeyValueDependencyInfo>.SingleDependency))]
[assembly: AnalysisSerializationSupportedType(typeof(SingleDict<ProjectEntry, TypedDependencyInfo>))]
[assembly: AnalysisSerializationSupportedType(typeof(SingleDict<ProjectEntry, TypedDependencyInfo>.SingleDependency))]
[assembly: AnalysisSerializationSupportedType(typeof(SingleDict<ProjectEntry, DependencyInfo>))]
[assembly: AnalysisSerializationSupportedType(typeof(SingleDict<ProjectEntry, DependencyInfo>.SingleDependency))]
[assembly: AnalysisSerializationSupportedType(typeof(SingleDict<ProjectEntry, ReferenceableDependencyInfo>))]
[assembly: AnalysisSerializationSupportedType(typeof(SingleDict<ProjectEntry, ReferenceableDependencyInfo>.SingleDependency))]
[assembly: AnalysisSerializationSupportedType(typeof(LocatedVariableDef))]
[assembly: AnalysisSerializationSupportedType(typeof(SetOfOne<ProjectEntry>))]
[assembly: AnalysisSerializationSupportedType(typeof(SetOfTwo<ProjectEntry>))]
[assembly: AnalysisSerializationSupportedType(typeof(SetOfOne<EncodedLocation>))]
[assembly: AnalysisSerializationSupportedType(typeof(SetOfTwo<EncodedLocation>))]
[assembly: AnalysisSerializationSupportedType(typeof(SetOfOne<AnalysisUnit>))]
[assembly: AnalysisSerializationSupportedType(typeof(SetOfTwo<AnalysisUnit>))]
[assembly: AnalysisSerializationSupportedType(typeof(EncodedLocation))]
[assembly: AnalysisSerializationSupportedType(typeof(EphemeralVariableDef))]
[assembly: AnalysisSerializationSupportedType(typeof(DependencyInfo))]
[assembly: AnalysisSerializationSupportedType(typeof(Node))]
[assembly: AnalysisSerializationSupportedType(typeof(CallNode))]
[assembly: AnalysisSerializationSupportedType(typeof(ConstantWrapper))]
[assembly: AnalysisSerializationSupportedType(typeof(Expression))]
[assembly: AnalysisSerializationSupportedType(typeof(Statement))]
[assembly: AnalysisSerializationSupportedType(typeof(JsAst))]
[assembly: AnalysisSerializationSupportedType(typeof(Block))]
[assembly: AnalysisSerializationSupportedType(typeof(Var))]
[assembly: AnalysisSerializationSupportedType(typeof(IndexSpan))]
[assembly: AnalysisSerializationSupportedType(typeof(Declaration))]
[assembly: AnalysisSerializationSupportedType(typeof(VariableDeclaration))]
[assembly: AnalysisSerializationSupportedType(typeof(Member))]
[assembly: AnalysisSerializationSupportedType(typeof(Lookup))]
[assembly: AnalysisSerializationSupportedType(typeof(JSVariableField))]
[assembly: AnalysisSerializationSupportedType(typeof(IndexResolver))]
[assembly: AnalysisSerializationSupportedType(typeof(SourceLocation))]
[assembly: AnalysisSerializationSupportedType(typeof(ModuleAnalysis))]
[assembly: AnalysisSerializationSupportedType(typeof(ExpressionStatement))]
[assembly: AnalysisSerializationSupportedType(typeof(BinaryOperator))]
[assembly: AnalysisSerializationSupportedType(typeof(HashSet<VariableDef>))]
[assembly: AnalysisSerializationSupportedType(typeof(IProjectEntry))]
[assembly: AnalysisSerializationSupportedType(typeof(InvalidNumericErrorValue))]
[assembly: AnalysisSerializationSupportedType(typeof(JSArgumentField))]
[assembly: AnalysisSerializationSupportedType(typeof(ArrayLiteral))]
[assembly: AnalysisSerializationSupportedType(typeof(Break))]
[assembly: AnalysisSerializationSupportedType(typeof(CommaOperator))]
[assembly: AnalysisSerializationSupportedType(typeof(Conditional))]
[assembly: AnalysisSerializationSupportedType(typeof(ConstStatement))]
[assembly: AnalysisSerializationSupportedType(typeof(ContinueNode))]
[assembly: AnalysisSerializationSupportedType(typeof(DebuggerNode))]
[assembly: AnalysisSerializationSupportedType(typeof(Declaration))]
[assembly: AnalysisSerializationSupportedType(typeof(DirectivePrologue))]
[assembly: AnalysisSerializationSupportedType(typeof(DoWhile))]
[assembly: AnalysisSerializationSupportedType(typeof(EmptyStatement))]
[assembly: AnalysisSerializationSupportedType(typeof(ForNode))]
[assembly: AnalysisSerializationSupportedType(typeof(ForIn))]
[assembly: AnalysisSerializationSupportedType(typeof(FunctionExpression))]
[assembly: AnalysisSerializationSupportedType(typeof(FunctionObject))]
[assembly: AnalysisSerializationSupportedType(typeof(GetterSetter))]
[assembly: AnalysisSerializationSupportedType(typeof(GroupingOperator))]
[assembly: AnalysisSerializationSupportedType(typeof(IfNode))]
[assembly: AnalysisSerializationSupportedType(typeof(IterationStatement))]
[assembly: AnalysisSerializationSupportedType(typeof(JSToken))]
[assembly: AnalysisSerializationSupportedType(typeof(LabeledStatement))]
[assembly: AnalysisSerializationSupportedType(typeof(LexicalDeclaration))]
[assembly: AnalysisSerializationSupportedType(typeof(ObjectLiteral))]
[assembly: AnalysisSerializationSupportedType(typeof(ObjectLiteralField))]
[assembly: AnalysisSerializationSupportedType(typeof(ObjectLiteralProperty))]
[assembly: AnalysisSerializationSupportedType(typeof(ParameterDeclaration))]
[assembly: AnalysisSerializationSupportedType(typeof(RegExpLiteral))]
[assembly: AnalysisSerializationSupportedType(typeof(ReturnNode))]
[assembly: AnalysisSerializationSupportedType(typeof(Switch))]
[assembly: AnalysisSerializationSupportedType(typeof(SwitchCase))]
[assembly: AnalysisSerializationSupportedType(typeof(ThisLiteral))]
[assembly: AnalysisSerializationSupportedType(typeof(ThrowNode))]
[assembly: AnalysisSerializationSupportedType(typeof(TryNode))]
[assembly: AnalysisSerializationSupportedType(typeof(UnaryOperator))]
[assembly: AnalysisSerializationSupportedType(typeof(WhileNode))]
[assembly: AnalysisSerializationSupportedType(typeof(WithNode))]
[assembly: AnalysisSerializationSupportedType(typeof(NodejsModuleBuilder.EventListenerKey))]
[assembly: AnalysisSerializationSupportedType(typeof(KeyValueDependencyInfo))]
[assembly: AnalysisSerializationSupportedType(typeof(AnalysisLimits))]

namespace Microsoft.NodejsTools.Analysis {
    /// <summary>
    /// Marks a type as being available for analysis serialization.  Types not 
    /// marked with this need explicit serialization support in the serializer
    /// or they're not serializable.
    /// 
    /// This type is also required for types used in generic constructs.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    sealed class AnalysisSerializationSupportedTypeAttribute : Attribute {
        private readonly Type _type;
        private static readonly Type[] _allowedTypes = GetAllowedTypes();
        private static readonly Dictionary<Type, int> _allowedTypeIndexes = MakeAllowedTypesIndexes();


        public AnalysisSerializationSupportedTypeAttribute(Type type) {
            _type = type;
        }

        public Type Type {
            get {
                return _type;
            }
        }

        public static Type[] AllowedTypes {
            get {
                return _allowedTypes;
            }
        }

        public static Dictionary<Type, int> AllowedTypeIndexes {
            get {
                return _allowedTypeIndexes;
            }
        }

        private static Type[] GetAllowedTypes() {
            AnalysisSerializationSupportedTypeAttribute[] attrs = (AnalysisSerializationSupportedTypeAttribute[])
                typeof(AnalysisSerializer).Assembly.GetCustomAttributes(
                    typeof(AnalysisSerializationSupportedTypeAttribute),
                    false
                );
            Array.Sort<AnalysisSerializationSupportedTypeAttribute>(attrs,
                (x, y) => String.Compare(x.Type.FullName, y.Type.FullName));
            return attrs.Select(x => x.Type).ToArray();
        }

        private static Dictionary<Type, int> MakeAllowedTypesIndexes() {
            var res = new Dictionary<Type, int>();
            for (int i = 0; i < _allowedTypes.Length; i++) {
                res.Add(_allowedTypes[i], i);
            }
            return res;
        }

    }
}
