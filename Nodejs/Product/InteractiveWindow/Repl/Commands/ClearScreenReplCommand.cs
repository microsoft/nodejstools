// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.ComponentModel.Composition;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Microsoft.NodejsTools.Repl
{
    [Export(typeof(IReplCommand))]
    internal class ClearScreenReplCommand : IReplCommand
    {
        #region IReplCommand Members

        public Task<ExecutionResult> Execute(IReplWindow window, string arguments)
        {
            window.ClearScreen();
            return ExecutionResult.Succeeded;
        }

        public string Description
        {
            get { return Resources.ClearDescription; }
        }

        public string Command
        {
            get { return "cls"; }
        }

        public object ButtonContent
        {
            get
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microsoft.VisualStudio.Resources.clearscr.gif");
                image.EndInit();

                var res = new Image();
                res.Source = image;
                res.Opacity = 1.0;
                res.Width = res.Height = 16;
                return res;
            }
        }

        #endregion
    }
}
