// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.NodejsTools.Jade
{
    [Export(typeof(ITaggerProvider))]
    [TagType(typeof(IOutliningRegionTag))]
    [ContentType(JadeContentTypeDefinition.JadeContentType)]
    internal sealed class JadeOutliningTaggerProvider : ITaggerProvider
    {
        #region ITaggerProvider
        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            var tagger = ServiceManager.GetService<JadeOutliningTagger>(buffer);
            if (tagger == null)
            {
                tagger = new JadeOutliningTagger(buffer);
            }

            return tagger as ITagger<T>;
        }
        #endregion
    }
}
