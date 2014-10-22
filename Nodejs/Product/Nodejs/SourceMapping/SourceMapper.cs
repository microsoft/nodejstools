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

using Microsoft.NodejsTools;

namespace Microsoft.NodejsTools.SourceMapping {
    internal class SourceMapper {
        private readonly Dictionary<string, SourceMap> _generatedFileToSourceMap = new Dictionary<string, SourceMap>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, JavaScriptSourceMapInfo> _originalFileToSourceMap = new Dictionary<string, JavaScriptSourceMapInfo>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets a source mapping for the given filename.  Line numbers are zero based.
        /// </summary>
        internal SourceMapInfo MapToOriginal(string filename, int line, int column = 0) {
            JavaScriptSourceMapInfo mapInfo;
            if (!_originalFileToSourceMap.TryGetValue(filename, out mapInfo)) {
                if (File.Exists(filename)) {
                    string[] contents = File.ReadAllLines(filename);
                    const string marker = "# sourceMappingURL=";
                    int markerStart;
                    string markerLine = contents.Reverse().FirstOrDefault(x => x.IndexOf(marker, StringComparison.Ordinal) != -1);
                    if (markerLine != null && (markerStart = markerLine.IndexOf(marker, StringComparison.Ordinal)) != -1) {
                        string sourceMapFilename = markerLine.Substring(markerStart + marker.Length).Trim();

                        try {
                            if (!File.Exists(sourceMapFilename)) {
                                sourceMapFilename = Path.Combine(Path.GetDirectoryName(filename) ?? string.Empty, Path.GetFileName(sourceMapFilename));
                            }
                        } catch (ArgumentException) {
                        } catch (PathTooLongException) {
                        }

                        try {
                            if (File.Exists(sourceMapFilename)) {
                                using (StreamReader reader = new StreamReader(sourceMapFilename)) {
                                    var sourceMap = new SourceMap(reader);
                                    _originalFileToSourceMap[filename] = mapInfo = new JavaScriptSourceMapInfo(sourceMap, contents);
                                }
                            }
                        } catch (ArgumentException) {
                        } catch (PathTooLongException) {
                        } catch (NotSupportedException) {
                        } catch (InvalidOperationException) {
                        } 
                    }
                }
            }
            if (mapInfo != null) {
                SourceMapInfo mapping;
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
        internal bool MapToJavaScript(string requestedFileName, int requestedLineNo, int requestedColumnNo, out string fileName, out int lineNo, out int columnNo) {
            fileName = requestedFileName;
            lineNo = requestedLineNo;
            columnNo = requestedColumnNo;
            SourceMap sourceMap = GetSourceMap(requestedFileName);

            if (sourceMap != null) {
                SourceMapInfo result;
                if (sourceMap.TryMapPointBack(requestedLineNo, requestedColumnNo, out result)) {
                    lineNo = result.Line;
                    columnNo = result.Column;
                    try {
                        fileName = Path.Combine(Path.GetDirectoryName(fileName) ?? string.Empty, result.FileName);
                    } catch (ArgumentException) {
                    } catch (PathTooLongException) {
                    }
                    Debug.WriteLine("Mapped breakpoint from {0} {1} to {2} {3}", requestedFileName, requestedLineNo, fileName, lineNo);
                }

                return true;
            }

            return false;
        }

        private SourceMap GetSourceMap(string fileName) {
            SourceMap sourceMap;
            if (!_generatedFileToSourceMap.TryGetValue(fileName, out sourceMap)) {
                // See if we are using source maps for this file.

                string extension;
                try {
                    extension = Path.GetExtension(fileName);
                } catch (ArgumentException) {
                    extension = String.Empty;
                }

                if (!string.Equals(extension, NodejsConstants.JavaScriptExtension, StringComparison.OrdinalIgnoreCase)) {
                    string baseFile = fileName.Substring(0, fileName.Length - extension.Length);
                    if (File.Exists(baseFile + NodejsConstants.JavaScriptExtension) && File.Exists(baseFile + NodejsConstants.JavaScriptExtension + NodejsConstants.MapExtension)) {
                        // we're using source maps...
                        try {
                            using (StreamReader reader = new StreamReader(baseFile + NodejsConstants.JavaScriptExtension + NodejsConstants.MapExtension)) {
                                _generatedFileToSourceMap[fileName] = sourceMap = new SourceMap(reader);
                            }
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

        private static char[] InvalidPathChars = Path.GetInvalidPathChars();

        internal static FunctionInformation MaybeMap(FunctionInformation funcInfo) {
            return MaybeMap(funcInfo, null);
        }

        internal static FunctionInformation MaybeMap(FunctionInformation funcInfo, Dictionary<string, SourceMap> sourceMaps) {
            if (funcInfo.Filename != null &&
                funcInfo.Filename.IndexOfAny(InvalidPathChars) == -1 &&
                File.Exists(funcInfo.Filename) &&
                File.Exists(funcInfo.Filename + ".map") &&
                funcInfo.LineNumber != null) {
                SourceMap map = null;
                if (sourceMaps == null || !sourceMaps.TryGetValue(funcInfo.Filename, out map)) {
                    try {
                        using (StreamReader reader = new StreamReader(funcInfo.Filename + ".map")) {
                            map = new SourceMap(reader);
                        }
                    } catch (InvalidOperationException) {
                    } catch (FileNotFoundException) {
                    } catch (DirectoryNotFoundException) {
                    } catch (IOException) {
                    }

                    if (sourceMaps != null && map != null) {
                        sourceMaps[funcInfo.Filename] = map;
                    }
                }

                SourceMapInfo mapping;
                // We explicitly don't convert our 1 based line numbers into 0 based
                // line numbers here.  V8 is giving us the starting line of the function,
                // and TypeScript doesn't give the right name for the declaring name.
                // But TypeScript also happens to always emit a newline after the {
                // for a function definition, and we're always mapping line numbers from
                // function definitions, so mapping line + 1 happens to work out for
                // the time being.
                if (map != null && map.TryMapLine(funcInfo.LineNumber.Value, out mapping)) {
                    string filename = mapping.FileName;
                    if (filename != null && !Path.IsPathRooted(filename)) {
                        filename = Path.Combine(Path.GetDirectoryName(funcInfo.Filename), filename);
                    }

                    return new FunctionInformation(
                        funcInfo.Namespace,
                        mapping.Name ?? funcInfo.Function,
                        mapping.Line + 1,
                        filename ?? funcInfo.Filename,
                        funcInfo.IsRecompilation
                    );
                }
            }
            return funcInfo;
        }
    }
}