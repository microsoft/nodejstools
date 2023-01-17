// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using Microsoft.NodejsTools.Extras;


namespace Microsoft.NodejsTools.Jade
{
    /// <summary>
    /// JScript Classification definitions
    /// </summary>
    internal class JadeClassificationDefinitions
    {
        #region JadeKeyword
        [Export(typeof(ClassificationTypeDefinition))]
        [Name(JadeClassificationTypes.Keyword), Export]
        internal ClassificationTypeDefinition KeywordClassificationType
        {
            get;
            set;
        }

        [Export(typeof(EditorFormatDefinition))]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = JadeClassificationTypes.Keyword)]
        [Name("JadeKeywordFormatDefinition")]
        [Order]
        internal sealed class KeywordClassificationFormat : ClassificationFormatDefinition
        {
            internal KeywordClassificationFormat()
            {
                this.ForegroundColor = Colors.OrangeRed;
                this.DisplayName = Resources.JadeKeyword;
            }
        }
        #endregion

        #region JadeFilter
        [Export(typeof(ClassificationTypeDefinition))]
        [Name(JadeClassificationTypes.Filter), Export]
        internal ClassificationTypeDefinition FilterClassificationType
        {
            get;
            set;
        }

        [Export(typeof(EditorFormatDefinition))]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = JadeClassificationTypes.Filter)]
        [Name("JadeFilterFormatDefinition")]
        [Order]
        internal sealed class FilterClassificationFormat : ClassificationFormatDefinition
        {
            internal FilterClassificationFormat()
            {
                this.ForegroundColor = Colors.DarkRed;
                this.DisplayName = Resources.JadeFilter ;
            }
        }
        #endregion

        #region JadeClassLiteral

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(JadeClassificationTypes.ClassLiteral), Export]
        internal ClassificationTypeDefinition ClassLiteralClassificationType
        {
            get;
            set;
        }

        [Export(typeof(EditorFormatDefinition))]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = JadeClassificationTypes.ClassLiteral)]
        [Name("JadeClassLiteralFormatDefinition")]
        [Order]
        internal sealed class ClassLiteralClassificationFormat : ClassificationFormatDefinition
        {
            internal ClassLiteralClassificationFormat()
            {
                this.ForegroundColor = new Color() { R = 255, G = 128, B = 0 };
                this.DisplayName = Resources.JadeClassLiteral;
            }
        }

        #endregion

        #region JadeIdLiteral

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(JadeClassificationTypes.IdLiteral), Export]
        internal ClassificationTypeDefinition IdLiteralClassificationType
        {
            get;
            set;
        }

        [Export(typeof(EditorFormatDefinition))]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = JadeClassificationTypes.IdLiteral)]
        [Name("JadeIdLiteralFormatDefinition")]
        [Order]
        internal sealed class IdLiteralClassificationFormat : ClassificationFormatDefinition
        {
            internal IdLiteralClassificationFormat()
            {
                this.ForegroundColor = Colors.Maroon;
                this.DisplayName = Resources.JadeIdLiteral;
            }
        }

        #endregion

        #region JadeVariable
        [Export(typeof(ClassificationTypeDefinition))]
        [Name(JadeClassificationTypes.Variable), Export]
        internal ClassificationTypeDefinition VariableClassificationType
        {
            get;
            set;
        }

        [Export(typeof(EditorFormatDefinition))]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = JadeClassificationTypes.Variable)]
        [Name("JadeVariableFormatDefinition")]
        [Order]
        internal sealed class VariableClassificationFormat : ClassificationFormatDefinition
        {
            internal VariableClassificationFormat()
            {
                this.ForegroundColor = Colors.Blue;
                this.DisplayName = Resources.JadeVariable;
            }
        }
        #endregion
    }
}
