using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;

namespace TypeScriptSourceMapReader
{
    /// <summary>
    /// Helper class to read the source map and get the decoded source map
    /// It also has some apis to test if the url is js url and get the sourceMap url
    /// </summary>
    public class SourceMapReader : SourceMapTextReader
    {
        #region ApiLoadSourceMapping
        /// <summary>
        /// Gets the decoded source map for the given script document
        /// </summary>
        /// <param name="scriptFilePathOrUrl">FilePath or Url of the js script file</param>
        /// <param name="sourceMapUrl">url of the source map</param>
        /// <returns>DecodedSourceMap if the mapping present otherwise null, throws SourceMapReaderException if read fails</returns>
        public DecodedSourceMap LoadSourceMap(string scriptFilePathOrUrl, string sourceMapUrl)
        {
            // Read source map
            Uri sourceMapUri;
            var encodedSourceMap = this.ReadEncodedSourceMap(scriptFilePathOrUrl, sourceMapUrl, out sourceMapUri);
            return this.DecodeSourceMap(encodedSourceMap, scriptFilePathOrUrl, sourceMapUri);
        }

        /// <summary>
        /// Gets the decoded source map for the given script document's source map data
        /// </summary>
        /// <param name="sourceMapText">source map text - to decode</param>
        /// <param name="scriptFilePathOrUrl">FilePath or Url of the js script file</param>
        /// <param name="sourceMapUri">Uri of the source map (that was used to read the source map)</param>
        /// <returns>DecodedSourceMap if the mapping present otherwise null, throws SourceMapReaderException if read fails</returns>
        public DecodedSourceMap LoadSourceMapFromSourceMapText(string sourceMapText, string scriptFilePathOrUrl, Uri sourceMapUri)
        {
            // Convert the sourceMapText into EncodedSourceMap json format
            EncodedSourceMap encodedSourceMap;
            if (sourceMapText == null)
            {
                // Read the source Map from sourceMapUri
                encodedSourceMap = this.ReadEncodedSourceMap(sourceMapUri);
            }
            else
            {
                // Read source map using sourceMapText
                StringReader stringReader = null;
                try
                {
                    stringReader = new StringReader(sourceMapText);
                    encodedSourceMap = this.ReadEncodedSourceMap(stringReader);
                }
                catch (Exception e)
                {
                    throw new SourceMapReadFailedException(sourceMapUri.IsFile ? sourceMapUri.LocalPath : sourceMapUri.AbsoluteUri, e);
                }
                finally
                {
                    if (stringReader != null)
                    {
                        stringReader.Close();
                    }
                }
            }

            // Decode the source map for use
            return DecodeSourceMap(encodedSourceMap, scriptFilePathOrUrl, sourceMapUri);
        }
        #endregion

        #region Read encoded source map
        /// <summary>
        /// Gets the encoded source map (json) for the given script document
        /// </summary>
        /// <param name="scriptFilePathOrUrl">FilePath or Url of the js script file</param>
        /// <param name="sourceMapUrl">url of the source map</param>
        /// <param name="sourceMapUri">Returns Uri used to read the source map from</param>
        /// <returns>EncodedSourceMap if the mapping present otherwise null, throws SourceMapReaderException if read fails</returns>
        private EncodedSourceMap ReadEncodedSourceMap(string scriptFilePathOrUrl, string sourceMapUrl, out Uri sourceMapUri)
        {
            EncodedSourceMap encodedSourceMap;
            sourceMapUri = this.ReadSourceMap(scriptFilePathOrUrl, sourceMapUrl, out encodedSourceMap, textReader => this.ReadEncodedSourceMap(textReader));
            return encodedSourceMap;
        }

        /// <summary>
        /// Gets the encoded source map (json) for the given sourceMapUri
        /// </summary>
        /// <param name="sourceMapUri">Uri used to read the source map from</param>
        /// <returns>EncodedSourceMap if the mapping present otherwise null, throws SourceMapReaderException if read fails</returns>
        private EncodedSourceMap ReadEncodedSourceMap(Uri sourceMapUri)
        {
            return this.ReadMapFile(sourceMapUri, textReader => this.ReadEncodedSourceMap(textReader));
        }

        /// <summary>
        /// Gets the encoded source map from the sourceMapText reader
        /// Note that this method doesnt catch any exceptions to convert into SourceMapRead failed exception
        /// </summary>
        /// <param name="sourceMapTextReader"></param>
        /// <returns></returns>
        private EncodedSourceMap ReadEncodedSourceMap(TextReader sourceMapTextReader)
        {
            JsonTextReader jsonReader = null;
            try
            {
                jsonReader = new JsonTextReader(sourceMapTextReader);
                JsonSerializer serializer = new JsonSerializer();
                return (EncodedSourceMap)serializer.Deserialize(jsonReader, typeof(EncodedSourceMap));
            }
            finally
            {
                if (jsonReader != null)
                {
                    jsonReader.Close();
                }
            }
        }
        #endregion

        #region DecodeSourceMap
        /// <summary>
        /// Decodes the source map json information into the DecodedSourceMap for easy consumption
        /// </summary>
        /// <param name="encodedSourceMap">Encoded sourcemap to decode</param>
        /// <param name="jsFileUrl">jsScriptUrl corresponding to this source map</param>
        /// <param name="sourceMapUri">uri for the source map</param>
        /// <returns>decoded Source map for the script document</returns>
        private DecodedSourceMap DecodeSourceMap(EncodedSourceMap encodedSourceMap, string jsFileUrl, Uri sourceMapUri)
        {
            // We support version of source maps only containing these options
            if (encodedSourceMap == null || encodedSourceMap.mappings == null || encodedSourceMap.names == null || encodedSourceMap.sourceRoot == null || encodedSourceMap.sources == null)
            {
                throw new UnsupportedFormatSourceMapException();
            }

            var decodedSourceMap = new DecodedSourceMap(jsFileUrl, encodedSourceMap.names);

            string mappingStr = encodedSourceMap.mappings;
            int mappingStrLen = encodedSourceMap.mappings.Length;

            int jsLine = 1;
            int jsColumn = 1;

            int tsLine = 1;
            int tsColumn = 1;

            int tsSourceIndex = -1;

            int nameIndex = 0;
            Debug.WriteLine("JSFile: url=" + jsFileUrl);
            Debug.WriteLine("SourceMapUrl: url=" + sourceMapUri.AbsoluteUri);

            for (int i = 0; i < mappingStrLen; )
            {
                if (mappingStr[i] == ';')
                {
                    // New line
                    jsLine++;
                    jsColumn = 1;
                    i++;
                    continue;
                }

                if (mappingStr[i] == ',')
                {
                    // Next entry is on same line - no action needed
                    i++;
                    continue;
                }

                // Read the current span
                // 1. Column offset from prev read jsColumn
                jsColumn = jsColumn + Base64VLQFormat.decode(mappingStr, ref i);
                if (jsColumn < 1)
                {
                    // Incorrect start column dont support this map
                    throw new ErrorDecodingSourcemapException("Invalid jsEntry start column found");
                }

                if (IsSourceMappingSegmentEnd(i, mappingStrLen, mappingStr)) {
                    // Dont support reading mappings that dont have information about original source and its line numbers
                    throw new UnsupportedFormatSourceMapException();
                }

                // 2. Relative sourceIndex 
                var deltaSourceIndex = Base64VLQFormat.decode(mappingStr, ref i);
                if (deltaSourceIndex != 0 || tsSourceIndex == -1)
                {
                    // We should create entry for this jsStartLine and Column and new sourceFile association
                    if (tsSourceIndex == -1)
                    {
                        tsSourceIndex = tsSourceIndex + deltaSourceIndex + 1;
                    }
                    else
                    {
                        tsSourceIndex = tsSourceIndex + deltaSourceIndex;
                    }

                    // Incorrect source index, invalid source map do not support this map
                    if (tsSourceIndex < 0 || tsSourceIndex >= encodedSourceMap.sources.Length)
                    {
                        throw new ErrorDecodingSourcemapException("Invalid source index");
                    }

                    // Check if the tsFileName after prepending sourceRoot is rooted name
                    var tsFileName = encodedSourceMap.sourceRoot + encodedSourceMap.sources[tsSourceIndex];
                    Uri tsUri = null;
                    try
                    {
                        // If this succeeds then it is rooted name
                        tsUri = new Uri(tsFileName);
                    }
                    catch (UriFormatException)
                    {
                    }

                    // Get the relative ts Uri
                    if (tsUri == null)
                    {
                        // Since we couldnt create tsFile uri it is relative to the mapFile
                        try
                        {
                            tsUri = new Uri(sourceMapUri, tsFileName);
                        }
                        catch
                        {
                            // It is error if we cannot deduce the ts file name we would report the exception
                            throw new ErrorDecodingSourcemapException("Invalid tsSource name: tsSource" + encodedSourceMap.sources[tsSourceIndex] + "sourceRoot: " + encodedSourceMap.sourceRoot);
                        }
                    }

                    // Get the Url and file path
                    var tsUrl = tsUri.AbsoluteUri;
                    var tsFilePath = tsUri.IsFile ? tsUri.LocalPath : "";

                    decodedSourceMap.AddSourceMapSourceInfo(tsUrl, tsFilePath);
                    Debug.WriteLine("tsFile: url=" + tsUrl + " fileName=" + tsFilePath);
                }
                Debug.Assert(i < mappingStrLen);

                // 3. Relative sourceLine 0 based
                tsLine = tsLine + Base64VLQFormat.decode(mappingStr, ref i);
                Debug.Assert(i < mappingStrLen);
                if (tsLine < 1)
                {
                    // Incorrect start column dont support this map
                    throw new ErrorDecodingSourcemapException("Invalid tsEntry start line found");
                }

                // 4. Relative sourceColumn 0 based 
                tsColumn = tsColumn + Base64VLQFormat.decode(mappingStr, ref i);
                Debug.Assert(i <= mappingStrLen);
                if (tsColumn < 1)
                {
                    // Incorrect start column dont support this map
                    throw new ErrorDecodingSourcemapException("Invalid tsEntry start column found");
                }

                // 5. Check if there is name:
                int recordNameIndex = -1;
                if (!IsSourceMappingSegmentEnd(i, mappingStrLen, mappingStr))
                {
                    nameIndex = nameIndex + Base64VLQFormat.decode(mappingStr, ref i);
                    recordNameIndex = nameIndex;
                    Debug.Assert(i <= mappingStrLen);

                    if (nameIndex < 0 || nameIndex >= encodedSourceMap.names.Length)
                    {
                        // Invalid name index found - do not support this map
                        throw new ErrorDecodingSourcemapException("Invalid name index for the source map entry");
                    }
                }

                var jsEntry = new SourceMapSpan(jsLine, jsColumn);
                var tsEntry = new SourceMapSpan(tsLine, tsColumn);

                var myString = "Emitted (" + jsEntry.StartLine + ", " + jsEntry.StartColumn + ") ";
                myString = myString + "source (" + tsEntry.StartLine + ", " + tsEntry.StartColumn + ") ";
                if (recordNameIndex >= 0)
                {
                    myString = myString + "name (" + encodedSourceMap.names[recordNameIndex] + ") ";
                }
                Debug.WriteLine(myString);

                decodedSourceMap.AddSourceMapSpanMapping(jsEntry, tsEntry, recordNameIndex);
            }

            return decodedSourceMap;
        }

        /// <summary>
        /// Returns if the current index is the end of source code mapping segment end
        /// </summary>
        /// <param name="currentIndex">current index in the mappings string</param>
        /// <param name="mappingsStringLength">length of mappings string</param>
        /// <param name="mappingsString">mappings string</param>
        /// <returns>true if current index in the mappingsString indicates end of mapping entry segment</returns>
        private bool IsSourceMappingSegmentEnd(int currentIndex, int mappingsStringLength, string mappingsString)
        {
            if (currentIndex == mappingsStringLength)
            {
                return true;
            }

            if (mappingsString[currentIndex] == ',')
            {
                return true;
            }

            if (mappingsString[currentIndex] == ';')
            {
                return true;
            }

            return false;
        }
        #endregion
    }
}