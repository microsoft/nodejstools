// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.NodejsTools.Profiling
{
    [ComVisible(true)]
    public sealed class AutomationProfiling : INodeProfiling
    {
        private readonly SessionsNode _sessions;

        internal AutomationProfiling(SessionsNode sessions)
        {
            _sessions = sessions;
        }

        INodeProfileSession INodeProfiling.GetSession(object item)
        {
            if (item is int)
            {
                int id = (int)item - 1;
                if (id >= 0 && id < _sessions.Sessions.Count)
                {
                    return _sessions.Sessions[id].GetAutomationObject();
                }
            }
            else if (item is string)
            {
                string name = (string)item;
                foreach (var session in _sessions.Sessions)
                {
                    if (session.Name == name)
                    {
                        return session.GetAutomationObject();
                    }
                }
            }
            return null;
        }

        INodeProfileSession INodeProfiling.LaunchProject(EnvDTE.Project projectToProfile, bool openReport)
        {
            var target = new ProfilingTarget();
            target.ProjectTarget = new ProjectTarget();
            target.ProjectTarget.TargetProject = new Guid(projectToProfile.Properties.Item("Guid").Value as string);
            target.ProjectTarget.FriendlyName = projectToProfile.Name;

            return NodejsProfilingPackage.Instance.ProfileTarget(target, openReport).GetAutomationObject();
        }

        INodeProfileSession INodeProfiling.LaunchProcess(string interpreter, string script, string workingDir, string arguments, bool openReport)
        {
            var target = new ProfilingTarget();
            target.StandaloneTarget = new StandaloneTarget();
            target.StandaloneTarget.WorkingDirectory = workingDir;
            target.StandaloneTarget.Script = script;
            target.StandaloneTarget.Arguments = arguments;

            if (interpreter.IndexOfAny(Path.GetInvalidPathChars()) == -1 && File.Exists(interpreter))
            {
                target.StandaloneTarget.InterpreterPath = interpreter;
            }
            else
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Invalid interpreter: {0}", interpreter));
            }

            return NodejsProfilingPackage.Instance.ProfileTarget(target, openReport).GetAutomationObject();
        }

        void INodeProfiling.RemoveSession(INodeProfileSession session, bool deleteFromDisk)
        {
            for (int i = 0; i < _sessions.Sessions.Count; i++)
            {
                if (session == _sessions.Sessions[i].GetAutomationObject())
                {
                    ErrorHandler.ThrowOnFailure(
                        _sessions.DeleteItem(
                            (uint)(deleteFromDisk ? __VSDELETEITEMOPERATION.DELITEMOP_DeleteFromStorage : __VSDELETEITEMOPERATION.DELITEMOP_RemoveFromProject),
                            (uint)_sessions.Sessions[i].ItemId
                        )
                    );
                    return;
                }
            }
            throw new InvalidOperationException("Session has already been removed");
        }

        bool INodeProfiling.IsProfiling
        {
            get { return NodejsProfilingPackage.Instance.IsProfiling; }
        }
    }
}

