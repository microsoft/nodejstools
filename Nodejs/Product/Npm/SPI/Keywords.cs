using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Npm.SPI
{
    internal class Keywords : IKeywords
    {
        private JArray m_Keywords;

        public Keywords(JArray keywords)
        {
            m_Keywords = keywords;
        }

        public int Count
        {
            get
            {
                return m_Keywords.Count;
            }
        }

        public string this[int index]
        {
            get { return m_Keywords[ index ].Value<string>(); }
        }

        public IEnumerator<string> GetEnumerator()
        {
            return m_Keywords.Select(keyword => keyword.Value<string>()).ToList().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}