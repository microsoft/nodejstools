// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.IO;
using System.Net;

namespace TestUtilities
{
    public static class WebDownloadUtility
    {
        public static string GetString(Uri siteUri)
        {
            string text;
            var req = HttpWebRequest.CreateHttp(siteUri);

            using (var resp = req.GetResponse())
            using (StreamReader reader = new StreamReader(resp.GetResponseStream()))
            {
                text = reader.ReadToEnd();
            }

            return text;
        }
    }
}

