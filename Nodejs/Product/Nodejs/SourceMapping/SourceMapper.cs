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
using System.Diagnostics;
using System.IO;
using System.Linq;

using Microsoft.NodejsTools;
using Microsoft.VisualStudioTools;

namespace Microsoft.NodejsTools.SourceMapping {
    internal class SourceMapper {
        private readonly Dictionary<string, ReverseSourceMap> _generatedFileToSourceMap = new Dictionary<string, ReverseSourceMap>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, JavaScriptSourceMapInfo> _originalFileToSourceMap = new Dictionary<string, JavaScriptSourceMapInfo>(StringComparer.OrdinalIgnoreCase);

        private JavaScriptSourceMapInfo TryGetMapInfo(string filename) {
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
                                    // clear all of our cached _generatedFileToSourceMap files...
                                    foreach (var cachedInvalid in _generatedFileToSourceMap.Where(x => x.Value == null).Select(x => x.Key).ToArray()) {
                                        _generatedFileToSourceMap.Remove(cachedInvalid);
                                    }
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
            return mapInfo;
        }
        
        /// <summary>
        /// Given a filename finds the original filename
        /// </summary>
        /// <param name="filename">the mapped filename</param>
        /// <returns>The original filename
        ///     null - if the file is not mapped
        /// </returns>
        internal string MapToOriginal(string filename) {
            JavaScriptSourceMapInfo mapInfo = TryGetMapInfo(filename);
            if (mapInfo != null && mapInfo.Map != null && mapInfo.Map.Sources.Count > 0) {
                return mapInfo.Map.Sources[0];
            }
            return null;
        }

        /// <summary>
        /// Gets a source mapping for the given filename.  Line numbers are zero based.
        /// </summary>
        internal SourceMapInfo MapToOriginal(string filename, int line, int column = 0) {
            JavaScriptSourceMapInfo mapInfo = TryGetMapInfo(filename);
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
            ReverseSourceMap sourceMap = GetReverseSourceMap(requestedFileName);

            if (sourceMap != null) {
                SourceMapInfo result;
                if (sourceMap.Mapping.TryMapPointBack(requestedLineNo, requestedColumnNo, out result)) {
                    lineNo = result.Line;
                    columnNo = result.Column;

                    foreach (var source in sourceMap.Mapping.Sources) {
                        // requestedFilename == projectdir\server.ts
                        // sourceMap.JavaScriptFile == projectdir\out\server.js
                        // source == ..\server.ts
                        //
                        var path = GetFileRelativeToFile(sourceMap.JavaScriptFile, source);
                        if (CommonUtils.IsSamePath(path, requestedFileName)) {
                            fileName = sourceMap.JavaScriptFile;
                            return true;
                        }
                    }

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

        /// <summary>
        /// Get the original file name to map to.
        /// </summary>
        /// <param name="javaScriptFileName">JavaScript compiled file.</param>
        /// <param name="line">Line number</param>
        /// <param name="column">Column number</param>
        internal string GetOriginalFileName(string javaScriptFileName, int? line, int? column) {
            string originalFileName = null;

            if (line != null && column != null) {
                SourceMapInfo tempMapping = this.MapToOriginal(javaScriptFileName, (int)line, (int)column);

                if (tempMapping != null) {
                    originalFileName = tempMapping.FileName;
                }
            }

            return originalFileName ?? this.MapToOriginal(javaScriptFileName);
        }

        private static string GetFileRelativeToFile(string relativeToFile, string newFileName) {
            return Path.Combine(Path.GetDirectoryName(relativeToFile), newFileName.Replace('/', '\\'));
        }

        class ReverseSourceMap {
            public readonly SourceMap Mapping;
            public readonly string JavaScriptFile;

            public ReverseSourceMap(SourceMap mapping, string javaScriptFile) {
                Mapping = mapping;
                JavaScriptFile = javaScriptFile;
            }
        }

        /// <summary>
        /// Given a generated filename gets the source map from the .js file
        /// </summary>
        private ReverseSourceMap GetReverseSourceMap(string fileName) {
            ReverseSourceMap sourceMap;
            if (!_generatedFileToSourceMap.TryGetValue(fileName, out sourceMap)) {
                // See if we are using source maps for this file.
                foreach(var keyValue in _originalFileToSourceMap) {
                    foreach (var source in keyValue.Value.Map.Sources) {
                        var path = GetFileRelativeToFile(keyValue.Key, source);
                        if (CommonUtils.IsSamePath(path, fileName)) {
                            return _generatedFileToSourceMap[fileName] = new ReverseSourceMap(
                                keyValue.Value.Map,
                                keyValue.Key
                            );
                        }
                    }
                }

                // Fallback to TypeScript specific logic...  This might be better for 
                // try and look next to the .js file...
                string extension;
                try {
                    extension = Path.GetExtension(fileName);
                } catch (ArgumentException) {
                    extension = String.Empty;
                }

                if (!string.Equals(extension, NodejsConstants.JavaScriptExtension, StringComparison.OrdinalIgnoreCase)) {
                    string baseFile = fileName.Substring(0, fileName.Length - extension.Length);
                    string jsFile = baseFile + NodejsConstants.JavaScriptExtension;
                    if (File.Exists(jsFile) && File.Exists(jsFile + NodejsConstants.MapExtension)) {
                        // we're using source maps...
                        try {
                            using (StreamReader reader = new StreamReader(baseFile + NodejsConstants.JavaScriptExtension + NodejsConstants.MapExtension)) {
                                _generatedFileToSourceMap[fileName] = sourceMap = new ReverseSourceMap(
                                    new SourceMap(reader),
                                    jsFile
                                );
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