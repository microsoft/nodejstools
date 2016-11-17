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
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace VsctToXliff
{
    internal sealed class VsctFile
    {
        private static readonly XNamespace NS = @"http://schemas.microsoft.com/VisualStudio/2005-10-18/CommandTable";
        private static readonly string[] ChildNames = { "ButtonText", "ToolTipText", "MenuText", "CommandName" };

        public const string VsctExt = ".vsct";

        public string FileName { get; }

        public VsctFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("fileName should be set");
            }

            if (!Path.IsPathRooted(fileName))
            {
                throw new ArgumentException("Expected a rooted path");
            }

            this.FileName = fileName;
        }

        public IEnumerable<ITranslationUnit> ReadTranslatableUnits()
        {
            var document = XDocument.Load(this.FileName);
            foreach (var element in this.GetTranslatableElements(document))
            {
                var id = element.Attribute("id").Value;
                var strings = element.Element(NS + "Strings");

                foreach (var name in ChildNames)
                {
                    var child = strings.Element(NS + name);
                    if (child != null)
                    {
                        yield return new TranslationUnit($"{id}|{name}", child.Value);
                    }
                }
            }
        }

        private IEnumerable<XElement> GetTranslatableElements(XDocument root)
        {
            foreach (var menuItem in root.Descendants(NS + "Menu"))
            {
                yield return menuItem;
            }
            foreach (var menuItem in root.Descendants(NS + "Button"))
            {
                yield return menuItem;
            }
        }

        public void WriteTranslatedFile(IDictionary<string, string> translations, string targetLanguage)
        {
            var document = XDocument.Load(this.FileName);
            foreach( var element in this.GetTranslatableElements(document))
            {
                var id = element.Attribute("id").Value;
                var strings = element.Element(NS + "Strings");

                foreach (var name in ChildNames)
                {
                    var child = strings.Element(NS + name);
                    if (child != null && translations.TryGetValue($"{id}|{name}", out var value))
                    {
                        child.Value = value;
                    }
                }
            }

            var rootDir = Path.GetDirectoryName(this.FileName);
            var rootName = Utilities.VsctFileNameWithoutExtension(this.FileName);

            document.Save(Path.Combine(rootDir, $"{rootName}.{targetLanguage}{VsctExt}"));
        }

        private sealed class TranslationUnit : ITranslationUnit
        {
            public string Key { get; }
            public string EnglishValue { get; }

            public TranslationUnit(string key, string value)
            {
                this.Key = key;
                this.EnglishValue = value;
            }
        }
    }

    internal interface ITranslationUnit
    {
        string Key { get; }
        string EnglishValue { get; }
    }
}
