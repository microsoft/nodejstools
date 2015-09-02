using Microsoft.VisualStudio.Debugger;
using Microsoft.VisualStudio.Debugger.Script;
using Microsoft.VisualStudio.Debugger.Symbols;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using TypeScriptSourceMapReader;

namespace Microsoft.NodejsTools.Debugger {
    /// <summary>
    /// This is class with helper method to set and get the symbol file id information as script document
    /// so that it can be retrieved later to use the script document that just got loaded
    /// </summary>
    internal static class TypeScriptSymbolFileId
    {
        /// <summary>
        /// Converts the unique script document id, source map text, script filepath/url and source map uri
        /// into read only collection so that it can be stored in symbol file id
        /// </summary>
        /// <param name="scriptDocument"></param>
        /// <param name="sourceMapText"></param>
        /// <param name="jsScriptFilePathOrUrl"></param>
        /// <param name="sourceMapUri"></param>
        /// <returns></returns>
        internal static ReadOnlyCollection<byte> GetSymbolFileId(DkmScriptDocument scriptDocument, 
            string sourceMapText, string jsScriptFilePathOrUrl, Uri sourceMapUri)
        {
            var binarySerializer = new BinarySerializer();

            binarySerializer.Serialize(scriptDocument.UniqueId);
            var hasReadSourceMapText = sourceMapText != null;
            binarySerializer.Serialize(hasReadSourceMapText);
            if (hasReadSourceMapText)
            {
                binarySerializer.Serialize(sourceMapText);
            }
            binarySerializer.Serialize(jsScriptFilePathOrUrl);
            binarySerializer.Serialize(sourceMapUri.AbsoluteUri);

            var byteArray = binarySerializer.ToArray();
            
            binarySerializer.Close();
            return new ReadOnlyCollection<byte>(byteArray); 
        }

        /// <summary>
        /// Gets the script document, sourcemap, jscript file path/url and source map URI from encoded data in the symbol file id of the ts module
        /// </summary>
        /// <param name="tsModuleInstance">Typescript module instance</param>
        /// <param name="sourceMapText">returns the source map text</param>
        /// <param name="jsScriptFilePathOrUrl"></param>
        /// <param name="sourceMapUri"></param>
        /// <returns></returns>
        internal static DkmScriptDocument GetSourceMapTextInfo(DkmModuleInstance tsModuleInstance,
            out string sourceMapText, out string jsScriptFilePathOrUrl, out Uri sourceMapUri)
        {
            if (TypeScriptRuntime.IsTypeScriptModuleInstance(tsModuleInstance))
            {
                var symbolFileIdData = (tsModuleInstance.SymbolFileId as DkmCustomSymbolFileId).Data;
                var binaryDeserializer = new BinaryDeserializer(symbolFileIdData.ToArray());

                var scriptDocumentId = binaryDeserializer.DeserializeGuid();
                var hasReadSourceMapText = binaryDeserializer.DeserializeBoolean();
                sourceMapText = hasReadSourceMapText ? binaryDeserializer.DeserializeString() : null;
                jsScriptFilePathOrUrl = binaryDeserializer.DeserializeString();
                var sourceMapUrl = binaryDeserializer.DeserializeString();
                binaryDeserializer.Close();

                sourceMapUri = new Uri(sourceMapUrl);
                return TypeScriptRuntime.GetScriptDocument(tsModuleInstance.RuntimeInstance, scriptDocumentId);
            }

            sourceMapText = null;
            jsScriptFilePathOrUrl = null;
            sourceMapUri = null;
            return null;
        }

        /// <summary>
        /// Gets the script document from that was just loaded from the typescript module's symbol file id information
        /// </summary>
        /// <param name="tsModuleInstance"></param>
        /// <returns></returns>
        internal static DkmScriptDocument GetScriptDocument(DkmModuleInstance tsModuleInstance)
        {
            var scriptDocumentId = TypeScriptSymbolFileId.GetScriptDocumentUniqueId(tsModuleInstance);
            if (scriptDocumentId != Guid.Empty)
            {
                return TypeScriptRuntime.GetScriptDocument(tsModuleInstance.RuntimeInstance, scriptDocumentId);
            }

            return null;
        }

        /// <summary>
        /// Gets the unique id of jsScript document from the typescript module
        /// </summary>
        /// <param name="tsModuleInstance">typescript module instance</param>
        /// <returns></returns>
        internal static Guid GetScriptDocumentUniqueId(DkmModuleInstance tsModuleInstance)
        {
            if (TypeScriptRuntime.IsTypeScriptModuleInstance(tsModuleInstance))
            {
                var symbolFileIdData = (tsModuleInstance.SymbolFileId as DkmCustomSymbolFileId).Data;
                var binaryDeserializer = new BinaryDeserializer(symbolFileIdData.ToArray());

                var scriptDocumentId = binaryDeserializer.DeserializeGuid();
                binaryDeserializer.Close();

                return scriptDocumentId;
            }

            return Guid.Empty;
        }
    }
}
