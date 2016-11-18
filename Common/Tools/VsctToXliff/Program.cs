/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. 
 * By using this source code in any fashion, you are agreeing to be bound by the terms of 
 * the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.IO;

namespace VsctToXliff
{
    class Program
    {
        private static readonly string[] Locales = new[] { "cs", "de", "en", "es", "fr", "it", "ja", "ko", "pl", "pt-BR", "ru", "tr", "zh-Hans", "zh-Hant" };

        private static int Main(string[] args)
        {
            var parsedArgs = new Args(args);
            if (parsedArgs.IsError)
            {
                return -1;
            }

            switch (parsedArgs.Mode)
            {
                case Mode.GenerateXliff:
                    CreateXliffFiles(parsedArgs.SourceFile, parsedArgs.XliffDir);
                    return 0;
                case Mode.GenerateVsct:
                    CreateVsctFiles(parsedArgs.SourceFile, parsedArgs.XliffDir);
                    return 0;
                case Mode.Error:
                default:
                    Console.WriteLine($"Unexpected processing mode: \'{parsedArgs.Mode}\'.");
                    return -1;
            }
        }

        private static void CreateXliffFiles(string sourceFile, string xlfDir)
        {
            if (string.IsNullOrEmpty(sourceFile) || string.IsNullOrEmpty(xlfDir))
            {
                throw new ArgumentNullException("file and targetDir should be set.");
            }

            var rootName = Utilities.VsctFileNameWithoutExtension(sourceFile);

            var reader = new VsctFile(sourceFile);
            var writer = new XliffFile(xlfDir, rootName);

            foreach (var locale in Locales)
            {
                writer.WriteTranslationFile(sourceFile, reader.ReadTranslatableUnits(), locale);
            }
        }

        private static void CreateVsctFiles(string sourceFile, string xlfDir)
        {
            if (string.IsNullOrEmpty(sourceFile))
            {
                throw new ArgumentNullException("file should be set.");
            }

            var targetDir = Path.GetDirectoryName(sourceFile);
            var rootName = Utilities.VsctFileNameWithoutExtension(sourceFile);

            var vsctFile = new VsctFile(sourceFile);
            var xlfFiles = new XliffFile(xlfDir, rootName);

            foreach (var locale in Locales)
            {
                if (StringComparer.OrdinalIgnoreCase.Equals(locale, "en"))
                {
                    // for english just copy the file to a new file name
                    var destFileName = Path.Combine(targetDir, $"{rootName}.en{VsctFile.VsctExt}");
                    File.Copy(sourceFile, destFileName, overwrite: true);
                }
                else
                {
                    var translations = xlfFiles.LoadTranslatedElements(locale);
                    vsctFile.WriteTranslatedFile(translations, locale);
                }
            }
        }

        private class Args
        {
            public bool IsError { get; }

            public string XliffDir { get; }

            public string SourceFile { get; }

            public Mode Mode { get; }

            public Args(string[] args)
            {
                if (args.Length < 3)
                {
                    this.IsError = true;
                    DisplayHelp();
                }
                else
                {
                    this.SourceFile = Utilities.EnsureRootPath(args[0]);
                    this.XliffDir = Utilities.EnsureRootPath(args[1]);
                    switch (args[2].ToLowerInvariant())
                    {
                        case "--generatexlf":
                            this.Mode = Mode.GenerateXliff;
                            break;
                        case "--generatevsct":
                            this.Mode = Mode.GenerateVsct;
                            break;
                        default:
                            this.IsError = true;
                            break;
                    }
                }
            }

            private void DisplayHelp()
            {
                Console.WriteLine("usage: VsctToXliff.exe <sourcefile.vsct> <xliff dir> [--generatexlf | --generatevsct].");
                Console.WriteLine("--generatexlf\tThis will create xlf files for all VS locales in the xliff dir, overwriting any existing files!");
                Console.WriteLine("--generatevsct\tThis will create vsct files for all VS locales in the same dir as the sourecfile.vsct, overwriting any existing files!");
            }
        }

        private enum Mode
        {
            Error,
            GenerateXliff,
            GenerateVsct,
        }
    }
}
