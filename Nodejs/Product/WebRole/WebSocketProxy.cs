// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.IO;
using System.Web;
using Microsoft.VisualStudioTools;

namespace Microsoft.NodejsTools.Debugger
{
    public class WebSocketProxy : WebSocketProxyBase
    {
        public override int DebuggerPort
        {
            get { return 5858; }
        }

        public override bool AllowConcurrentConnections
        {
            get { return false; }
        }

        public override void ProcessHelpPageRequest(HttpContext context)
        {
            using (var stream = GetType().Assembly.GetManifestResourceStream("Microsoft.NodejsTools.WebRole.WebSocketProxy.html"))
            using (var reader = new StreamReader(stream))
            {
                string html = reader.ReadToEnd();
                context.Response.Write(html);
                context.Response.End();
            }
        }
    }
}
