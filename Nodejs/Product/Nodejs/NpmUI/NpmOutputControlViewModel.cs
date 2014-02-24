/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Microsoft.NodejsTools.Npm;

namespace Microsoft.NodejsTools.NpmUI {
    class NpmOutputControlViewModel : INotifyPropertyChanged, IDisposable {

        private class QueuedNpmCommandInfo : EventArgs {

            public QueuedNpmCommandInfo(
                string arguments) {
                Name = arguments;
                IsFreeformArgumentCommand = true;
            }

            public QueuedNpmCommandInfo(
                string name,
                string version) {
                Name = name;
                Version = version;
                IsGlobalInstall = true;
                IsFreeformArgumentCommand = false;
            }

            public QueuedNpmCommandInfo(
                string name,
                string version,
                DependencyType depType) : this(name, version) {
                DependencyType = depType;
                IsGlobalInstall = false;
            }

            public bool IsFreeformArgumentCommand { get; private set; }
            public string Arguments {
                get { return Name; }
            }
            public string Name { get; private set; }
            public string Version { get; private set; }
            public DependencyType DependencyType { get; private set; }
            public bool IsGlobalInstall { get; private set; }
        }

        private INpmController _npmController;
        private Queue<QueuedNpmCommandInfo> _commandQueue = new Queue<QueuedNpmCommandInfo>();
        private readonly object _queueLock = new object();
        private bool _isDisposed;
        private string _statusText = Resources.NpmStatusReady;
        private bool _isExecutingCommand;
        private bool _withErrors;
        private FlowDocument _output = new FlowDocument();
        private Thread _worker;
        
        public NpmOutputControlViewModel() {
            var style = new Style(typeof(Paragraph));
            style.Setters.Add(new Setter(Block.MarginProperty, new Thickness(0)));
            _output.Resources.Add(typeof(Paragraph), style);

            _worker = new Thread(Run);
            _worker.Name = "npm UI Execution";
            _worker.IsBackground = true;
            _worker.Start();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Pulse() {
            lock (_queueLock) {
                Monitor.PulseAll(_queueLock);
            }
        }

        public INpmController NpmController {
            get { return _npmController; }
            set {
                _npmController = value;   
                OnPropertyChanged();
                Pulse();
            }
        }

        public string StatusText {
            get { return _statusText; }
            set {
                _statusText = value;
                OnPropertyChanged();
            }
        }

        public bool IsExecutingCommand {
            get {
                lock (_queueLock) {
                    return _isExecutingCommand;
                }
            }
            set {
                lock (_queueLock) {
                    _isExecutingCommand = value;
                    Pulse();
                }
                OnPropertyChanged();
                OnPropertyChanged("ExecutionProgressVisibility");
            }
        }

        public Visibility ExecutionProgressVisibility {
            get { return IsExecutingCommand ? Visibility.Visible : Visibility.Hidden; }
        }

        private void QueueCommand(QueuedNpmCommandInfo info) {
            lock (_queueLock) {
                _commandQueue.Enqueue(info);
                Monitor.PulseAll(_queueLock);
            }
        }

        public void QueueCommand(string arguments) {
            QueueCommand(new QueuedNpmCommandInfo(arguments));
        }

        public void QueueCommand(string command, string arguments) {
            QueueCommand(string.Format("{0} {1}", command, arguments));
        }

        public void QueueInstallPackage(
            string name,
            string version,
            DependencyType type) {
            QueueCommand(new QueuedNpmCommandInfo(name, version, type));
        }

        public void QueueInstallGlobalPackage(
            string name,
            string version) {
            QueueCommand(new QueuedNpmCommandInfo(name, version));
        }

        private async void Execute(QueuedNpmCommandInfo info) {
            IsExecutingCommand = true;
            using (var commander = _npmController.CreateNpmCommander()) {
                commander.OutputLogged += commander_OutputLogged;
                commander.ErrorLogged += commander_ErrorLogged;
                commander.ExceptionLogged += commander_ExceptionLogged;
                commander.CommandCompleted += commander_CommandCompleted;

                try {
                    if (info.IsFreeformArgumentCommand) {
                        await commander.ExecuteNpmCommandAsync(info.Arguments);
                    } else if (info.IsGlobalInstall) {
                        await commander.InstallGlobalPackageByVersionAsync(
                                info.Name,
                                info.Version);
                    } else {
                        await commander.InstallPackageByVersionAsync(
                                    info.Name,
                                    info.Version,
                                    info.DependencyType,
                                    true);
                    }
                } finally {
                    commander.OutputLogged -= commander_OutputLogged;
                    commander.ErrorLogged -= commander_ErrorLogged;
                    commander.ExceptionLogged -= commander_ExceptionLogged;
                    commander.CommandCompleted -= commander_CommandCompleted;
                }
            }
        }

        private void HandleCompletion() {
            //  Don't think we need to do any of this.
            //_busyControl.Finished = true;

            //var exception = _task.Exception;
            //if (null != exception) {
            //    _withErrors = true;
            //    WriteLines(ErrorHelper.GetExceptionDetailsText(exception), true);
            //}

            //if (_cancelled || _withErrors) {
            //    _busyControl.Message = _cancelled
            //        ? "npm Operation Cancelled..."
            //        : "npm Operation Failed...";
            //} else {
            //    _busyControl.Message = "npm Operation Completed Successfully...";
            //}

            //_btnClose.Location = _btnCancel.Location;
            //_btnCancel.Visible = false;
            //_btnClose.Visible = true;
        }

        private void commander_CommandCompleted(object sender, NpmCommandCompletedEventArgs e) {
            IsExecutingCommand = false;
            Application.Current.Dispatcher.BeginInvoke(
                new Action(HandleCompletion));
        }

        public FlowDocument Output {
            get { return _output; }
        }

//        private void WriteOutput(string output) {
//            if (_rtf.Length == 0) {
//                _rtf.Append(@"{\rtf1\ansicpg"
//                    + Console.OutputEncoding.CodePage
//                    + @"\deff0 {\fonttbl {\f0 Consolas;}}
//{\colortbl;\red255\green255\blue255;\red255\green0\blue0;\red255\green255\blue0;}\fs16
//");
//            }

//            if (output.Length > 0 && output[0] != '\\') {
//                //  Apply default text color
//                _rtf.Append(@"\cf1");
//            }

//            _rtf.Append(output.EndsWith(Environment.NewLine) ? output.Substring(0, output.Length - Environment.NewLine.Length) : output);
//            _rtf.Append("\\line");
//            _rtf.Append(Environment.NewLine);

//            //  There surely has to be a nicer way to do this but
//            //  AppendText() just appends plaintext, hence the use
//            //  of the buffer, and the following...
//            _textOutput.Rtf = _rtf.ToString();
//            _textOutput.SelectionStart = _rtf.Length;
//            _textOutput.ScrollToCaret();
//        }

//        private void WriteError(string error) {
//            _withErrors = true;
//            WriteOutput(@"\cf2" + error);
//        }

//        private void WriteWarning(string warning) {
//            WriteOutput(@"\cf3" + warning);
//        }

//        private void WriteLine(string line, bool forceError) {
//            if (forceError || line.StartsWith("npm ERR!")) {
//                WriteError(line);
//            } else if (line.StartsWith("npm WARN")) {
//                WriteWarning(line);
//            } else {
//                WriteOutput(line);
//            }
//        }

        public event EventHandler OutputWritten;

        private void OnOutputWritten() {
            var handlers = OutputWritten;
            if (null != handlers) {
                handlers(this, EventArgs.Empty);
            }
        }

        private string Preprocess(string source) {
            //var buff = new StringBuilder();
            //foreach (var ch in source) {
            //    if (ch == '\\') {
            //        buff.Append("\\'5c");
            //    } else {
            //        buff.Append(ch);
            //    }
            //}
            //var result = buff.ToString();
            return source.EndsWith(Environment.NewLine) ? source.Substring(0, source.Length - Environment.NewLine.Length) : source;
        }

        private void WriteLines(string text, bool forceError) {
            text = Preprocess(text);
            if (forceError) {
                _withErrors = true;
            }
            foreach (var line in text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None)) {
                var sub = line;
                var paragraph = new Paragraph();

                if (sub.StartsWith("npm ")) {
                    paragraph.Inlines.Add(new Run(sub.Substring(0,4)));
                    sub = sub.Length > 4 ? sub.Substring(4) : string.Empty;
                    if (sub.StartsWith("ERR!")) {
                        _withErrors = true;
                        paragraph.Inlines.Add(new Run(sub.Substring(0, 4)) { Foreground = Brushes.Red });
                        sub = sub.Length > 4 ? sub.Substring(4) : string.Empty;
                    } else if (sub.StartsWith("WARN")) {
                        paragraph.Inlines.Add(new Run(sub.Substring(0, 4)) { Foreground = Brushes.Yellow });
                        sub = sub.Length > 4 ? sub.Substring(4) : string.Empty;
                    }
                }

                paragraph.Inlines.Add(new Run(sub));

                _output.Blocks.Add(paragraph);
            }

            OnOutputWritten();
        }

        private void commander_ExceptionLogged(object sender, NpmExceptionEventArgs e) {
            Application.Current.Dispatcher.BeginInvoke(
                new Action(() => WriteLines(ErrorHelper.GetExceptionDetailsText(e.Exception), true)));
        }

        private void commander_ErrorLogged(object sender, NpmLogEventArgs e) {
            Application.Current.Dispatcher.BeginInvoke(new Action(() => WriteLines(e.LogText, false)));
        }

        private void commander_OutputLogged(object sender, NpmLogEventArgs e) {
            Application.Current.Dispatcher.BeginInvoke(new Action(() => WriteLines(e.LogText, false)));
        }

        private string GetCommandText(QueuedNpmCommandInfo info) {
            var buff = new StringBuilder("npm ");
            if (info.IsFreeformArgumentCommand) {
                buff.Append(info.Arguments);
            } else {
                buff.Append(NpmArgumentBuilder.GetNpmInstallArguments(
                    info.Name,
                    info.Version,
                    info.DependencyType,
                    info.IsGlobalInstall,
                    true));
            }
            return buff.ToString();
        }

        private void Run() {
            int count = 0;
            // We want the thread to continue running queued commands before
            // exiting so the user can close the install window without having to wait
            // for commands to complete.
            while (!_isDisposed || count > 0) {
                QueuedNpmCommandInfo info = null;
                lock (_queueLock) {
                    while ((_commandQueue.Count == 0 && !_isDisposed)
                        || null == _npmController
                        || IsExecutingCommand) {
                        Monitor.Wait(_queueLock);
                    }

                    if (_commandQueue.Count > 0) {
                        info = _commandQueue.Dequeue();
                    }
                    count = _commandQueue.Count;
                }

                if (null != info) {
                    string  status,
                            commandText = GetCommandText(info);

                    if (count > 0) {
                        status = string.Format(
                            _withErrors ? Resources.NpmStatusExecutingQueuedErrors : Resources.NpmStatusExecutingQueued,
                            commandText,
                            count);
                    } else {
                        status = string.Format(
                            _withErrors ? Resources.NpmStatusExecutingErrors : Resources.NpmStatusExecuting,
                            commandText);
                    }

                    Application.Current.Dispatcher.BeginInvoke(
                        new Action(() => StatusText = status));

                    Execute(info);
                }
            }
        }

        public void Dispose() {
            _isDisposed = true;
            OutputWritten = null;
            Pulse();
        }
    }
}
