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

namespace Microsoft.NodejsTools.Debugger.Serialization {
    /// <summary>
    /// Defines an interface for a variable.
    /// </summary>
    interface INodeVariable {
        /// <summary>
        /// Gets a variable identifier.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Gets a parent variable.
        /// </summary>
        NodeEvaluationResult Parent { get; }

        /// <summary>
        /// Gets or sets a stack frame.
        /// </summary>
        NodeStackFrame StackFrame { get; }

        /// <summary>
        /// Gets a variable name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets a variable type name.
        /// </summary>
        string TypeName { get; }

        /// <summary>
        /// Gets a variable value.
        /// </summary>
        string Value { get; }

        /// <summary>
        /// Gets a variable class.
        /// </summary>
        string Class { get; }

        /// <summary>
        /// Gets a variable text.
        /// </summary>
        string Text { get; }

        /// <summary>
        /// Gets a variable attributes.
        /// </summary>
        NodePropertyAttributes Attributes { get; }

        /// <summary>
        /// Gets a variable type.
        /// </summary>
        NodePropertyType Type { get; }
    }
}