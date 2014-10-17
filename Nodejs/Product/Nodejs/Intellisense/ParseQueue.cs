/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Microsoft.NodejsTools.Analysis;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

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
            TextReader reader = null;
            ITextSnapshot snapshot = GetOpenSnapshot(projEntry);
            IAnalysisCookie cookie = null;
            if (snapshot != null) {
                cookie = new SnapshotCookie(snapshot);
                reader = new SnapshotSpanSourceCodeReader(new SnapshotSpan(snapshot, 0, snapshot.Length));
            }

            EnqueWorker(() => {
                for (int i = 0; i < 10; i++) {
                    try {
                        if (reader == null) {
                            if (!File.Exists(filename)) {
                                break;
                            }

                            cookie = new FileCookie(filename);
                            reader = new StreamReader(
                                new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete)
                            );
                        }

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
