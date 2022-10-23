using UnityEngine;
using System.IO;
using System.Collections.Generic;
using ExternMaker.Serialization;

namespace ExternMaker
{
    public static class LiwbSerializer
    {
        public static LiwbProject CompileProject(string path, string directory, LiwbOptions options = null)
        {
            if(options == null)
            {
                options = new LiwbOptions();
            }
            var result = new LiwbProject();
            result.project = ExtProjectManager.instance.project;
            var res = Path.Combine(directory, "Resources");
            foreach (var file in Directory.GetFiles(res))
            {
                if (!file.Contains(".meta"))
                {
                    result.resourceFiles.Add(new LiwbFile(file, options.compressResources));
                }
            }
            var musPath = ExtProjectManager.instance.audioManager.originalAudioPath;
            if (!string.IsNullOrWhiteSpace(musPath) && File.Exists(musPath))
            {
                result.audioFile = new LiwbFile(musPath, options.compressAudio, options.externalAudio);
            }
            result.assembly = new LiwbAssembly();
            if (options.embedCompiledAssembly)
            {
                if (ExtCompiler.compiledAssembly != null)
                {
                    if (!string.IsNullOrWhiteSpace(ExtCompiler.compiledAssembly.Location) && File.Exists(ExtCompiler.compiledAssembly.Location))
                    {
                        result.assembly.assemblyFile = new LiwbFile(ExtCompiler.compiledAssembly.Location, options.compressAssembly);
                        result.assembly.isFileEmbedded = true;
                    }
                }
                else
                {
                    result.assembly = null;
                }
            }
            if (options.embedAssemblyCodes)
            {
                var csprojPath = result.project.info.levelName + ".csproj";
                csprojPath = Path.Combine(directory, csprojPath);
                if(Storage.FileExists(csprojPath))
                {
                    var actualProject = Storage.ReadAllText(csprojPath).XmlDeserializeFromString<ExtModProject>();
                    var codes = new List<string>();
                    var paths = new List<string>();
                    foreach (var item in actualProject.ItemGroup[1].Compile)
                    {
                        string codePath = Path.Combine(directory, item.Include);
                        Debug.Log("Embedding assembly codes: " + codePath);
                        if (File.Exists(codePath))
                        {
                            if (!string.IsNullOrWhiteSpace(item.DependentUpon))
                            {
                                int index = ExtCompiler.LookForPath(paths, item.DependentUpon);
                                if (index > -1)
                                    codes[index] += File.ReadAllText(codePath);
                            }
                            else
                            {
                                string build = File.ReadAllText(codePath);
                                codes.Add(build);
                                paths.Add(codePath);
                            }
                        }
                    }
                    foreach(var code in codes)
                    {
                        result.assembly.assemblyCodes.Add(code);
                    }
                }
                result.assembly.isCodeEmbedded = true;
            }
            return result;
        }
    }

    [System.Serializable]
    public class LiwbProject
    {
        public LiwProject project = new LiwProject();
        public LiwbFile audioFile = new LiwbFile();
        public List<LiwbFile> resourceFiles = new List<LiwbFile>();
        public LiwbFile assemblyFile
        {
            get
            {
                if (assembly == null) assembly = new LiwbAssembly();
                return assembly.assemblyFile;
            }
            set
            {
                if (assembly == null) assembly = new LiwbAssembly();
                assembly.assemblyFile = value;
            }
        }
        public LiwbAssembly assembly = new LiwbAssembly();

        public LiwbFile FindResource(string path, bool cutPath = false)
        {
            if (cutPath) path = Path.GetFileName(path);
            foreach(var res in resourceFiles)
            {
                if (res.path == path) return res;
            }
            return null;
        }

        public void ToLiwProject(string directory)
        {
            LiwSerializer.CreateProject(directory, project);
            var resPath = Path.Combine(directory, "Resources");
            foreach(var resFile in resourceFiles)
            {
                var path = Path.Combine(resPath, resFile.path);
                File.WriteAllBytes(path, resFile.GetBytes());

                LiwCustomObject.CreateMeta(path, resFile.instanceID);
            }

            try
            {
                if (assembly.isFileEmbedded)
                {
                    var asmPath = Path.Combine(directory, "Library", project.info.levelName);
                    File.WriteAllBytes(asmPath, assemblyFile.GetBytes());
                }
                if (assembly.isCodeEmbedded)
                {
                    ExtModProject.Generate(Storage.GoUpFolder(directory), Path.GetFileName(directory));
                    int i = 0;
                    foreach (var code in assembly.assemblyCodes)
                    {
                        var codePath = "CustomTriggerCode" + (i == 0 ? ".cs" : i + ".cs");
                        codePath = Path.Combine(directory, "Source", codePath);
                        File.WriteAllText(codePath, code);
                        i++;
                    }
                    // CSProj
                    var csprojPath = project.info.levelName + ".csproj";
                    csprojPath = Path.Combine(directory, csprojPath);
                    var projecta = Storage.ReadAllTextLocal("DefaultProject.xml").XmlDeserializeFromString<ExtModProject>();
                    var d = Application.isEditor ? Storage.GoUpFolder(System.AppDomain.CurrentDomain.BaseDirectory) : Path.Combine(ExtSolutionGenerator.dataPath, "Managed");
                    var n = Application.isEditor ? Path.Combine(ExtSolutionGenerator.dataPath, "Library", "ScriptAssemblies") : Path.Combine(ExtSolutionGenerator.dataPath, "Managed");
                    foreach (var refer in projecta.ItemGroup[0].Reference)
                    {
                        if (refer.Include.Contains("Unity"))
                        {
                            refer.HintPath = refer.HintPath.Replace("{UnityDirectory}", d);
                        }
                    }
                    projecta.ItemGroup[0].Reference.Add(new Reference()
                    {
                        HintPath = Path.Combine(n, "Assembly-CSharp.dll"),
                        Include = "Assembly-CSharp"
                    });

                    projecta.ItemGroup[1].Compile.Add(new Compile()
                    {
                        Include = "Source/AllCodes.cs"
                    });

                    var actualProject = File.ReadAllText(csprojPath).XmlDeserializeFromString<ExtModProject>();
                    actualProject.ItemGroup[0].Reference = projecta.ItemGroup[0].Reference;

                    string projectData = ExtSolutionGenerator.ToXmlString(actualProject);
                    projectData = projectData.Replace("utf-16", "utf-8");
                    File.WriteAllText(csprojPath, projectData, System.Text.Encoding.UTF8);
                }
            }
            catch (System.Exception e)
            {
                ExtActionInspector.Log("Can't export out the assembly" , "ExtExporter", e.Message + e.StackTrace);
            }

            if (audioFile != null)
            {
                var path = Path.Combine(directory, "audio.ogg");
                File.WriteAllBytes(path, audioFile.GetBytes());
            }
        }
    }

    [System.Serializable]
    public class LiwbOptions
    {
        public bool compressAssembly;
        public bool compressResources;
        public bool compressAudio;
        public bool externalAudio;
        public bool embedAssemblyCodes;
        public bool embedCompiledAssembly;
    }

    [System.Serializable]
    public class LiwbAssembly
    {
        public bool isFileEmbedded;
        public LiwbFile assemblyFile;
        public bool isCodeEmbedded;
        public List<string> assemblyCodes = new List<string>();
    }

    [System.Serializable]
    public class LiwbFile
    {
        public string path;
        public int instanceID;
        public string base64;
        public bool isCompressed = false;
        public bool isExternal;

        public LiwbFile()
        {

        }

        public LiwbFile(string path, bool compress = false, bool external = false)
        {
            var info = LiwCustomObject.RegisterMeta(path);
            instanceID = info.id;
            this.path = Path.GetFileName(path);
            if (!external)
            {
                var bytes = File.ReadAllBytes(path);
                base64 = System.Convert.ToBase64String(compress ? bytes.Compress() : bytes);
                isCompressed = compress;
            }
            isExternal = external;
        }

        public byte[] GetBytes()
        {
            return isCompressed ? System.Convert.FromBase64String(base64).Decompress() : System.Convert.FromBase64String(base64);
        }
    }
}
