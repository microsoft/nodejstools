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

        private dynamic m_Scripts;

        public Scripts(dynamic scripts)
        {
            m_Scripts = scripts;
        }

        public int Count
        {
            get
            {
                JObject temp = m_Scripts;
                return null == temp ? 0 : temp.Count;
            }
        }

        public IScript this[string name]
        {
            get
            {
                IScript script  = null;
                dynamic json    = m_Scripts[ name ];
                if (null != json)
                {
                    script = new Script(name, json);
                }
                return script;
            }
        }
    }
}
