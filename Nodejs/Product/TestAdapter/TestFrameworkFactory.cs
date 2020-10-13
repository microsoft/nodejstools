using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Evaluation;
using Microsoft.NodejsTools.TestAdapter.TestFrameworks;
using Microsoft.NodejsTools.TypeScript;
using Microsoft.VisualStudioTools;

namespace Microsoft.NodejsTools.TestAdapter
{
    public static class TestFrameworkFactory
    {
        public static Dictionary<string, HashSet<string>> GetTestItems(string projectRoot, Project project)
        {
            var configItems = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
            var testItems = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

            var projectItems = project.Items.Where(x => x.ItemType != "NodeModulesContent");

            foreach (var projectItem in projectItems)
            {
                string testFrameworkName = null;
                // TODO: Should it validate karma.conf.js? 
                // consider it can also be inside a <root>/.config/ folder and will configure the <root> folder.
                if (projectItem.EvaluatedInclude.Contains("angular.json"))
                {
                    testFrameworkName = "angular";
                }
                // TODO: Add configuration files for jest, mocha , jasmine. Consider some frameworks can be configured using package.json
                // Also some frameworks have more than one filename to define a configuration.
                // Tape is the only framework we support that doesn't have a configuration file. Decide what to do about that.

                if (testFrameworkName != null)
                {
                    if (!configItems.ContainsKey(testFrameworkName))
                    {
                        configItems.Add(testFrameworkName, new HashSet<string>(StringComparer.OrdinalIgnoreCase));
                    }

                    configItems[testFrameworkName].Add(CommonUtils.GetAbsoluteFilePath(projectRoot, projectItem.EvaluatedInclude));
                }
                else if (!configItems.Any()) // Legacy behavior. If we have found a test configuration file is safe to ignore the rest of the files.
                {
                    // TODO: Deprecate. Is configured by project file or is configured per file.
                    var testFrameworkAndFilePath = GetTestFrameworkAndFilePath(project, projectItem, projectRoot);
                    if (testFrameworkAndFilePath.HasValue)
                    {
                        var (testFramework, fileAbsolutePath) = testFrameworkAndFilePath.Value;
                        if (!testItems.ContainsKey(testFramework))
                        {
                            testItems.Add(testFramework, new HashSet<string>(StringComparer.OrdinalIgnoreCase));
                        }

                        testItems[testFramework].Add(fileAbsolutePath);
                    }
                }
            }

            return configItems.Any() ? configItems : testItems;
        }

        private static (string, string)? GetTestFrameworkAndFilePath(Project project, ProjectItem projectItem, string projectRoot)
        {
            var testRoot = project.GetProperty(NodeProjectProperty.TestRoot)?.EvaluatedValue;
            var testFrameworkName = project.GetProperty(NodeProjectProperty.TestFramework)?.EvaluatedValue;

            string fileAbsolutePath;
            if (!string.IsNullOrEmpty(testRoot) && !string.IsNullOrEmpty(testFrameworkName)) // If the test root and framework have been configured on the project.
            {
                var testRootPath = Path.GetFullPath(Path.Combine(project.DirectoryPath, testRoot));

                try
                {
                    fileAbsolutePath = CommonUtils.GetAbsoluteFilePath(projectRoot, projectItem.EvaluatedInclude);
                }
                catch (ArgumentException)
                {
                    // .Net core projects include invalid paths, ignore them and continue checking the items.
                    return null;
                }

                if (!fileAbsolutePath.StartsWith(testRootPath, StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }
            }
            else // If the file has been configured individually.
            {
                testFrameworkName = projectItem.GetMetadataValue("TestFramework");
                if (!TestFramework.IsValidTestFramework(testFrameworkName))
                {
                    return null;
                }
                fileAbsolutePath = CommonUtils.GetAbsoluteFilePath(projectRoot, projectItem.EvaluatedInclude);
            }

            // Check if file is a typecript file. If so, get the javascript file. The javascript file needs to be in the same path and name.
            // It doesn't work with bundlers or minimizers. Also, project needs to be build in order to have the js file created.
            var typeScriptTest = TypeScriptHelpers.IsTypeScriptFile(fileAbsolutePath);
            if (typeScriptTest)
            {
                fileAbsolutePath = TypeScriptHelpers.GetTypeScriptBackedJavaScriptFile(project, fileAbsolutePath);
            }
            else if (!StringComparer.OrdinalIgnoreCase.Equals(Path.GetExtension(fileAbsolutePath), ".js"))
            {
                return null;
            }

            return (testFrameworkName, fileAbsolutePath);
        }
    }
}
