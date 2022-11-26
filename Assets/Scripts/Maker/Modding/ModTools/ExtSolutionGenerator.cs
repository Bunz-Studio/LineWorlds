using System.IO;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

namespace ExternMaker
{
    public static class ExtSolutionGenerator
    {
        public static string format = "Microsoft Visual Studio Solution File, Format Version 12.00\n" +
            "# Visual Studio 2012\n" +
            "# SharpDevelop 5.1\n" +
            "VisualStudioVersion = 12.0.20827.3\n" +
            "MinimumVisualStudioVersion = 10.0.40219.1\n" +
            "Project(\"{0}\") = \"{2}\", \"{2}.csproj\", \"{1}\"\n" +
            "EndProject\n" +
            "Global\n" +
            "	GlobalSection(SolutionConfigurationPlatforms) = preSolution\n" +
            "		Debug|Any CPU = Debug|Any CPU\n" +
            "		Release|Any CPU = Release|Any CPU\n" +
            "	EndGlobalSection\n" +
            "	GlobalSection(ProjectConfigurationPlatforms) = postSolution\n" +
            "		{1}.Debug|Any CPU.ActiveCfg = Debug|Any CPU\n" +
            "		{1}.Debug|Any CPU.Build.0 = Debug|Any CPU\n" +
            "		{1}.Release|Any CPU.ActiveCfg = Release|Any CPU\n" +
            "		{1}.Release|Any CPU.Build.0 = Release|Any CPU\n" +
            "	EndGlobalSection\n" +
            "EndGlobal";

        public static string assemblyInfo = "#region Using directives\n" +
        "using System;\n" +
        "using System.Reflection;\n" +
        "using System.Runtime.InteropServices;\n" +
        "#endregion\n" +
        "// General Information about an assembly is controlled through the following\n" +
        "// set of attributes. Change these attribute values to modify the information\n" +
        "// associated with an assembly.\n" +
        "[assembly: AssemblyTitle(\"{0}\")]\n" +
        "[assembly: AssemblyDescription(\"\")]\n" +
        "[assembly: AssemblyConfiguration(\"\")]\n" +
        "[assembly: AssemblyCompany(\"\")]\n" +
        "[assembly: AssemblyProduct(\"{0}\")]\n" +
        "[assembly: AssemblyCopyright(\"Copyright 2021\")]\n" +
        "[assembly: AssemblyTrademark(\"\")]\n" +
        "[assembly: AssemblyCulture(\"\")]\n" +
        "// This sets the default COM visibility of types in the assembly to invisible.\n" +
        "// If you need to expose a type to COM, use [ComVisible(true)] on that type.\n" +
        "[assembly: ComVisible(false)]\n" +
        "[assembly: AssemblyVersion(\"{1}\")]";

        static bool p_dataChanged;
        static string p_dataPath = Path.Combine(Storage.GoUpFolder(Application.dataPath));
        public static string dataPath
        {
            get
            {
                return Application.isEditor ? p_dataPath : p_dataChanged ? p_dataPath : Application.dataPath;
            }

            set
            {
                p_dataChanged = true;
                p_dataPath = value;
            }
        }

        public static void Generate(string directory, ExtModProject project)
        {
            try
            {
                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
                string[] paths =
                {
                    "Source",
                    "Properties",
                    "Library"
                };
                foreach (var path in paths)
                {
                    string p = Path.Combine(directory, path);
                    if (!Directory.Exists(p)) Directory.CreateDirectory(p);
                }

                // Solution
                PropertyGroup projectInfo = project.PropertyGroup[0];
                string fileData = string.Format(format, projectInfo.ProjectTypeGuids, projectInfo.ProjectGuid, projectInfo.AssemblyName);
                File.WriteAllText(Path.Combine(directory, projectInfo.RootNamespace + ".sln"), fileData);

                // Info
                File.WriteAllText(Path.Combine(directory, "Properties", "AssemblyInfo.cs"), string.Format(assemblyInfo, projectInfo.RootNamespace, "1.0.*"));

                // CSProj
                var d = Application.isEditor ? Storage.GoUpFolder(System.AppDomain.CurrentDomain.BaseDirectory) : Path.Combine(dataPath, "Managed");
                var n = Application.isEditor ? Path.Combine(dataPath, "Library", "ScriptAssemblies") : Path.Combine(dataPath, "Managed");
                foreach(var refer in project.ItemGroup[0].Reference)
                {
                    if(refer.Include.Contains("Unity"))
                    {
                        refer.HintPath = refer.HintPath.Replace("{UnityDirectory}", d);
                    }
                }
                project.ItemGroup[0].Reference.Add(new Reference()
                {
                    HintPath = Path.Combine(n, "Assembly-CSharp.dll"),
                    Include = "Assembly-CSharp"
                });
                string projectData = ToXmlString(project);
                projectData = projectData.Replace("utf-16", "utf-8");
                File.WriteAllText(Path.Combine(directory, projectInfo.AssemblyName + ".csproj"), projectData, System.Text.Encoding.UTF8);
            }
            catch (System.Exception e)
            {
                // Debug.LogError("SolutionGenerator:Generating", e.Message + " => " + e.Source + " => " + e.StackTrace);
                Debug.LogError(e.Message + " => " + e.Source + " => " + e.StackTrace);
            }
        }

        public static string ToXmlString<T>(this T input)
        {
            using (var writer = new StringWriter())
            {
                input.ToXml(writer);
                return writer.ToString();
            }
        }
        public static void ToXml<T>(this T objectToSerialize, Stream stream)
        {
            new XmlSerializer(typeof(T)).Serialize(stream, objectToSerialize);
        }

        public static void ToXml<T>(this T objectToSerialize, StringWriter writer)
        {
            new XmlSerializer(typeof(T)).Serialize(writer, objectToSerialize);
        }
        public static string XmlSerializeToString(this object objectInstance)
        {
            var serializer = new XmlSerializer(objectInstance.GetType());
            var sb = new StringBuilder();

            using (TextWriter writer = new StringWriter(sb))
            {
                serializer.Serialize(writer, objectInstance);
            }

            return sb.ToString();
        }

        public static T XmlDeserializeFromString<T>(this string objectData)
        {
            return (T)XmlDeserializeFromString(objectData, typeof(T));
        }

        public static object XmlDeserializeFromString(this string objectData, System.Type type)
        {
            var serializer = new XmlSerializer(type);
            object result;

            using (TextReader reader = new StringReader(objectData))
            {
                result = serializer.Deserialize(reader);
            }

            return result;
        }
    }
}
