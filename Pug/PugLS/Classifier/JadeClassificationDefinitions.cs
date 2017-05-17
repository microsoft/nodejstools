// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.Pug
{
    internal class PugClassificationDefinitions
    {
        [Export(typeof(ClassificationTypeDefinition))]
        [Name(PugClassificationTypes.Keyword), Export]
        internal ClassificationTypeDefinition KeywordClassificationType
        {
            get;
            set;
        }

        [Export(typeof(EditorFormatDefinition))]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = PugClassificationTypes.Keyword)]
        [Name("PugKeywordFormatDefinition")]
        [Order]
        internal sealed class KeywordClassificationFormat : ClassificationFormatDefinition
        {
            internal KeywordClassificationFormat()
            {
                this.ForegroundColor = Colors.OrangeRed;
                this.DisplayName = "Pug Keyword";
            }
        }

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(PugClassificationTypes.Filter), Export]
        internal ClassificationTypeDefinition FilterClassificationType
        {
            get;
            set;
        }

        [Export(typeof(EditorFormatDefinition))]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = PugClassificationTypes.Filter)]
        [Name("PugFilterFormatDefinition")]
        [Order]
        internal sealed class FilterClassificationFormat : ClassificationFormatDefinition
        {
            internal FilterClassificationFormat()
            {
                this.ForegroundColor = Colors.DarkRed;
                this.DisplayName = "Pug Filter";
            }
        }

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(PugClassificationTypes.ClassLiteral), Export]
        internal ClassificationTypeDefinition ClassLiteralClassificationType
        {
            get;
            set;
        }

        [Export(typeof(EditorFormatDefinition))]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = PugClassificationTypes.ClassLiteral)]
        [Name("PugClassLiteralFormatDefinition")]
        [Order]
        internal sealed class ClassLiteralClassificationFormat : ClassificationFormatDefinition
        {
            internal ClassLiteralClassificationFormat()
            {
                this.ForegroundColor = new Color() { R = 255, G = 128, B = 0 };
                this.DisplayName = "Pug Class Literal";
            }
        }

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(PugClassificationTypes.IdLiteral), Export]
        internal ClassificationTypeDefinition IdLiteralClassificationType
        {
            get;
            set;
        }

        [Export(typeof(EditorFormatDefinition))]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = PugClassificationTypes.IdLiteral)]
        [Name("PugIdLiteralFormatDefinition")]
        [Order]
        internal sealed class IdLiteralClassificationFormat : ClassificationFormatDefinition
        {
            internal IdLiteralClassificationFormat()
            {
                this.ForegroundColor = Colors.Maroon;
                this.DisplayName = "Pug Id Literal";
            }
        }

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(PugClassificationTypes.Variable), Export]
        internal ClassificationTypeDefinition VariableClassificationType
        {
            get;
            set;
        }

        [Export(typeof(EditorFormatDefinition))]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = PugClassificationTypes.Variable)]
        [Name("PugVariableFormatDefinition")]
        [Order]
        internal sealed class VariableClassificationFormat : ClassificationFormatDefinition
        {
            internal VariableClassificationFormat()
            {
                this.ForegroundColor = Colors.Blue;
                this.DisplayName = "Pug Variable";
            }
        }
    }
}
