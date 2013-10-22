namespace Microsoft.NodejsTools.Npm.SPI
{
    internal class Script : IScript
    {
        private dynamic m_Code;

        public Script(string name, dynamic code)
        {
            Name = name;
            m_Code = code;
        }

        public string Name { get; private set; }

        public string Code
        {
            get
            {
                return m_Code.ToString();
            }
        }
    }
}