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
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EnvDTE;
using Microsoft.NodejsTools.Npm;

namespace Microsoft.NodejsTools.NpmUI{
    public partial class BusyPopup : Form{

        private INpmCommander _commander;
        private Task _task;
        private bool _withErrors;
        private bool _cancelled;
        private StringBuilder _rtf = new StringBuilder();

        public BusyPopup(){
            InitializeComponent();
            CreateHandle();

            _btnClose.Visible = false;
        }

        public bool WithErrors{
            get { return _withErrors; }
        }

        public string Message{
            get { return _busyControl.Message; }
            set { _busyControl.Message = value; }
        }

        private void HandleCompletion(){
            _busyControl.Finished = true;

            var exception = _task.Exception;
            if (null != exception){
                _withErrors = true;
                WriteLines(ErrorHelper.GetExceptionDetailsText(exception), true);
            }

            if (_cancelled || _withErrors){
                _busyControl.Message = _cancelled
                    ? "npm Operation Cancelled..."
                    : "npm Operation Failed...";
            } else{
                _busyControl.Message = "npm Operation Completed Successfully...";
            }

            _btnClose.Location = _btnCancel.Location;
            _btnCancel.Visible = false;
            _btnClose.Visible = true;
        }

        private void Completed(){
            BeginInvoke(
                new Action(HandleCompletion));
        }

        public void ShowPopup(
            IWin32Window parent,
            INpmCommander commander,
            Action action){
            _commander = commander;
            commander.OutputLogged += commander_OutputLogged;
            commander.ErrorLogged += commander_ErrorLogged;
            commander.ExceptionLogged += commander_ExceptionLogged;
            commander.CommandCompleted += commander_CommandCompleted;

            using (_task = new Task(action)){
                //  N.B. This WON'T work because you're effectively passing in an async void that will "complete" immediately - grr
                //_task.ContinueWith(t2 => Completed());
                _task.Start();

                ShowDialog(parent);

                commander.OutputLogged -= commander_OutputLogged;
                commander.ErrorLogged -= commander_ErrorLogged;
                commander.ExceptionLogged -= commander_ExceptionLogged;
                commander.CommandCompleted -= commander_CommandCompleted;
            }
        }

        void commander_CommandCompleted(object sender, NpmCommandCompletedEventArgs e)
        {
            Completed();
        }

        private void WriteOutput(string output){
            if (_rtf.Length == 0){
                _rtf.Append(@"{\rtf1\ansicpg"
                    + Console.OutputEncoding.CodePage
                    + @"\deff0 {\fonttbl {\f0 Consolas;}}
{\colortbl;\red255\green255\blue255;\red255\green0\blue0;\red255\green255\blue0;}\fs16
");
            }

            if (output.Length > 0 && output[0] != '\\'){
                //  Apply default text color
                _rtf.Append(@"\cf1");
            }

            _rtf.Append(output.EndsWith(Environment.NewLine) ? output.Substring(0, output.Length - Environment.NewLine.Length) : output);
            _rtf.Append("\\line");
            _rtf.Append(Environment.NewLine);

            //  There surely has to be a nicer way to do this but
            //  AppendText() just appends plaintext, hence the use
            //  of the buffer, and the following...
            _textOutput.Rtf = _rtf.ToString();
            _textOutput.SelectionStart = _rtf.Length;
            _textOutput.ScrollToCaret();
        }

        private void WriteError(string error){
            _withErrors = true;
            WriteOutput(@"\cf2" + error);
        }

        private void WriteWarning(string warning){
            WriteOutput(@"\cf3" + warning);
        }

        private void WriteLine(string line, bool forceError){
            if (forceError || line.StartsWith("npm ERR!")){
                WriteError(line);
            }
            else if (line.StartsWith("npm WARN")){
                WriteWarning(line);
            } else{
                WriteOutput(line);
            }
        }

        private string Preprocess(string source){
            var buff = new StringBuilder();
            foreach (var ch in source){
                if (ch == '\\'){
                    buff.Append("\\'5c");
                } else{
                    buff.Append(ch);
                }
            }
            var result = buff.ToString();
            return result.EndsWith(Environment.NewLine) ? result.Substring(0, result.Length - Environment.NewLine.Length) : result;
        }

        private void WriteLines(string text, bool forceError){
            text = Preprocess(text);
            foreach (var line in text.Split(new string[]{"\r\n", "\n"}, StringSplitOptions.None)){
                WriteLine(line, forceError);
            }
        }

        private void commander_ErrorLogged(object sender, NpmLogEventArgs e){
            BeginInvoke(new Action(() => WriteLines(e.LogText, false)));
        }

        private void commander_OutputLogged(object sender, NpmLogEventArgs e){
            BeginInvoke(new Action(() => WriteLines(e.LogText, false)));
        }

        void commander_ExceptionLogged(object sender, NpmExceptionEventArgs e){
            BeginInvoke(new Action(() => WriteLines(ErrorHelper.GetExceptionDetailsText(e.Exception), true)));
        }

        private void DoClose(){
            DialogResult = DialogResult.OK;
            Close();
        }

        private void _btnClose_Click(object sender, EventArgs e)
        {
            DoClose();
        }

        private void _btnCancel_Click(object sender, EventArgs e)
        {
            if (null != _commander){
                _cancelled = true;
                _commander.CancelCurrentCommand();
            }
        }
    }
}
