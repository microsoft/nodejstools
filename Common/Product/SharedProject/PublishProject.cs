// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudioTools.Project
{
    internal class PublishProject : IPublishProject
    {
        private readonly CommonProjectNode _node;
        private ReadOnlyCollection<IPublishFile> _files;
        private readonly IVsStatusbar _statusBar;
        private readonly PublishProjectOptions _options;
        private int _progress;

        public PublishProject(CommonProjectNode node, PublishProjectOptions options)
        {
            this._statusBar = (IVsStatusbar)node.Site.GetService(typeof(SVsStatusbar));
            this._statusBar.SetText("Starting publish...");
            this._node = node;
            this._options = options;
        }

        #region IPublishProject Members

        public IList<IPublishFile> Files
        {
            get
            {
                if (this._files == null)
                {
                    var files = new List<IPublishFile>();
                    foreach (var item in this._node.CurrentConfig.Items)
                    {
                        var publish = GetPublishSetting(item);

                        // publish if we're a Compile node and we haven't been disabled or if 
                        // we've been specifically enabled.
                        if ((item.ItemType == "Compile" && (publish == null || publish.Value)) ||
                            (publish != null && publish.Value))
                        {
                            var file = item.GetMetadataValue("FullPath");

                            var destFile = Path.GetFileName(file);
                            if (CommonUtils.IsSubpathOf(this._node.ProjectHome, file))
                            {
                                destFile = CommonUtils.GetRelativeFilePath(this._node.ProjectHome, file);
                            }
                            else
                            {
                                destFile = Path.GetFileName(file);
                            }

                            files.Add(new PublishFile(file, destFile));
                        }
                    }

                    foreach (var file in this._options.AdditionalFiles)
                    {
                        files.Add(file);
                    }

                    this._files = new ReadOnlyCollection<IPublishFile>(files);
                }

                return this._files;
            }
        }

        private static bool? GetPublishSetting(Build.Execution.ProjectItemInstance item)
        {
            bool? publish = null;
            var pubValue = item.GetMetadataValue("Publish");
            bool pubSetting;
            if (!string.IsNullOrWhiteSpace(pubValue) && Boolean.TryParse(pubValue, out pubSetting))
            {
                publish = pubSetting;
            }
            return publish;
        }

        public string ProjectDir => this._node.ProjectHome;

        public int Progress
        {
            get
            {
                return this._progress;
            }
            set
            {
                this._progress = value;
                this._statusBar.SetText(string.Format("Publish {0}% done...", this._progress));
            }
        }

        #endregion

        internal void Done()
        {
            this._statusBar.SetText("Publish succeeded");
        }

        internal void Failed(string msg)
        {
            this._statusBar.SetText("Publish failed: " + msg);
        }
    }
}

