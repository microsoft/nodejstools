// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.InteractiveWindow;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.NodejsTools.Repl
{
    /// <summary>
    /// Provides the classifier for our repl error output buffer.
    /// </summary>
    [Export(typeof(IClassifierProvider)), ContentType(PredefinedInteractiveContentTypes.InteractiveOutputContentTypeName)]
    internal class ReplOutputClassifierProvider : IClassifierProvider
    {
        private readonly Dictionary<InteractiveWindowColor, IClassificationType> classTypes = new Dictionary<InteractiveWindowColor, IClassificationType>();

        [ImportingConstructor]
        public ReplOutputClassifierProvider(IClassificationTypeRegistryService classificationService)
        {
            this.classTypes[InteractiveWindowColor.Foreground] = classificationService.GetClassificationType("Text");
            this.classTypes[InteractiveWindowColor.Error] = classificationService.GetClassificationType(InteractiveErrorFormatDefinition.Name);

            this.classTypes[InteractiveWindowColor.Black] = classificationService.GetClassificationType(InteractiveBlackFormatDefinition.Name);
            this.classTypes[InteractiveWindowColor.DarkBlue] = classificationService.GetClassificationType(InteractiveDarkBlueFormatDefinition.Name);
            this.classTypes[InteractiveWindowColor.DarkGreen] = classificationService.GetClassificationType(InteractiveDarkGreenFormatDefinition.Name);
            this.classTypes[InteractiveWindowColor.DarkCyan] = classificationService.GetClassificationType(InteractiveDarkCyanFormatDefinition.Name);
            this.classTypes[InteractiveWindowColor.DarkRed] = classificationService.GetClassificationType(InteractiveDarkRedFormatDefinition.Name);
            this.classTypes[InteractiveWindowColor.DarkMagenta] = classificationService.GetClassificationType(InteractiveDarkMagentaFormatDefinition.Name);
            this.classTypes[InteractiveWindowColor.DarkYellow] = classificationService.GetClassificationType(InteractiveDarkYellowFormatDefinition.Name);
            this.classTypes[InteractiveWindowColor.Gray] = classificationService.GetClassificationType(InteractiveGrayFormatDefinition.Name);
            this.classTypes[InteractiveWindowColor.DarkGray] = classificationService.GetClassificationType(InteractiveDarkGrayFormatDefinition.Name);
            this.classTypes[InteractiveWindowColor.Blue] = classificationService.GetClassificationType(InteractiveBlueFormatDefinition.Name);
            this.classTypes[InteractiveWindowColor.Green] = classificationService.GetClassificationType(InteractiveGreenFormatDefinition.Name);
            this.classTypes[InteractiveWindowColor.Cyan] = classificationService.GetClassificationType(InteractiveCyanFormatDefinition.Name);
            this.classTypes[InteractiveWindowColor.Red] = classificationService.GetClassificationType(InteractiveRedFormatDefinition.Name);
            this.classTypes[InteractiveWindowColor.Magenta] = classificationService.GetClassificationType(InteractiveMagentaFormatDefinition.Name);
            this.classTypes[InteractiveWindowColor.Yellow] = classificationService.GetClassificationType(InteractiveYellowFormatDefinition.Name);
            this.classTypes[InteractiveWindowColor.White] = classificationService.GetClassificationType(InteractiveWhiteFormatDefinition.Name);
        }

        public bool TryGetValue(InteractiveWindowColor key, out IClassificationType value)
        {
            return this.classTypes.TryGetValue(key, out value);
        }

        public IClassifier GetClassifier(ITextBuffer textBuffer)
        {
            return new ReplOutputClassifier(this, textBuffer);
        }
    }
}
