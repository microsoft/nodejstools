﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace MigrateToJsps
{
    internal class NjsprojFileModel
    {
        public string ProjectName { get; set; } = string.Empty;

        public string StartupFile { get; set; } = string.Empty;

        public List<string> ProjectFiles { get; set; } = new List<string>();

        public string ProjectTypeGuids { get; set; }
    }

    [XmlRoot(ElementName = "PropertyGroup")]
    public class PropertyGroup
    {
        [XmlElement(ElementName = "Name")]
        public string Name { get; set; }

        [XmlElement(ElementName = "VisualStudioVersion")]
        public string VisualStudioVersion { get; set; }

        [XmlElement(ElementName = "RootNamespace")]
        public string RootNamespace { get; set; }

        [XmlElement(ElementName = "StartupFile")]
        public string StartupFile { get; set; }

        [XmlElement(ElementName = "StartWebBrowser")]
        public string StartWebBrowser { get; set; }

        [XmlElement(ElementName = "ProjectTypeGuids")]
        public string ProjectTypeGuids { get; set; }

        [XmlAnyElement]
        public List<XmlElement> ExtraElements { get; set; } = new List<XmlElement> { };

        [XmlAnyAttribute]
        public List<XmlAttribute> ExtraAttributes { get; set; } = new List<XmlAttribute> { };
    }

    [XmlRoot(ElementName = "ItemGroup")]
    public class ItemGroup
    {
        [XmlElement(ElementName = "Content")]
        public List<Content> Content { get; set; }

        //[XmlElement(ElementName = "ProjectReference")]
        //public ProjectReference ProjectReference { get; set; }
    }

    [XmlRoot(ElementName = "Content")]
    public class Content
    {
        [XmlAttribute(AttributeName = "Include")]
        public string Include { get; set; }

        [XmlAttribute(AttributeName = "Update")]
        public string Update { get; set; }

        [XmlAnyElement]
        public List<XmlElement> ExtraElements { get; set; } = new List<XmlElement> { };

        [XmlAnyAttribute]
        public List<XmlAttribute> ExtraAttributes { get; set; } = new List<XmlAttribute> { };
    }

    [XmlRoot(ElementName = "Project", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
    public class Project
    {
        [XmlElement(ElementName = "PropertyGroup")]
        public List<PropertyGroup> PropertyGroup { get; set; } = new List<PropertyGroup> { };

        [XmlElement(ElementName = "ItemGroup")]
        public List<ItemGroup> ItemGroup { get; set; } = new List<ItemGroup> { };

        //[XmlAnyElement]
        //public List<XmlElement> ExtraElements { get; set; } = new List<XmlElement> { };

        //[XmlAnyAttribute]
        //public List<XmlAttribute> ExtraAttributes { get; set; } = new List<XmlAttribute> { };
    }
}
