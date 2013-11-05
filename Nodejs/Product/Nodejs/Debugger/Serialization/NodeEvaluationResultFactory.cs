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

using Microsoft.VisualStudioTools.Project;

namespace Microsoft.NodejsTools.Debugger.Serialization {
    class NodeEvaluationResultFactory : INodeEvaluationResultFactory {
        /// <summary>
        /// Creates a new <see cref="NodeEvaluationResult" />.
        /// </summary>
        /// <param name="variable">Variable provider.</param>
        /// <returns>Result.</returns>
        public NodeEvaluationResult Create(INodeVariable variable) {
            Utilities.ArgumentNotNull("variable", variable);

            int id = variable.Id;
            NodeStackFrame stackFrame = variable.StackFrame;
            NodeEvaluationResult parent = variable.Parent;

            string name = variable.Name;
            string fullName = GetFullName(parent, variable.Name, ref name);
            string stringValue, hexValue = null;
            string typeName = variable.TypeName;
            var type = NodeExpressionType.None;

            switch (typeName) {
                case "undefined":
                    stringValue = "undefined";
                    typeName = NodeVariableType.Undefined;
                    break;

                case "null":
                    stringValue = "null";
                    typeName = NodeVariableType.Null;
                    break;

                case "number":
                    stringValue = variable.Value;
                    typeName = NodeVariableType.Number;
                    int intValue;
                    if (int.TryParse(stringValue, out intValue)) {
                        hexValue = string.Format("0x{0:x}", intValue);
                    }
                    break;

                case "boolean":
                    stringValue = variable.Value.ToLowerInvariant();
                    typeName = NodeVariableType.Boolean;
                    type |= NodeExpressionType.Boolean;
                    break;

                case "regexp":
                    stringValue = variable.Value;
                    typeName = NodeVariableType.Regexp;
                    type |= NodeExpressionType.Expandable;
                    break;

                case "function":
                    stringValue = string.IsNullOrEmpty(variable.Text) ? string.Format("{{{0}}}", NodeVariableType.Function) : variable.Text;
                    typeName = NodeVariableType.Function;
                    type |= NodeExpressionType.Function | NodeExpressionType.Expandable;
                    break;

                case "string":
                    stringValue = variable.Value;
                    typeName = NodeVariableType.String;
                    type |= NodeExpressionType.String;
                    break;

                case "object":
                    stringValue = variable.Class == NodeVariableType.Object ? "{...}" : string.Format("{{{0}}}", variable.Class);
                    typeName = NodeVariableType.Object;
                    type |= NodeExpressionType.Expandable;
                    break;

                case "error":
                    stringValue = variable.Value;
                    if (!string.IsNullOrEmpty(stringValue) && stringValue.StartsWith("Error: ")) {
                        stringValue = variable.Value.Substring(7);
                    }
                    typeName = NodeVariableType.Error;
                    type |= NodeExpressionType.Expandable;
                    break;

                default:
                    stringValue = string.IsNullOrEmpty(variable.Value) ? variable.Text : variable.Value;
                    typeName = NodeVariableType.Unknown;
                    break;
            }

            if (variable.Attributes.HasFlag(NodePropertyAttributes.ReadOnly)) {
                type |= NodeExpressionType.ReadOnly;
            }

            if (variable.Attributes.HasFlag(NodePropertyAttributes.DontEnum)) {
                type |= NodeExpressionType.Private;
            }

            return new NodeEvaluationResult(id, stringValue, hexValue, typeName, name, fullName, type, stackFrame);
        }

        private static string GetFullName(NodeEvaluationResult parent, string fullName, ref string name) {
            if (parent == null) {
                return fullName;
            }

            fullName = string.Format(@"{0}[""{1}""]", parent.FullName, name);

            if (parent.TypeName != NodeVariableType.Object) {
                return fullName;
            }

            int indexer;
            if (int.TryParse(name, out indexer)) {
                name = string.Format("[{0}]", indexer);
                fullName = parent.FullName + name;
            }

            return fullName;
        }
    }
}