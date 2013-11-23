using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Npm.SPI
{
    internal class AbstractNpmLogSource : INpmLogSource
    {
        protected void FireNpmLogEvent(string logText, EventHandler<NpmLogEventArgs> handlers)
        {
            if (null != handlers && !string.IsNullOrEmpty(logText))
            {
                handlers(this, new NpmLogEventArgs(logText));
            }
        }

        public event EventHandler<NpmLogEventArgs> OutputLogged;

        protected void OnOutputLogged(string logText)
        {
            FireNpmLogEvent(logText, OutputLogged);
        }

        public event EventHandler<NpmLogEventArgs> ErrorLogged;

        protected void OnErrorLogged(string logText)
        {
            FireNpmLogEvent(logText, ErrorLogged);
        }

        public event EventHandler<NpmExceptionEventArgs> ExceptionLogged;

        protected void OnExceptionLogged(Exception e)
        {
            var handlers = ExceptionLogged;
            if (null != handlers)
            {
                handlers(this, new NpmExceptionEventArgs(e));
            }
        }
    }
}
