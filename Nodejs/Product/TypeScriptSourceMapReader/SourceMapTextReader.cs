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
using System.Diagnostics;
using System.IO;

namespace Microsoft.NodejsTools.TypeScriptSourceMapReader {
    /// <summary>
    /// Basic source map text reader class
    /// </summary>
    public class SourceMapTextReader {
        #region ApiGetSourceMapUri
        /// <summary>
        /// Get the source map uri from raw sourceMapUrl read from the js contents of the scriptFilePathOrUrl
        /// </summary>
        /// <param name="scriptFilePathOrUrl">FilePath or Url of the js script file</param>
        /// <param name="sourceMapUrl">url of the source map</param>
        /// <returns>source map uri if the source map url is valid and resolved</returns>
        public Uri GetSourceMapUri(string scriptFilePathOrUrl, string sourceMapUrl) {
            Uri sourceMapUri = null;
            if (!string.IsNullOrEmpty(sourceMapUrl)) {
                // try getting uri for mapFile
                try {
                    // Rooted map file name
                    sourceMapUri = new Uri(sourceMapUrl, UriKind.Absolute);
                } catch (UriFormatException) {
                    // Source map was relative path so couldnt create Uri
                }

                // This is relative path and hence we couldnt just create the uri
                if (sourceMapUri == null) {
                    Uri scriptUri = null;
                    try {
                        // We couldnt create url from the sourceMapUrl so it is a relative path,
                        // create a new Uri to serve as the root.
                        scriptUri = new Uri(scriptFilePathOrUrl, UriKind.Absolute);
                    } catch (UriFormatException e) {
                        // If we couldnt create the script Uri and the sourcemap path wasnt relative
                        // we cant work with this sourcemap (eg. cscript opening tc.js with 
                        // mapPath as tc.js.map instead of fully qualified path
                        throw new InvalidSourceMapUrlException(sourceMapUrl, scriptFilePathOrUrl, e);
                    }

                    try {
                        sourceMapUri = new Uri(scriptUri, new Uri(sourceMapUrl, UriKind.Relative));
                    } catch (Exception e) {
                        // If this fails, this is invalid source map url
                        throw new InvalidSourceMapUrlException(sourceMapUrl, scriptFilePathOrUrl, e);
                    }
                }
            }

            return sourceMapUri;
        }
        #endregion

        #region ApiReadSourceMapText
        /// <summary>
        /// Gets the source map text for the given script document
        /// </summary>
        /// <param name="scriptFilePathOrUrl">FilePath or Url of the js script file</param>
        /// <param name="sourceMapUrl">url of the source map</param>
        /// <param name="sourceMapText">sourceMap contents if the mapping present otherwise null, throws SourceMapReaderException if read fails</param>
        /// <returns>Returns Uri used to read the source map from</returns>
        public Uri ReadSourceMapText(string scriptFilePathOrUrl, string sourceMapUrl, out string sourceMapText) {
            return this.ReadSourceMap(scriptFilePathOrUrl, sourceMapUrl, out sourceMapText, textReader => textReader.ReadToEnd());
        }
        #endregion

        #region Helper to read the sourcemap in the format of the text reader delegate returns
        /// <summary>
        /// Delegate to read the contents of type T from text reader
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="textReader"></param>
        /// <returns></returns>
        protected delegate T TextReadDelegate<T>(TextReader textReader);

        /// <summary>
        /// Gets the source map for the given script document
        /// </summary>
        /// <param name="scriptFilePathOrUrl">FilePath or Url of the js script file</param>
        /// <param name="sourceMapUrl">url of the source map</param>
        /// <param name="textReadDelegate">Delegate to read the sourceMapfrom the steam in desired format</param>
        /// <param name="sourceMapText">sourceMap contents if the mapping present otherwise null, throws SourceMapReaderException if read fails</param>
        /// <returns>Returns Uri used to read the source map from</returns>
        protected Uri ReadSourceMap<T>(string scriptFilePathOrUrl, string sourceMapUrl, out T sourceMapText, TextReadDelegate<T> textReadDelegate) {
            var sourceMapUri = this.GetSourceMapUri(scriptFilePathOrUrl, sourceMapUrl);
            if (sourceMapUri != null) {
                // Read map files
                sourceMapText = this.ReadMapFile(sourceMapUri, textReadDelegate);
            } else {
                sourceMapText = default(T);
            }
            return sourceMapUri;
        }
        #endregion

        #region ReadEncodedSourceMapFromMapFile
        /// <summary>
        /// Read the map file from the disk and returns it
        /// </summary>
        /// <param name="sourceMapUri">Uri correspoding to the map file to read from</param>
        /// <returns>SourceMap for the mapfile</returns>
        protected T ReadMapFile<T>(Uri sourceMapUri, TextReadDelegate<T> textReadDelegate) {
            Debug.WriteLine("Reading JSMapFile: " + sourceMapUri.AbsoluteUri);
            try {
                if (sourceMapUri.IsFile) {
                    using (var streamReader = new StreamReader(sourceMapUri.LocalPath)) {
                        return textReadDelegate(streamReader);
                    }
                } else {
                    using (var webResponse = SourceMapUrlHelper.GetWebResponse(sourceMapUri))
                    using (var stream = webResponse.GetResponseStream())
                    using (var streamReader = new StreamReader(stream)) {
                        return textReadDelegate(streamReader);
                    }
                }
            } catch (Exception e) {
                throw new SourceMapReadFailedException(sourceMapUri.IsFile ? sourceMapUri.LocalPath : sourceMapUri.AbsoluteUri, e);
            }
        }
        #endregion
    }
}
