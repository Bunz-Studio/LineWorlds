// XML2CSharp is used, I have no choice but to keep this for legal reasons.
/* 
Licensed under the Apache License, Version 2.0

http://www.apache.org/licenses/LICENSE-2.0
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace ExternMaker
{
    [XmlRoot(ElementName = "Configuration", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
    public class Configuration
    {
        [XmlAttribute(AttributeName = "Condition")]
        public string Condition { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "Platform", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
    public class Platform
    {
        [XmlAttribute(AttributeName = "Condition")]
        public string Condition { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "PropertyGroup", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
    public class PropertyGroup
    {
        [XmlElement(ElementName = "ProjectGuid", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
        public string ProjectGuid { get; set; }
        [XmlElement(ElementName = "ProjectTypeGuids", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
        public string ProjectTypeGuids { get; set; }
        [XmlElement(ElementName = "Configuration", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
        public Configuration Configuration { get; set; }
        [XmlElement(ElementName = "Platform", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
        public Platform Platform { get; set; }
        [XmlElement(ElementName = "OutputType", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
        public string OutputType { get; set; }
        [XmlElement(ElementName = "RootNamespace", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
        public string RootNamespace { get; set; }
        [XmlElement(ElementName = "AssemblyName", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
        public string AssemblyName { get; set; }
        [XmlElement(ElementName = "TargetFrameworkVersion", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
        public string TargetFrameworkVersion { get; set; }
        [XmlElement(ElementName = "AppDesignerFolder", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
        public string AppDesignerFolder { get; set; }
        [XmlElement(ElementName = "NoWin32Manifest", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
        public string NoWin32Manifest { get; set; }
        [XmlElement(ElementName = "RunPostBuildEvent", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
        public string RunPostBuildEvent { get; set; }
        [XmlElement(ElementName = "AllowUnsafeBlocks", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
        public string AllowUnsafeBlocks { get; set; }
        [XmlElement(ElementName = "NoStdLib", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
        public string NoStdLib { get; set; }
        [XmlElement(ElementName = "TreatWarningsAsErrors", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
        public string TreatWarningsAsErrors { get; set; }
        [XmlElement(ElementName = "IntermediateOutputPath", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
        public string IntermediateOutputPath { get; set; }
        [XmlElement(ElementName = "WarningLevel", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
        public string WarningLevel { get; set; }
        [XmlElement(ElementName = "PlatformTarget", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
        public string PlatformTarget { get; set; }
        [XmlElement(ElementName = "BaseAddress", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
        public string BaseAddress { get; set; }
        [XmlElement(ElementName = "RegisterForComInterop", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
        public string RegisterForComInterop { get; set; }
        [XmlElement(ElementName = "GenerateSerializationAssemblies", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
        public string GenerateSerializationAssemblies { get; set; }
        [XmlElement(ElementName = "FileAlignment", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
        public string FileAlignment { get; set; }
        [XmlAttribute(AttributeName = "Condition")]
        public string Condition { get; set; }
        [XmlElement(ElementName = "OutputPath", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
        public string OutputPath { get; set; }
        [XmlElement(ElementName = "DebugSymbols", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
        public string DebugSymbols { get; set; }
        [XmlElement(ElementName = "DebugType", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
        public string DebugType { get; set; }
        [XmlElement(ElementName = "Optimize", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
        public string Optimize { get; set; }
        [XmlElement(ElementName = "CheckForOverflowUnderflow", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
        public string CheckForOverflowUnderflow { get; set; }
        [XmlElement(ElementName = "DefineConstants", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
        public string DefineConstants { get; set; }
        [XmlElement(ElementName = "BaseIntermediateOutputPath", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
        public string BaseIntermediateOutputPath { get; set; }
    }

    [Serializable]
    [XmlRoot(ElementName = "Reference", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
    public class Reference
    {
        [XmlElement(ElementName = "RequiredTargetFramework", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
        public string RequiredTargetFramework { get; set; }
        [XmlAttribute(AttributeName = "Include")]
        public string Include { get; set; }
        [XmlElement(ElementName = "HintPath", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
        public string HintPath { get; set; }
    }

    [Serializable]
    public class JsonProjectReference
    {
        public JsonProjectReference()
        {

        }

        public JsonProjectReference(string rtf, string i, string hp)
        {
            RequiredTargetFramework = rtf;
            Include = i;
            HintPath = hp;
        }
        public string RequiredTargetFramework;
        public string Include;
        public string HintPath;
    }

    [XmlRoot(ElementName = "ItemGroup", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
    public class ItemGroup
    {
        [XmlElement(ElementName = "Reference", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
        public List<Reference> Reference { get; set; }
        [XmlElement(ElementName = "Compile", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
        public List<Compile> Compile { get; set; }
        [XmlElement(ElementName = "Folder", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
        public List<Folder> Folder { get; set; }
    }

    [XmlRoot(ElementName = "Compile", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
    public class Compile
    {
        [XmlAttribute(AttributeName = "Include")]
        public string Include { get; set; }
        [XmlElement(ElementName = "DependentUpon", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
        public string DependentUpon { get; set; }
    }

    [XmlRoot(ElementName = "Folder", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
    public class Folder
    {
        [XmlAttribute(AttributeName = "Include")]
        public string Include { get; set; }
    }

    [XmlRoot(ElementName = "Import", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
    public class Import
    {
        [XmlAttribute(AttributeName = "Project")]
        public string Project { get; set; }
    }

    [XmlRoot(ElementName = "Project", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
    public class ExtModProject
    {
        [XmlElement(ElementName = "PropertyGroup", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
        public List<PropertyGroup> PropertyGroup { get; set; }
        [XmlElement(ElementName = "ItemGroup", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
        public List<ItemGroup> ItemGroup { get; set; }
        [XmlElement(ElementName = "Import", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
        public Import Import { get; set; }
        [XmlAttribute(AttributeName = "ToolsVersion")]
        public string ToolsVersion { get; set; }
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }
        [XmlAttribute(AttributeName = "DefaultTargets")]
        public string DefaultTargets { get; set; }

        public static ExtModProject Generate(string directory, string projectName)
        {
            string dir = Path.Combine(directory, RenormalizeName(projectName));
            ExtModProject project = null;
            try
            {
                project = Storage.ReadAllTextLocal("DefaultProject.xml").XmlDeserializeFromString<ExtModProject>();
            }
            catch (Exception e)
            {
                ExtActionInspector.Log("Failed to generate project: " + e.Message, "SolutionGenerator", e.StackTrace);
            }
            if (project != null)
            {
                PropertyGroup projectInfo = null;
                try
                {
                    projectInfo = project.PropertyGroup[0];
                    projectInfo.ProjectGuid = GetUid();
                    projectInfo.ProjectTypeGuids = GetUid();
                    projectInfo.RootNamespace = projectName;
                    projectInfo.AssemblyName = projectName;
                    project.PropertyGroup[0] = projectInfo;
                    project.ItemGroup[1] = new ItemGroup();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message + e.StackTrace);
                }
                ExtSolutionGenerator.Generate(dir, project);
            }
            return project;
        }

        public static string RenormalizeName(string name)
        {
            char[] invalid = Path.GetInvalidPathChars();
            foreach (var a in invalid)
            {
                name = name.Replace(a, default(char));
            }
            return name;
        }
        public static string GetUid()
        {
            string[] ids =
            {
                GetRandom(7),
                GetRandom(4),
                GetRandom(4),
                GetRandom(4),
                GetRandom(12)
            };
            return "{F" + string.Join("-", ids) + "}";
        }
        public static string GetRandom(int length)
        {
            char[] c = "ABCDEFGHIJKLMNOPQRSTUVWXZ1234567890".ToCharArray();
            string b = null;
            var r = new Random();
            for (int i = 0; i < length; i++)
            {
                b += c[r.Next(0, c.Length - 1)];
            }
            return b;
        }
    }
    public class ProjectInfo
    {
        public string name;
        public string directory;
        public string owner;
        public string version = "v0.0.1-beta";
    }
}