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

using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text;

namespace TestUtilities.Mocks {
    public class MockTextCaret : ITextCaret {
        private SnapshotPoint _position;
        private readonly MockTextView _view;

        public MockTextCaret(MockTextView view) {
            _view = view;
        }

        public double Bottom {
            get { throw new System.NotImplementedException(); }
        }

        public Microsoft.VisualStudio.Text.Formatting.ITextViewLine ContainingTextViewLine {
            get { throw new System.NotImplementedException(); }
        }

        public void EnsureVisible() {
            throw new System.NotImplementedException();
        }

        public double Height {
            get { throw new System.NotImplementedException(); }
        }

        public bool InVirtualSpace {
            get { throw new System.NotImplementedException(); }
        }

        public bool IsHidden {
            get {
                throw new System.NotImplementedException();
            }
            set {
                throw new System.NotImplementedException();
            }
        }

        public double Left {
            get { throw new System.NotImplementedException(); }
        }

        public CaretPosition MoveTo(Microsoft.VisualStudio.Text.VirtualSnapshotPoint bufferPosition, Microsoft.VisualStudio.Text.PositionAffinity caretAffinity, bool captureHorizontalPosition) {
            throw new System.NotImplementedException();
        }

        public CaretPosition MoveTo(Microsoft.VisualStudio.Text.VirtualSnapshotPoint bufferPosition, Microsoft.VisualStudio.Text.PositionAffinity caretAffinity) {
            throw new System.NotImplementedException();
        }

        public CaretPosition MoveTo(Microsoft.VisualStudio.Text.VirtualSnapshotPoint bufferPosition) {
            throw new System.NotImplementedException();
        }

        public CaretPosition MoveTo(Microsoft.VisualStudio.Text.SnapshotPoint bufferPosition, Microsoft.VisualStudio.Text.PositionAffinity caretAffinity, bool captureHorizontalPosition) {
            throw new System.NotImplementedException();
        }

        public CaretPosition MoveTo(Microsoft.VisualStudio.Text.SnapshotPoint bufferPosition, Microsoft.VisualStudio.Text.PositionAffinity caretAffinity) {
            throw new System.NotImplementedException();
        }

        public CaretPosition MoveTo(Microsoft.VisualStudio.Text.SnapshotPoint bufferPosition) {
            _view.Selection.Clear();
            _position = bufferPosition;
            return Position;
        }

        public CaretPosition MoveTo(Microsoft.VisualStudio.Text.Formatting.ITextViewLine textLine) {
            throw new System.NotImplementedException();
        }

        public CaretPosition MoveTo(Microsoft.VisualStudio.Text.Formatting.ITextViewLine textLine, double xCoordinate, bool captureHorizontalPosition) {
            throw new System.NotImplementedException();
        }

        public CaretPosition MoveTo(Microsoft.VisualStudio.Text.Formatting.ITextViewLine textLine, double xCoordinate) {
            throw new System.NotImplementedException();
        }

        public CaretPosition MoveToNextCaretPosition() {
            throw new System.NotImplementedException();
        }

        public CaretPosition MoveToPreferredCoordinates() {
            throw new System.NotImplementedException();
        }

        public CaretPosition MoveToPreviousCaretPosition() {
            throw new System.NotImplementedException();
        }

        public bool OverwriteMode {
            get { throw new System.NotImplementedException(); }
        }

        public CaretPosition Position {
            get { return new CaretPosition(
                new VirtualSnapshotPoint(_position), 
                new MockMappingPoint(), 
                PositionAffinity.Predecessor); 
            }
        }

        internal void SetPosition(SnapshotPoint position) {
            _position = position;
        }

        public event System.EventHandler<CaretPositionChangedEventArgs> PositionChanged {
            add {
            }
            remove {
            }
        }

        public double Right {
            get { throw new System.NotImplementedException(); }
        }

        public double Top {
            get { throw new System.NotImplementedException(); }
        }

        public double Width {
            get { throw new System.NotImplementedException(); }
        }
    }
}
