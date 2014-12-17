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

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

#if NTVS_FEATURE_INTERACTIVEWINDOW
namespace Microsoft.NodejsTools.Repl {
#else
namespace Microsoft.VisualStudio.Repl {
#endif
    /// <summary>
    /// Provides the classifier for our repl error output buffer.
    /// </summary>
    [Export(typeof(IClassifierProvider)), ContentType(ReplConstants.ReplOutputContentTypeName)]
    class ReplOutputClassifierProvider : IClassifierProvider {
        internal readonly Dictionary<ConsoleColor, IClassificationType> _classTypes = new Dictionary<ConsoleColor, IClassificationType>();

        [ImportingConstructor]
        public ReplOutputClassifierProvider(IClassificationTypeRegistryService classificationService) {
            _classTypes[ConsoleColor.Black] = classificationService.GetClassificationType(InteractiveBlackFormatDefinition.Name);            
            _classTypes[ConsoleColor.DarkBlue] = classificationService.GetClassificationType(InteractiveDarkBlueFormatDefinition.Name);
            _classTypes[ConsoleColor.DarkGreen] = classificationService.GetClassificationType(InteractiveDarkGreenFormatDefinition.Name);
            _classTypes[ConsoleColor.DarkCyan] = classificationService.GetClassificationType(InteractiveDarkCyanFormatDefinition.Name);
            _classTypes[ConsoleColor.DarkRed] = classificationService.GetClassificationType(InteractiveDarkRedFormatDefinition.Name);
            _classTypes[ConsoleColor.DarkMagenta] = classificationService.GetClassificationType(InteractiveDarkMagentaFormatDefinition.Name);
            _classTypes[ConsoleColor.DarkYellow] = classificationService.GetClassificationType(InteractiveDarkYellowFormatDefinition.Name);
            _classTypes[ConsoleColor.Gray] = classificationService.GetClassificationType(InteractiveGrayFormatDefinition.Name);
            _classTypes[ConsoleColor.DarkGray] = classificationService.GetClassificationType(InteractiveDarkGrayFormatDefinition.Name);
            _classTypes[ConsoleColor.Blue] = classificationService.GetClassificationType(InteractiveBlueFormatDefinition.Name);
            _classTypes[ConsoleColor.Green] = classificationService.GetClassificationType(InteractiveGreenFormatDefinition.Name);
            _classTypes[ConsoleColor.Cyan] = classificationService.GetClassificationType(InteractiveCyanFormatDefinition.Name);
            _classTypes[ConsoleColor.Red] = classificationService.GetClassificationType(InteractiveRedFormatDefinition.Name);
            _classTypes[ConsoleColor.Magenta] = classificationService.GetClassificationType(InteractiveMagentaFormatDefinition.Name);
            _classTypes[ConsoleColor.Yellow] = classificationService.GetClassificationType(InteractiveYellowFormatDefinition.Name);
            _classTypes[ConsoleColor.White] = classificationService.GetClassificationType(InteractiveWhiteFormatDefinition.Name);
        }

        #region IClassifierProvider Members

        public IClassifier GetClassifier(ITextBuffer textBuffer) {
            return new ReplOutputClassifier(this, textBuffer);
        }

        #endregion
    }
}
