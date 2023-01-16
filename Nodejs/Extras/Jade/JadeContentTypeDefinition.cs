// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.NodejsTools.Jade
{
    /// <summary>
    /// Exports the Jade content type and file extension
    /// </summary>
    internal class JadeContentTypeDefinition
    {
        public const string JadeLanguageName = "Jade";
        public const string JadeContentType = "jade";
        public const string JadeFileExtension = ".jade";
        public const string PugFileExtension = ".pug";

        /// <summary>
        /// Exports the Jade content type
        /// </summary>
        [Export(typeof(ContentTypeDefinition))]
        [Name(JadeContentType)]
        [BaseDefinition("text")]
        public ContentTypeDefinition IJadeContentType { get; set; }

        /// <summary>
        /// Exports the Jade file extension
        /// </summary>
        [Export(typeof(FileExtensionToContentTypeDefinition))]
        [ContentType(JadeContentType)]
        [FileExtension(JadeFileExtension)]
        public FileExtensionToContentTypeDefinition IJadeFileExtension { get; set; }

        /// <summary>
        /// Exports the Pug file extension
        /// </summary>
        [Export(typeof(FileExtensionToContentTypeDefinition))]
        [ContentType(JadeContentType)]
        [FileExtension(PugFileExtension)]
        public FileExtensionToContentTypeDefinition IPugFileExtension { get; set; }
    }
}
