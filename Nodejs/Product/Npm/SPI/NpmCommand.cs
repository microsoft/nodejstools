// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudioTools.Project;

namespace Microsoft.NodejsTools.Npm.SPI
{
    internal abstract class NpmCommand : AbstractNpmLogSource
    {
        private string pathToNpm;

        private readonly StringBuilder output = new StringBuilder();
        private readonly StringBuilder error = new StringBuilder();

        private readonly bool showConsole;

        private readonly ManualResetEvent cancellation = new ManualResetEvent(false);
        private readonly object bufferLock = new object();

        protected NpmCommand(
            string fullPathToRootPackageDirectory,
            bool showConsole = false,
            string pathToNpm = null)
        {
            this.FullPathToRootPackageDirectory = fullPathToRootPackageDirectory;
            this.pathToNpm = pathToNpm;
            this.showConsole = showConsole;
        }

        protected string Arguments { get; set; }

        internal string FullPathToRootPackageDirectory { get; }

        protected string GetPathToNpm()
        {
            if (string.IsNullOrEmpty(this.pathToNpm) || !File.Exists(this.pathToNpm))
            {
                this.pathToNpm = NpmHelpers.GetPathToNpm();
            }
            return this.pathToNpm;
        }

        public string StandardOutput
        {
            get
            {
                lock (this.bufferLock)
                {
                    return this.output.ToString();
                }
            }
        }

        public string StandardError
        {
            get
            {
                lock (this.bufferLock)
                {
                    return this.error.ToString();
                }
            }
        }

        public void CancelCurrentTask()
        {
            this.cancellation.Set();
        }

        public virtual async Task<bool> ExecuteAsync()
        {
            this.OnCommandStarted(this.Arguments);

            var redirector = this.showConsole ? null : new NpmCommandRedirector(this);
            var cancelled = false;

            try
            {
                // Call this method first to throw a NpmNotFoundException before writing anything to the output.
                var pathToNpm = this.GetPathToNpm();

                redirector?.WriteLine(
                    string.Format(CultureInfo.InvariantCulture, "===={0}====\r\n\r\n",
                    string.Format(CultureInfo.InvariantCulture, Resources.ExecutingCommand, this.Arguments)));

                await NpmHelpers.ExecuteNpmCommandAsync(
                    redirector,
                    pathToNpm,
                    this.FullPathToRootPackageDirectory,
                    new[] { this.Arguments },
                    this.showConsole,
                    cancellationResetEvent: this.cancellation);
            }
            catch (NpmNotFoundException)
            {
                redirector?.WriteErrorLine(Resources.CouldNotFindNpm);
                return false;
            }
            catch (OperationCanceledException)
            {
                cancelled = true;
            }
            finally
            {
                this.OnCommandCompleted(this.Arguments, redirector?.HasErrors ?? false, cancelled);
            }

            return redirector?.HasErrors != true; // return true when the command completed without errors, or redirector is null (which means we can't track)
        }

        private sealed class NpmCommandRedirector : Redirector
        {
            private readonly NpmCommand owner;

            public NpmCommandRedirector(NpmCommand owner)
            {
                this.owner = owner;
            }

            public bool HasErrors { get; private set; }

            private string AppendToBuffer(StringBuilder buffer, string data)
            {
                if (data != null)
                {
                    lock (this.owner.bufferLock)
                    {
                        buffer.Append(data + Environment.NewLine);
                    }
                }
                return data;
            }

            public override void WriteLine(string line)
            {
                this.owner.OnOutputLogged(AppendToBuffer(this.owner.output, line));
            }

            public override void WriteErrorLine(string line)
            {
                this.HasErrors = true;
                this.owner.OnErrorLogged(AppendToBuffer(this.owner.error, line));
            }
        }
    }
}
