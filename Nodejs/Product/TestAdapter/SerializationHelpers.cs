// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

internal sealed class NodejsProjectSettings
{
    public NodejsProjectSettings()
    {
        this.NodeExePath = string.Empty;
        this.SearchPath = string.Empty;
        this.WorkingDir = string.Empty;
    }

    public string NodeExePath { get; set; }
    public string SearchPath { get; set; }
    public string WorkingDir { get; set; }
    public string ProjectRootDir { get; set; }
}

internal sealed class ResultObject
{
    public string title { get; set; }
    public bool passed { get; set; }
    public bool? pending { get; set; }
    public string stdout { get; set; }
    public string stderr { get; set; }
}

internal sealed class TestEvent
{
    public string type { get; set; }
    public string title { get; set; }
    public ResultObject result { get; set; }
}

internal sealed class TestCaseObject
{
    public TestCaseObject()
    {
        this.framework = string.Empty;
        this.testName = string.Empty;
        this.testFile = string.Empty;
        this.workingFolder = string.Empty;
        this.projectFolder = string.Empty;
    }

    public TestCaseObject(string framework, string testName, string testFile, string workingFolder, string projectFolder)
    {
        this.framework = framework;
        this.testName = testName;
        this.testFile = testFile;
        this.workingFolder = workingFolder;
        this.projectFolder = projectFolder;
    }
    public string framework { get; set; }
    public string testName { get; set; }
    public string testFile { get; set; }
    public string workingFolder { get; set; }
    public string projectFolder { get; set; }
}
