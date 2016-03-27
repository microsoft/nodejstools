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
using System.Globalization;

namespace TypeScriptSourceMapReader
{
    /// <summary>
    /// Type of SourceMapException
    /// </summary>
    public enum SourceMapReaderExceptionKind
    {
        InvalidSourceMapUrlException,
        SourceMapReadFailedException,
        UnsupportedFormatSourceMapException,
        ErrorDecodingSourcemapException
    }

    /// <summary>
    /// Exception thrown by SourceMapReader
    /// </summary>
    public abstract class SourceMapReaderException : Exception
    {
        /// <summary>
        /// Kind of the exception
        /// </summary>
        public virtual SourceMapReaderExceptionKind ExceptionKind
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// First parameter that can be used to store data
        /// </summary>
        public string Parameter1;

        /// <summary>
        /// Second parameter that can be used to store data
        /// </summary>
        public string Parameter2;

        internal SourceMapReaderException(string parameter1 = null, string parameter2 = null, Exception innerException = null) :
            base(null, innerException)
        {
            this.Parameter1 = parameter1;
            this.Parameter2 = parameter2;
            if (innerException != null)
            {
                this.HResult = innerException.HResult;
            }
        }
    }

    /// <summary>
    /// Invalid source map url specified exception
    /// exceptionKind = InvalidSourceMapUrlException
    /// Parameter1 = sourceMapUrl
    /// Parameter2 = scriptUrl
    /// </summary>
    public class InvalidSourceMapUrlException : SourceMapReaderException
    {
        public override SourceMapReaderExceptionKind ExceptionKind
        {
            get
            {
                return SourceMapReaderExceptionKind.InvalidSourceMapUrlException; 
            }
        }

        internal InvalidSourceMapUrlException(string sourceMapUrl, string scriptUrl, Exception innerException) :
            base(sourceMapUrl, scriptUrl, innerException)
        {
        }
    }

    /// <summary>
    /// Source map read failed exception
    /// exceptionKind = SourceMapReadFailedException
    /// Parameter1 = mapFileName
    /// Parameter2 = this.InnerException.Message = Reason for read fail
    /// </summary>
    public class SourceMapReadFailedException : SourceMapReaderException
    {
        public override SourceMapReaderExceptionKind ExceptionKind
        {
            get
            {
                return SourceMapReaderExceptionKind.SourceMapReadFailedException;
            }
        }

        internal SourceMapReadFailedException(string mapFileName, Exception innerException) :
            base(mapFileName, innerException.Message, innerException)
        {
        }
    }

    /// <summary>
    /// Unsupported source map format
    /// exceptionKind = UnsupportedFormatSourceMapException
    /// Parameter1 = null
    /// Parameter2 = null
    /// </summary>
    public class UnsupportedFormatSourceMapException : SourceMapReaderException
    {
        public override SourceMapReaderExceptionKind ExceptionKind
        {
            get
            {
                return SourceMapReaderExceptionKind.UnsupportedFormatSourceMapException;
            }
        }

        internal UnsupportedFormatSourceMapException(string debugPrintMessage = null)
        {
            if (debugPrintMessage != null)
            {
                Debug.WriteLine(debugPrintMessage);
            }
        }
    }

    /// <summary>
    /// Error decoding the source map contents
    /// exceptionKind = ErrorDecodingSourcemapException
    /// Parameter1 = null
    /// Parameter2 = null
    /// </summary>
    public class ErrorDecodingSourcemapException : SourceMapReaderException
    {
        public override SourceMapReaderExceptionKind ExceptionKind
        {
            get
            {
                return SourceMapReaderExceptionKind.ErrorDecodingSourcemapException;
            }
        }

        internal ErrorDecodingSourcemapException(string debugPrintMessage)
        {
            Debug.WriteLine(debugPrintMessage);
        }
    }
}
