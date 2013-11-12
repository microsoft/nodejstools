using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Npm.SPI
{
    internal class Bugs : IBugs
    {
        private readonly dynamic _package;

        public Bugs(dynamic package)
        {
            _package = package;
        }


        public string Url
        {
            get
            {
                string url = null;
                var bugs = _package.bugs;
                if (null != bugs)
                {
                    var token = bugs as JToken;
                    if (token.Type == JTokenType.Object)
                    {
                        var temp = bugs.url ?? bugs.web;
                        if (null != temp)
                        {
                            url = temp.ToString();
                        }
                    }
                    else
                    {
                        url = token.Value<string>();
                    }
                }
                return url;
            }
        }

        public string Email
        {
            get
            {
                string email = null;
                var bugs = _package.bugs;
                if (null != bugs)
                {
                    var token = bugs as JToken;
                    if (token.Type == JTokenType.Object)
                    {
                        var temp = bugs.email ?? bugs.mail;
                        if (null != temp)
                        {
                            email = temp.ToString();
                        }
                    }
                }
                return email;
            }
        }
    }
}
