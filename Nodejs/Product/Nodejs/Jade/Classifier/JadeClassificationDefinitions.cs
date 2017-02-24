//*********************************************************//
//    Copyright (c) Microsoft. All rights reserved.
//    
//    Apache 2.0 License
//    
//    You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//    
//    Unless required by applicable law or agreed to in writing, software 
//    distributed under the License is distributed on an "AS IS" BASIS, 
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or 
//    implied. See the License for the specific language governing 
//    permissions and limitations under the License.
//
//*********************************************************//

using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

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
                ForegroundColor = Colors.OrangeRed;
                this.DisplayName = "Jade Keyword";
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
                ForegroundColor = Colors.DarkRed;
                this.DisplayName = "Jade Filter";
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
                ForegroundColor = new Color() { R = 255, G = 128, B = 0 };
                this.DisplayName = "Jade Class Literal";
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
                ForegroundColor = Colors.Maroon;
                this.DisplayName = "Jade Id Literal";
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
                ForegroundColor = Colors.Blue;
                this.DisplayName = "Jade Variable";
            }
        }
        #endregion
    }
}
