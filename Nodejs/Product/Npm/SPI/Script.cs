namespace Microsoft.NodejsTools.Npm.SPI{
    internal class Script : IScript{
        private dynamic _code;

        public Script(string name, dynamic code){
            Name = name;
            _code = code;
        }

        public string Name { get; private set; }

        public string Code{
            get { return _code.ToString(); }
        }
    }
}