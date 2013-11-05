using System;

namespace Microsoft.NodejsTools.Npm
{
    public class NpmLogEventArgs : EventArgs
    {
        public NpmLogEventArgs( string logText )
        {
            LogText = logText;
        }

        public string LogText { get; private set; }
    }
}