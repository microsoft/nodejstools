using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Npm.SPI
{
    internal class Scripts : IScripts
    {

        private dynamic _scripts;

        public Scripts(dynamic scripts)
        {
            _scripts = scripts;
        }

        public int Count
        {
            get
            {
                JObject temp = _scripts;
                return null == temp ? 0 : temp.Count;
            }
        }

        public IScript this[string name]
        {
            get
            {
                IScript script  = null;
                dynamic json    = _scripts[ name ];
                if (null != json)
                {
                    script = new Script(name, json);
                }
                return script;
            }
        }
    }
}
