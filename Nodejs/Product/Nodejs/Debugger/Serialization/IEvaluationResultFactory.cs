// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.NodejsTools.Debugger.Serialization
{
    internal interface IEvaluationResultFactory
    {
        /// <summary>
        /// Creates a new <see cref="NodeEvaluationResult" />.
        /// </summary>
        /// <param name="variable">Variable provider.</param>
        /// <returns>Result.</returns>
        NodeEvaluationResult Create(INodeVariable variable);
    }
}
