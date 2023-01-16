// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using Microsoft.NodejsTools.ProjectWizard;
using Microsoft.NodejsTools.Telemetry;
using Microsoft.NodejsTools.TypeScript;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools;

namespace Microsoft.NodejsTools.Project.ImportWizard
{
    internal class ImportSettings : DependencyObject
    {
        private static readonly string DefaultLanguageExtensionsFilter = string.Join(";",
            new[] {
                ".txt",
                ".htm",
                ".html",
                ".css",
                ".png",
                ".jpg",
                ".gif",
                ".bmp",
                ".ico",
                ".svg",
                ".json",
                ".md",
                ".ejs",
                ".styl",
                ".xml"
            }.Select(x => "*" + x));

        // prepend ';' since this is appended to the DefaultLanguageExtensionsFilter
        private const string TypeScriptFilters = ";*.ts;*.tsx";
        private const string JavaScriptFilters = ";*.js;*.jsx";

        private string ProjectPath => Path.Combine(this.SourcePath, this.ProjectName) + ".njsproj";

        public ImportSettings()
        {
            this.TopLevelScriptFiles = new BulkObservableCollection<string>();

            this.Filters = DefaultLanguageExtensionsFilter;
        }

        public string ProjectName
        {
            get { return (string)GetValue(ProjectNameProperty); }
            set { SetValue(ProjectNameProperty, value); }
        }

        public string SourcePath
        {
            get { return (string)GetValue(SourcePathProperty); }
            set { SetValue(SourcePathProperty, value); }
        }

        public string Filters
        {
            get { return (string)GetValue(FiltersProperty); }
            set { SetValue(FiltersProperty, value); }
        }

        public ObservableCollection<string> TopLevelScriptFiles
        {
            get { return (ObservableCollection<string>)GetValue(TopLevelScriptFilesProperty); }
            private set { SetValue(TopLevelScriptFilesPropertyKey, value); }
        }

        public string StartupFile
        {
            get { return (string)GetValue(StartupFileProperty); }
            set { SetValue(StartupFileProperty, value); }
        }

        public ProjectLanguage ProjectLanguage
        {
            get { return (ProjectLanguage)GetValue(ProjectLanguageProperty); }
            set { SetValue(ProjectLanguageProperty, value); }
        }

        public bool IsValid
        {
            get { return (bool)GetValue(IsValidProperty); }
            private set { SetValue(IsValidPropertyKey, value); }
        }

        public static readonly DependencyProperty ProjectNameProperty = DependencyProperty.Register(nameof(ProjectName), typeof(string), typeof(ImportSettings), new PropertyMetadata(ProjectName_Updated));
        public static readonly DependencyProperty SourcePathProperty = DependencyProperty.Register(nameof(SourcePath), typeof(string), typeof(ImportSettings), new PropertyMetadata(SourceOrLanguage_Updated));
        public static readonly DependencyProperty FiltersProperty = DependencyProperty.Register(nameof(Filters), typeof(string), typeof(ImportSettings), new PropertyMetadata());
        private static readonly DependencyPropertyKey TopLevelScriptFilesPropertyKey = DependencyProperty.RegisterReadOnly(nameof(TopLevelScriptFiles), typeof(ObservableCollection<string>), typeof(ImportSettings), new PropertyMetadata());
        public static readonly DependencyProperty TopLevelScriptFilesProperty = TopLevelScriptFilesPropertyKey.DependencyProperty;
        public static readonly DependencyProperty StartupFileProperty = DependencyProperty.Register(nameof(StartupFile), typeof(string), typeof(ImportSettings), new PropertyMetadata());
        public static readonly DependencyProperty ProjectLanguageProperty = DependencyProperty.Register(nameof(ProjectLanguage), typeof(ProjectLanguage), typeof(ImportSettings), new PropertyMetadata(SourceOrLanguage_Updated));

        private static void RecalculateIsValid(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!d.Dispatcher.CheckAccess())
            {
                d.Dispatcher.Invoke(() => RecalculateIsValid(d, e));
                return;
            }

            var s = d as ImportSettings;
            if (s == null)
            {
                d.SetValue(IsValidPropertyKey, false);
                return;
            }

            // Make sure the project name is not a relative path, 
            // further validation is handled by checking the whole path.
            var isValid = !string.IsNullOrEmpty(s.ProjectName) &&
                s.ProjectName.IndexOfAny(new[] { '\\', '/' }) < 0 &&
                CommonUtils.IsValidPath(s.SourcePath) &&
                CommonUtils.IsValidPath(s.ProjectPath) &&
                Directory.Exists(s.SourcePath);

            d.SetValue(IsValidPropertyKey, isValid);
        }

        private static void SourceOrLanguage_Updated(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!d.Dispatcher.CheckAccess())
            {
                d.Dispatcher.BeginInvoke((Action)(() => SourceOrLanguage_Updated(d, e)));
                return;
            }

            RecalculateIsValid(d, e);

            var settings = d as ImportSettings;
            if (settings == null)
            {
                return;
            }

            var sourcePath = settings.SourcePath;
            if (Directory.Exists(sourcePath))
            {
                // cache so we don't get a cross thread exception when accessing the settings object
                var filters = settings.Filters;
                var dispatcher = settings.Dispatcher;
                var language = settings.ProjectLanguage;

                // Nice async machinery does not work correctly in unit-tests,
                // so using Dispatcher directly.
                Task.Factory.StartNew(() =>
                {
                    string[] fileList = null;

                    // find a logical startup file
                    // ignore tsx and jsx for now
                    switch (language)
                    {
                        case ProjectLanguage.TypeScript:
                            fileList = EnumerateTopLevelFiles("*.ts");
                            break;
                        case ProjectLanguage.JavaScript:
                            fileList = EnumerateTopLevelFiles("*.js");
                            break;
                        default:
                            Debug.Assert(true, "unexpected language selected.");
                            fileList = EnumerateTopLevelFiles("*.js");
                            break;
                    }

                    dispatcher.BeginInvoke((Action)(() =>
                    {
                        if (settings.TopLevelScriptFiles is BulkObservableCollection<string> toplevelFiles)
                        {
                            toplevelFiles.Clear();
                            toplevelFiles.AddRange(fileList);
                        }
                        else
                        {
                            settings.TopLevelScriptFiles.Clear();
                            foreach (var file in fileList)
                            {
                                settings.TopLevelScriptFiles.Add(file);
                            }
                        }
                        if (fileList.Contains("server.ts"))
                        {
                            settings.StartupFile = "server.ts";
                        }
                        else if (fileList.Contains("server.js"))
                        {
                            settings.StartupFile = "server.js";
                        }
                        else if (fileList.Contains("app.ts"))
                        {
                            settings.StartupFile = "app.ts";
                        }
                        else if (fileList.Contains("app.js"))
                        {
                            settings.StartupFile = "app.js";
                        }
                        else if (fileList.Length > 0)
                        {
                            settings.StartupFile = fileList.First();
                        }
                    }));
                });
            }
            else
            {
                settings.TopLevelScriptFiles.Clear();
            }

            string[] EnumerateTopLevelFiles(string extension)
            {
                var files = Directory.EnumerateFiles(sourcePath, extension, SearchOption.TopDirectoryOnly);
                return files.Select(Path.GetFileName).ToArray();
            }
        }

        private static void ProjectName_Updated(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RecalculateIsValid(d, e);
        }

        private static readonly DependencyPropertyKey IsValidPropertyKey = DependencyProperty.RegisterReadOnly("IsValid", typeof(bool), typeof(ImportSettings), new PropertyMetadata(false));
        public static readonly DependencyProperty IsValidProperty = IsValidPropertyKey.DependencyProperty;

        private static XmlWriter GetDefaultWriter(string projectPath)
        {
            var settings = new XmlWriterSettings
            {
                CloseOutput = true,
                Encoding = Encoding.UTF8,
                Indent = true,
                IndentChars = "    ",
                NewLineChars = Environment.NewLine,
                NewLineOnAttributes = false
            };

            var dir = Path.GetDirectoryName(projectPath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            return XmlWriter.Create(projectPath, settings);
        }

        private static void EnsurePackageJson(string sourcePath, string projectName)
        {
            var packageJson = Path.Combine(sourcePath, "package.json");
            if (!File.Exists(packageJson))
            {
                try
                {
                    File.WriteAllText(packageJson,
$@"{{
  ""name"": ""{projectName}"",
  ""version"": ""0.0.0"",
  ""description"": ""{projectName}"",
  ""author"": {{
    ""name"": """"
  }}
}}");
                }
                catch (Exception exc) when (exc is UnauthorizedAccessException || exc is IOException)
                {
                }
            }
        }

        public string CreateRequestedProject()
        {
            var task = CreateRequestedProjectAsync();
            task.Wait();
            return task.Result;
        }

        public Task<string> CreateRequestedProjectAsync()
        {
            var projectPath = this.ProjectPath;
            var sourcePath = this.SourcePath;
            var filters = this.Filters;
            var startupFile = this.StartupFile;
            var projectLanguage = this.ProjectLanguage;
            var projectName = this.ProjectName;

            return Task.Run<string>(() =>
            {
                var success = false;
                try
                {
                    //ensure package.json
                    EnsurePackageJson(sourcePath, projectName);

                    using (var writer = GetDefaultWriter(projectPath))
                    {
                        WriteProjectXml(writer, projectPath, sourcePath, filters, startupFile, projectLanguage);
                    }
                    TelemetryHelper.LogProjectImported();
                    success = true;
                    return projectPath;
                }
                finally
                {
                    if (!success)
                    {
                        try
                        {
                            File.Delete(projectPath);
                        }
                        catch
                        {
                            // Try and avoid leaving stray files, but it does
                            // not matter much if we do.
                        }
                    }
                }
            });
        }

        private static bool ShouldIncludeDirectory(string dirName)
        {
            // Why relative paths only?
            // Consider the following absolute path:
            //   c:\sources\.dotted\myselectedfolder\routes\
            // Where the folder selected in the wizard is:
            //   c:\sources\.dotted\myselectedfolder\
            // We don't want to exclude that folder from the project, despite a part
            // of that path having a dot prefix.
            // By evaluating relative paths only:
            //   routes\
            // We won't reject the folder.
            Debug.Assert(!Path.IsPathRooted(dirName));
            return !dirName.Split('/', '\\').Any(name => name.StartsWith("."));
        }

        internal static void WriteProjectXml(
            XmlWriter writer,
            string projectPath,
            string sourcePath,
            string filters,
            string startupFile,
            ProjectLanguage projectLanguage
        )
        {
            var projectHome = CommonUtils.GetRelativeDirectoryPath(Path.GetDirectoryName(projectPath), sourcePath);
            var projectGuid = Guid.NewGuid();

            writer.WriteStartDocument();
            writer.WriteStartElement("Project", "http://schemas.microsoft.com/developer/msbuild/2003");
            writer.WriteAttributeString("DefaultTargets", "Build");

            writer.WriteStartElement("PropertyGroup");

            writer.WriteStartElement("Configuration");
            writer.WriteAttributeString("Condition", " '$(Configuration)' == '' ");
            writer.WriteString("Debug");
            writer.WriteEndElement();

            writer.WriteElementString("SchemaVersion", "2.0");
            writer.WriteElementString("ProjectGuid", projectGuid.ToString("B"));
            writer.WriteElementString("ProjectHome", string.IsNullOrWhiteSpace(projectHome) ? "." : projectHome);
            writer.WriteElementString("ProjectView", "ShowAllFiles");

            if (CommonUtils.IsValidPath(startupFile))
            {
                writer.WriteElementString("StartupFile", Path.GetFileName(startupFile));
            }
            else
            {
                writer.WriteElementString("StartupFile", string.Empty);
            }
            writer.WriteElementString("WorkingDirectory", ".");
            writer.WriteElementString("OutputPath", ".");
            writer.WriteElementString("ProjectTypeGuids", "{3AF33F2E-1136-4D97-BBB7-1795711AC8B8};{349c5851-65df-11da-9384-00065b846f21};{9092AA53-FB77-4645-B42D-1CCCA6BD08BD}");

            var typeScriptSupport = projectLanguage == ProjectLanguage.TypeScript || filters.IndexOf("*.ts", StringComparison.OrdinalIgnoreCase) > -1;

            if (typeScriptSupport)
            {
                writer.WriteElementString("EnableTypeScript", "true");
            }

            writer.WriteStartElement("VisualStudioVersion");
            writer.WriteAttributeString("Condition", "'$(VisualStudioVersion)' == ''");
            writer.WriteString("17.0");
            writer.WriteEndElement();

            writer.WriteStartElement("VSToolsPath");
            writer.WriteAttributeString("Condition", "'$(VSToolsPath)' == ''");
            writer.WriteString(@"$(MSBuildExtensionsPath32)\Microsoft\VisualStudio\v$(VisualStudioVersion)");
            writer.WriteEndElement();

            writer.WriteEndElement(); // </PropertyGroup>

            // VS requires property groups with conditions for Debug
            // and Release configurations or many COMExceptions are
            // thrown.
            writer.WriteStartElement("PropertyGroup");
            writer.WriteAttributeString("Condition", "'$(Configuration)' == 'Debug'");
            writer.WriteEndElement();
            writer.WriteStartElement("PropertyGroup");
            writer.WriteAttributeString("Condition", "'$(Configuration)' == 'Release'");
            writer.WriteEndElement();

            var folders = new HashSet<string>(
                Directory.EnumerateDirectories(sourcePath, "*", SearchOption.AllDirectories)
                    .Select(dirName =>
                        CommonUtils.TrimEndSeparator(
                            CommonUtils.GetRelativeDirectoryPath(sourcePath, dirName)
                        )
                    )
                    .Where(ShouldIncludeDirectory)
            );

            // Exclude node_modules and bower_components folders.
            folders.RemoveWhere(NodejsConstants.ContainsNodeModulesOrBowerComponentsFolder);

            writer.WriteStartElement("ItemGroup");

            var projectFileFilters = "";
            switch (projectLanguage)
            {
                case ProjectLanguage.TypeScript:
                    projectFileFilters = filters + TypeScriptFilters;
                    break;
                case ProjectLanguage.JavaScript:
                default:
                    projectFileFilters = filters + JavaScriptFilters;
                    break;
            }

            foreach (var file in EnumerateAllFiles(sourcePath, projectFileFilters, projectLanguage))
            {
                if (TypeScriptHelpers.IsTypeScriptFile(file) || TypeScriptHelpers.IsTsJsConfigJsonFile(file))
                {
                    writer.WriteStartElement(NodejsConstants.NoneItemType);
                }
                else
                {
                    writer.WriteStartElement(NodejsConstants.ContentItemType);
                }
                writer.WriteAttributeString("Include", file);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();

            writer.WriteStartElement("ItemGroup");
            foreach (var folder in folders.Where(s => !string.IsNullOrWhiteSpace(s)).OrderBy(s => s))
            {
                writer.WriteStartElement("Folder");
                writer.WriteAttributeString("Include", folder);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();

            writer.WriteStartElement("Import");
            writer.WriteAttributeString("Project", @"$(MSBuildToolsPath)\Microsoft.Common.targets");
            writer.WriteAttributeString("Condition", @"Exists('$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props')");
            writer.WriteEndElement();

            writer.WriteStartElement("Import");
            writer.WriteAttributeString("Project", @"$(VSToolsPath)\Node.js Tools\Microsoft.NodejsToolsV2.targets");
            writer.WriteEndElement();

            writer.WriteRaw(@"
    <ProjectExtensions>
        <VisualStudio>
          <FlavorProperties GUID=""{349c5851-65df-11da-9384-00065b846f21}"">
            <WebProjectProperties>
              <UseIIS>False</UseIIS>
              <AutoAssignPort>True</AutoAssignPort>
              <DevelopmentServerPort>0</DevelopmentServerPort>
              <DevelopmentServerVPath>/</DevelopmentServerVPath>
              <IISUrl>http://localhost:48022/</IISUrl>
              <NTLMAuthentication>False</NTLMAuthentication>
              <UseCustomServer>True</UseCustomServer>
              <CustomServerUrl>http://localhost:1337</CustomServerUrl>
              <SaveServerSettingsInUserFile>False</SaveServerSettingsInUserFile>
            </WebProjectProperties>
          </FlavorProperties>
          <FlavorProperties GUID=""{349c5851-65df-11da-9384-00065b846f21}"" User="""">
            <WebProjectProperties>
              <StartPageUrl>
              </StartPageUrl>
              <StartAction>CurrentPage</StartAction>
              <AspNetDebugging>True</AspNetDebugging>
              <SilverlightDebugging>False</SilverlightDebugging>
              <NativeDebugging>False</NativeDebugging>
              <SQLDebugging>False</SQLDebugging>
              <ExternalProgram>
              </ExternalProgram>
              <StartExternalURL>
              </StartExternalURL>
              <StartCmdLineArguments>
              </StartCmdLineArguments>
              <StartWorkingDirectory>
              </StartWorkingDirectory>
              <EnableENC>False</EnableENC>
              <AlwaysStartWebServerOnDebug>False</AlwaysStartWebServerOnDebug>
            </WebProjectProperties>
          </FlavorProperties>
        </VisualStudio>
    </ProjectExtensions>
");

            writer.WriteEndElement(); // </Project>

            writer.WriteEndDocument();
        }

        private static IEnumerable<string> EnumerateAllFiles(string source, string filters, ProjectLanguage language)
        {
            var files = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var patterns = filters.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();

            var directories = new List<string>() { source };

            try
            {
                directories.AddRange(
                    Directory.EnumerateDirectories(source, "*", SearchOption.AllDirectories)
                    .Where(dirName => ShouldIncludeDirectory(CommonUtils.TrimEndSeparator(CommonUtils.GetRelativeDirectoryPath(source, dirName))))
                );
            }
            catch (Exception exc) when (exc is UnauthorizedAccessException || exc is IOException)
            {
            }

            foreach (var dir in directories)
            {
                if (NodejsConstants.ContainsNodeModulesOrBowerComponentsFolder(dir))
                {
                    continue;
                }
                try
                {
                    foreach (var filter in patterns)
                    {
                        files.UnionWith(Directory.EnumerateFiles(dir, filter, SearchOption.TopDirectoryOnly));
                    }
                }
                catch (Exception exc) when (exc is UnauthorizedAccessException || exc is IOException)
                {
                }
            }

            var res = files
                .Where(path => path.StartsWith(source, StringComparison.OrdinalIgnoreCase))
                .Select(path => path.Substring(source.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));

            // We only want to include the language specific config file by default, since the user can always add other files later
            switch (language)
            {
                case ProjectLanguage.TypeScript:
                    return res.Where(f => !f.EndsWith("jsconfig.json"));
                case ProjectLanguage.JavaScript:
                default:
                    return res.Where(f => !f.EndsWith("tsconfig.json"));
            }
        }
    }
}
