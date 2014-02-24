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
using System.Linq;

namespace Microsoft.NodejsTools.Debugger {
    class SourceMapper {
        private readonly Dictionary<string, SourceMap> _generatedFileToSourceMap = new Dictionary<string, SourceMap>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, JavaScriptSourceMapInfo> _originalFileToSourceMap = new Dictionary<string, JavaScriptSourceMapInfo>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets a source mapping for the given filename.  Line numbers are zero based.
        /// </summary>
        public SourceMapping MapToOriginal(string filename, int line) {
            JavaScriptSourceMapInfo mapInfo;
            if (!_originalFileToSourceMap.TryGetValue(filename, out mapInfo)) {
                if (File.Exists(filename)) {
                    string[] contents = File.ReadAllLines(filename);
                    const string marker = "# sourceMappingURL=";
                    int markerStart;
                    string markerLine = contents.Reverse().FirstOrDefault(x => x.IndexOf(marker, StringComparison.Ordinal) != -1);
                    if (markerLine != null && (markerStart = markerLine.IndexOf(marker, StringComparison.Ordinal)) != -1) {
                        string sourceMapFilename = markerLine.Substring(markerStart + marker.Length).Trim();
                        if (!File.Exists(sourceMapFilename)) {
                            sourceMapFilename = Path.Combine(Path.GetDirectoryName(filename) ?? string.Empty, Path.GetFileName(sourceMapFilename));
                        }

                        if (File.Exists(sourceMapFilename)) {
                            try {
                                var sourceMap = new SourceMap(new StreamReader(sourceMapFilename));
                                _originalFileToSourceMap[filename] = mapInfo = new JavaScriptSourceMapInfo(sourceMap, contents);
                            } catch (InvalidOperationException) {
                                _originalFileToSourceMap[filename] = null;
                            } catch (NotSupportedException) {
                                _originalFileToSourceMap[filename] = null;
                            }
                        }
                    }
                }
            }
            if (mapInfo != null) {
                SourceMapping mapping;
                int column = 0;
                if (line < mapInfo.Lines.Length) {
                    string lineText = mapInfo.Lines[line];
                    // map to the 1st non-whitespace character on the line
                    // This ensures we get the correct line number, mapping to column 0
                    // can give us the previous line.
                    if (!String.IsNullOrWhiteSpace(lineText)) {
                        for (; column < lineText.Length; column++) {
                            if (!Char.IsWhiteSpace(lineText[column])) {
                                break;
                            }
                        }
                    }
                }
                if (mapInfo.Map.TryMapPoint(line, column, out mapping)) {
                    return mapping;
                }
            }
            return null;
        }

        /// <summary>
        /// Maps a line number from the original code to the generated JavaScript.
        /// Line numbers are zero based.
        /// </summary>
        public void MapToJavaScript(string requestedFileName, int requestedLineNo, out string fileName, out int lineNo) {
            fileName = requestedFileName;
            lineNo = requestedLineNo;
            SourceMap sourceMap = GetSourceMap(requestedFileName);

            if (sourceMap != null) {
                SourceMapping result;
                if (sourceMap.TryMapPointBack(requestedLineNo, 0, out result)) {
                    lineNo = result.Line;
                    fileName = Path.Combine(Path.GetDirectoryName(fileName) ?? string.Empty, result.FileName);
                    Debug.WriteLine("Mapped breakpoint from {0} {1} to {2} {3}", requestedFileName, requestedLineNo, fileName, lineNo);
                }
            }
        }

        private SourceMap GetSourceMap(string fileName) {
            SourceMap sourceMap;
            if (!_generatedFileToSourceMap.TryGetValue(fileName, out sourceMap)) {
                // see if we are using source maps for this file.
                if (!String.Equals(Path.GetExtension(fileName), NodejsConstants.FileExtension, StringComparison.OrdinalIgnoreCase)) {
                    string baseFile = fileName.Substring(0, fileName.Length - Path.GetExtension(fileName).Length);
                    if (File.Exists(baseFile + ".js") && File.Exists(baseFile + ".js.map")) {
                        // we're using source maps...
                        try {
                            _generatedFileToSourceMap[fileName] = sourceMap = new SourceMap(new StreamReader(baseFile + ".js.map"));
                        } catch (NotSupportedException) {
                            _generatedFileToSourceMap[fileName] = null;
                        } catch (InvalidOperationException) {
                            _generatedFileToSourceMap[fileName] = null;
                        }
                    } else {
                        _generatedFileToSourceMap[fileName] = null;
                    }
                }
            }
            return sourceMap;
        }
    }
}