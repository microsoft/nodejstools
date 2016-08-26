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
using System.IO;
using System.Threading;
using Microsoft.NodejsTools.Analysis;
using Microsoft.VisualStudio.Text;

namespace Microsoft.NodejsTools.Intellisense {
    sealed partial class VsProjectAnalyzer {
        internal int _analysisPending;

        /// <summary>
        /// Parses the specified text buffer.  Continues to monitor the parsed buffer and updates
        /// the parse tree asynchronously as the buffer changes.
        /// </summary>
        /// <param name="buffer"></param>
        private BufferParser EnqueueBuffer(IProjectEntry projEntry, ITextBuffer buffer) {
            // only attach one parser to each buffer, we can get multiple enqueue's
            // for example if a document is already open when loading a project.
            BufferParser bufferParser;
            if (!buffer.Properties.TryGetProperty<BufferParser>(typeof(BufferParser), out bufferParser)) {
                bufferParser = new BufferParser(projEntry, this, buffer);

                var curSnapshot = buffer.CurrentSnapshot;
                bufferParser.EnqueingEntry();
                EnqueWorker(() => {
                    ParseBuffers(bufferParser, curSnapshot);
                });
            } else {
                bufferParser.AttachedViews++;
            }

            return bufferParser;
        }

        /// <summary>
        /// Parses the specified file on disk.
        /// </summary>
        /// <param name="filename"></param>
        private void EnqueueFile(IProjectEntry projEntry, string filename) {
            // get the current snapshot from the UI thread

            EnqueWorker(() => {
                for (int i = 0; i < 10; i++) {
                    try {
                        if (!File.Exists(filename)) {
                            break;
                        }

                        var cookie = new FileCookie(filename);
                        var reader = new StreamReader(
                            new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete)
                        );

                        using (reader) {
                            ParseFile(projEntry, filename, reader, cookie);
                        }
                        return;
                    } catch (IOException) {
                        // file being copied, try again...
                        Thread.Sleep(100);
                    } catch (UnauthorizedAccessException) {
                        // file is inaccessible, try again...
                        Thread.Sleep(100);
                    }
                }

                IJsProjectEntry pyEntry = projEntry as IJsProjectEntry;
                if (pyEntry != null) {
                    // failed to parse, keep the UpdateTree calls balanced
                    pyEntry.UpdateTree(null, null);
                }
            });
        }

        private void EnqueWorker(Action parser) {
            Interlocked.Increment(ref _analysisPending);

            ThreadPool.QueueUserWorkItem(
                dummy => {
                    try {
                        parser();
                    } finally {
                        Interlocked.Decrement(ref _analysisPending);
                    }
                }
            );
        }

        private bool IsParsing {
            get {
                return _analysisPending > 0;
            }
        }

        private int ParsePending {
            get {
                return _analysisPending;
            }
        }
    }
}
