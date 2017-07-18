// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.NodejsTools.Debugger.Serialization
{
    /// <summary>
    /// Defines an interface for a variable.
    /// </summary>
    internal interface INodeVariable
    {
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
