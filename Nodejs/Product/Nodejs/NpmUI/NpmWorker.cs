// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.NodejsTools.Npm;

namespace Microsoft.NodejsTools.NpmUI
{
    internal sealed class NpmWorker : IDisposable
    {
        private readonly INpmController npmController;
        private readonly Queue<QueuedNpmCommandInfo> commandQueue = new Queue<QueuedNpmCommandInfo>();
        private readonly object queuelock = new object();

        private bool isDisposed;
        private bool isExecutingCommand;
        private readonly Thread worker;
        private QueuedNpmCommandInfo currentCommand;
        private INpmCommander commander;

        public NpmWorker(INpmController controller)
        {
            this.npmController = controller;

            this.worker = new Thread(this.Run)
            {
                Name = "npm worker Execution",
                IsBackground = true
            };
            this.worker.Start();
        }

        private void Pulse()
        {
            lock (this.queuelock)
            {
                Monitor.PulseAll(this.queuelock);
            }
        }

        public bool IsExecutingCommand
        {
            get
            {
                lock (this.queuelock)
                {
                    return this.isExecutingCommand;
                }
            }
            set
            {
                lock (this.queuelock)
                {
                    this.isExecutingCommand = value;
                    Pulse();
                }
            }
        }

        private void QueueCommand(QueuedNpmCommandInfo info)
        {
            lock (this.queuelock)
            {
                if (this.commandQueue.Contains(info)
                    || info.Equals(this.currentCommand))
                {
                    return;
                }
                this.commandQueue.Enqueue(info);
                Monitor.PulseAll(this.queuelock);
            }
        }

        public void QueueCommand(string arguments)
        {
            QueueCommand(new QueuedNpmCommandInfo(arguments));
        }

        private async void Execute(QueuedNpmCommandInfo info)
        {
            this.IsExecutingCommand = true;
            INpmCommander cmdr = null;
            try
            {
                lock (this.queuelock)
                {
                    cmdr = this.npmController.CreateNpmCommander();

                    this.commander = cmdr;
                }

                await cmdr.ExecuteNpmCommandAsync(info.Arguments);
            }
            finally
            {
                lock (this.queuelock)
                {
                    this.commander = null;
                }
                this.IsExecutingCommand = false;
            }
        }

        private void Run()
        {
            var count = 0;
            // We want the thread to continue running queued commands before
            // exiting so the user can close the install window without having to wait
            // for commands to complete.
            while (!this.isDisposed || count > 0)
            {
                lock (this.queuelock)
                {
                    while ((this.commandQueue.Count == 0 && !this.isDisposed)
                        || this.npmController == null
                        || this.IsExecutingCommand)
                    {
                        Monitor.Wait(this.queuelock);
                    }

                    if (this.commandQueue.Count > 0)
                    {
                        this.currentCommand = this.commandQueue.Dequeue();
                    }
                    else
                    {
                        this.currentCommand = null;
                    }
                    count = this.commandQueue.Count;
                }

                if (null != this.currentCommand)
                {
                    Execute(this.currentCommand);
                }
            }
        }

        public void Dispose()
        {
            this.isDisposed = true;
            Pulse();
        }

        private sealed class QueuedNpmCommandInfo
        {
            public QueuedNpmCommandInfo(string arguments)
            {
                this.Name = arguments;
            }

            public string Arguments => this.Name;
            public string Name { get; }

            public bool Equals(QueuedNpmCommandInfo other)
            {
                return StringComparer.CurrentCulture.Equals(this.ToString(), other?.ToString());
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as QueuedNpmCommandInfo);
            }

            public override int GetHashCode()
            {
                return this.ToString().GetHashCode();
            }

            public override string ToString()
            {
                var buff = new StringBuilder("npm ");
                buff.Append(this.Arguments);

                return buff.ToString();
            }
        }
    }
}
