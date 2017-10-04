// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Microsoft.VisualStudio.Editors.PropertyPages;

namespace Microsoft.VisualStudioTools.Project
{
    /// <summary>
    /// Base class for property pages based on a WinForm control.
    /// </summary>
    public abstract class CommonPropertyPage : PropPageBase
    {
        protected override Size DefaultSize { get; set; } = new Size(800, 600);

        public abstract void LoadSettings();

        public abstract Control Control { get; }

        internal virtual CommonProjectNode Project { get; set; }

        protected override void SetObjects(uint count, object[] punk)
        {
            if (punk == null)
            {
                return;
            }

            if (count > 0)
            {
                if (punk[0] is ProjectConfig)
                {
                    if (this.Project == null)
                    {
                        this.Project = (CommonProjectNode)((CommonProjectConfig)punk.First()).ProjectMgr;
                    }

                    var configs = new List<CommonProjectConfig>();

                    for (var i = 0; i < count; i++)
                    {
                        var config = (CommonProjectConfig)punk[i];

                        configs.Add(config);
                    }
                }
                else if (punk[0] is NodeProperties)
                {
                    if (this.Project == null)
                    {
                        this.Project = (CommonProjectNode)(punk[0] as NodeProperties).HierarchyNode.ProjectMgr;
                    }
                }
            }
            else
            {
                this.Project = null;
            }

            if (this.Project != null)
            {
                LoadSettings();
            }
        }
    }
}
