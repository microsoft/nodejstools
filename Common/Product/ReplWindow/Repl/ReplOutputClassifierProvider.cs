// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.NodejsTools.Repl
{
    /// <summary>
    /// Provides the classifier for our repl error output buffer.
    /// </summary>
    [Export(typeof(IClassifierProvider)), ContentType(ReplConstants.ReplOutputContentTypeName)]
    internal class ReplOutputClassifierProvider : IClassifierProvider
    {
        private readonly Dictionary<InteractiveWindowColor, IClassificationType> classTypes = new Dictionary<InteractiveWindowColor, IClassificationType>();

        [ImportingConstructor]
        public ReplOutputClassifierProvider(IClassificationTypeRegistryService classificationService)
        {
            classTypes[InteractiveWindowColor.Foreground] = classificationService.GetClassificationType("Text");
            classTypes[InteractiveWindowColor.Error] = classificationService.GetClassificationType(InteractiveErrorFormatDefinition.Name);

            classTypes[InteractiveWindowColor.Black] = classificationService.GetClassificationType(InteractiveBlackFormatDefinition.Name);
            classTypes[InteractiveWindowColor.DarkBlue] = classificationService.GetClassificationType(InteractiveDarkBlueFormatDefinition.Name);
            classTypes[InteractiveWindowColor.DarkGreen] = classificationService.GetClassificationType(InteractiveDarkGreenFormatDefinition.Name);
            classTypes[InteractiveWindowColor.DarkCyan] = classificationService.GetClassificationType(InteractiveDarkCyanFormatDefinition.Name);
            classTypes[InteractiveWindowColor.DarkRed] = classificationService.GetClassificationType(InteractiveDarkRedFormatDefinition.Name);
            classTypes[InteractiveWindowColor.DarkMagenta] = classificationService.GetClassificationType(InteractiveDarkMagentaFormatDefinition.Name);
            classTypes[InteractiveWindowColor.DarkYellow] = classificationService.GetClassificationType(InteractiveDarkYellowFormatDefinition.Name);
            classTypes[InteractiveWindowColor.Gray] = classificationService.GetClassificationType(InteractiveGrayFormatDefinition.Name);
            classTypes[InteractiveWindowColor.DarkGray] = classificationService.GetClassificationType(InteractiveDarkGrayFormatDefinition.Name);
            classTypes[InteractiveWindowColor.Blue] = classificationService.GetClassificationType(InteractiveBlueFormatDefinition.Name);
            classTypes[InteractiveWindowColor.Green] = classificationService.GetClassificationType(InteractiveGreenFormatDefinition.Name);
            classTypes[InteractiveWindowColor.Cyan] = classificationService.GetClassificationType(InteractiveCyanFormatDefinition.Name);
            classTypes[InteractiveWindowColor.Red] = classificationService.GetClassificationType(InteractiveRedFormatDefinition.Name);
            classTypes[InteractiveWindowColor.Magenta] = classificationService.GetClassificationType(InteractiveMagentaFormatDefinition.Name);
            classTypes[InteractiveWindowColor.Yellow] = classificationService.GetClassificationType(InteractiveYellowFormatDefinition.Name);
            classTypes[InteractiveWindowColor.White] = classificationService.GetClassificationType(InteractiveWhiteFormatDefinition.Name);
        }

        public bool TryGetValue(InteractiveWindowColor key, out IClassificationType value)
        {
            return classTypes.TryGetValue(key, out value);
        }

        public IClassifier GetClassifier(ITextBuffer textBuffer)
        {
            return new ReplOutputClassifier(this, textBuffer);
        }
    }
}
