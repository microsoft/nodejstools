// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.InteractiveWindow;
using Microsoft.VisualStudio.InteractiveWindow.Commands;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;

namespace Microsoft.NodejsTools.Repl
{
    internal abstract class InteractiveWindowCommand : IInteractiveWindowCommand
    {
        public abstract Task<ExecutionResult> Execute(IInteractiveWindow window, string arguments);

        public abstract string Command { get; }

        public abstract string Description { get; }

        public virtual IEnumerable<string> Names
        {
            get
            {
                yield return this.Command;
            }
        }

        public virtual IEnumerable<string> DetailedDescription
        {
            get
            {
                yield return this.Description;
            }
        }

        public virtual IEnumerable<KeyValuePair<string, string>> ParametersDescription
        {
            get
            {
                yield break;
            }
        }

        public virtual string CommandLine => null;

        public virtual IEnumerable<ClassificationSpan> ClassifyArguments(ITextSnapshot snapshot, Span argumentsSpan, Span spanToClassify)
        {
            yield break;
        }
    }
}
