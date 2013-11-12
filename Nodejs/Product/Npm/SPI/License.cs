namespace Microsoft.NodejsTools.Npm.SPI{
    internal class License : ILicense{
        public License(string type){
            Type = type;
        }

        public License(string type, string url) : this(type){
            Url = url;
        }

        public string Type { get; private set; }
        public string Url { get; private set; }
    }
}