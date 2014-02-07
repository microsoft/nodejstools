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

using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.NodejsTools.Jade {
    /// <summary>
    /// Exports the Jade content type and file extension
    /// </summary>
    class JadeContentTypeDefinition {
        public const string JadeLanguageName = "Jade";
        public const string JadeContentType = "jade";
        public const string JadeFileExtension = ".jade";

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
    }
}
