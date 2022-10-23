using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using ExternMaker.Serialization;

namespace ExternMaker
{
    public class ExtModDialog : MonoBehaviour
    {
        [Header("Assembly Info")]
        public InputField assemblyPathField;

        [Header("Type List View")]
        public Transform typeListView;
        public GameObject typeListPrefab;
        public List<GameObject> typeListItems = new List<GameObject>();

        public void CompileAssembly()
        {
            List<ObjectSerialMatch> serializables = new List<ObjectSerialMatch>();

            var projectPath = ExtProjectManager.instance.project.info.levelName + ".csproj";
            projectPath = Path.Combine(ExtProjectManager.instance.directory, projectPath);
            ExtCompiler.ResolveProjectReferences(projectPath);
            var projectData = File.ReadAllText(projectPath).XmlDeserializeFromString<ExtModProject>();
            StartCoroutine(ExtCompiler.CompileAsynchronously(ExtProjectManager.instance.directory, projectData, false));

            ExtCompiler.onAssemblyCompiled += (asm) =>
            {
                //var asm = ExtCompiler.Compile(ExtProjectManager.instance.directory, projectData, false, true);
                assemblyPathField.text = asm == null ? "Error compiling" : asm.Location;

                if (asm == null) return;
                foreach (var item in typeListItems)
                {
                    Destroy(item);
                }
                typeListItems.Clear();

                foreach (var modTrig in FindObjectsOfType<LineWorldsMod.ModTrigger>())
                {
                    var converter = new Serializables.SerializedModTrigger();
                    converter.TakeValues(modTrig.gameObject);
                    var ser = new ObjectSerialMatch();
                    ser.obj = modTrig.gameObject;
                    ser.ser = converter;
                    ser.type = modTrig.GetType().FullName;
                    serializables.Add(ser);
                }

                foreach (var trig in serializables)
                {
                    Destroy(trig.obj.GetComponent<LineWorldsMod.ModTrigger>());
                    var type = ExtCompiler.TryGetType(trig.type);
                    if (type != null)
                    {
                        var comp = trig.obj.AddComponent(type);
                        trig.ser.ApplyTo(trig.obj);
                    }
                    else
                    {
                        ExtActionInspector.Log("Missing component: " + trig.type, "ExtCompiler:Reassingner");
                    }
                }
                foreach (var type in asm.GetTypes())
                {
                    var instance = Instantiate(typeListPrefab, typeListView);
                    instance.GetComponent<Button>().onClick.AddListener(() =>
                    {

                    });
                    instance.GetComponentInChildren<Text>().text = type.FullName;
                    typeListItems.Add(instance);
                }
                ExtCompiler.onAssemblyCompiled = null;
            };
        }

        public void ResolveProjectReferences()
        {
            var projectPath = ExtProjectManager.instance.project.info.levelName + ".csproj";
            projectPath = Path.Combine(ExtProjectManager.instance.directory, projectPath);
            ExtCompiler.ResolveProjectReferences(projectPath);
        }

        public void RecreateProject()
        {
            var csprojPath = ExtProjectManager.instance.project.info.levelName + ".csproj";
            csprojPath = Path.Combine(ExtProjectManager.instance.directory, csprojPath);
            if (File.Exists(csprojPath))
            {
                // CSProj
                var project = Storage.ReadAllTextLocal("DefaultProject.xml").XmlDeserializeFromString<ExtModProject>();
                var d = Application.isEditor ? Storage.GoUpFolder(System.AppDomain.CurrentDomain.BaseDirectory) : Path.Combine(ExtSolutionGenerator.dataPath, "Managed");
                var n = Application.isEditor ? Path.Combine(ExtSolutionGenerator.dataPath, "Library", "ScriptAssemblies") : Path.Combine(ExtSolutionGenerator.dataPath, "Managed");
                foreach (var refer in project.ItemGroup[0].Reference)
                {
                    if (refer.Include.Contains("Unity"))
                    {
                        refer.HintPath = refer.HintPath.Replace("{UnityDirectory}", d);
                    }
                }
                project.ItemGroup[0].Reference.Add(new Reference()
                {
                    HintPath = Path.Combine(n, "Assembly-CSharp.dll"),
                    Include = "Assembly-CSharp"
                });

                var actualProject = File.ReadAllText(csprojPath).XmlDeserializeFromString<ExtModProject>();
                actualProject.ItemGroup[0].Reference = project.ItemGroup[0].Reference;

                string projectData = ExtSolutionGenerator.ToXmlString(actualProject);
                projectData = projectData.Replace("utf-16", "utf-8");

                ExtSolutionGenerator.Generate(ExtProjectManager.instance.directory, actualProject);

                File.WriteAllText(csprojPath, projectData, System.Text.Encoding.UTF8);
            }
            else
            {
                ExtModProject.Generate(Storage.GetPathLocal("Projects"), ExtProjectManager.instance.project.info.levelName);
            }
        }

        public void OpenSolution()
        {
            var slnPath = ExtProjectManager.instance.project.info.levelName + ".sln";
            slnPath = Path.Combine(ExtProjectManager.instance.directory, slnPath);

            var sharpDevelopPath = Application.isEditor ? Storage.GetPathLocal("Build/Develop/bin/SharpDevelop.exe") : Storage.GetPathLocal("Develop/bin/SharpDevelop.exe");

            if (File.Exists(slnPath))
            {
                if (File.Exists(sharpDevelopPath))
                {
                    System.Diagnostics.Process.Start(sharpDevelopPath, "\"" + slnPath + "\"");
                }
                else
                {
                    System.Diagnostics.Process.Start(slnPath);
                }
            }
            else
            {
                ExtDialogManager.Alert("Solution doesn't exist");
            }
        }

        public void NotAsyncCompile()
        {
            List<ObjectSerialMatch> serializables = new List<ObjectSerialMatch>();
            foreach (var item in typeListItems)
            {
                Destroy(item);
            }
            typeListItems.Clear();

            var projectPath = ExtProjectManager.instance.project.info.levelName + ".csproj";
            projectPath = Path.Combine(ExtProjectManager.instance.directory, projectPath);
            ExtCompiler.ResolveProjectReferences(projectPath);
            var projectData = File.ReadAllText(projectPath).XmlDeserializeFromString<ExtModProject>();
            var asm = ExtCompiler.Compile(ExtProjectManager.instance.directory, projectData, false, true);
            assemblyPathField.text = asm == null ? "Error compiling" : asm.Location;

            if (asm == null) return;
            foreach (var modTrig in FindObjectsOfType<LineWorldsMod.ModTrigger>())
            {
                var converter = new Serializables.SerializedModTrigger();
                converter.TakeValues(modTrig.gameObject);
                var ser = new ObjectSerialMatch();
                ser.obj = modTrig.gameObject;
                ser.ser = converter;
                ser.type = modTrig.GetType().FullName;
                serializables.Add(ser);
            }

            foreach (var trig in serializables)
            {
                Destroy(trig.obj.GetComponent<LineWorldsMod.ModTrigger>());
                var type = ExtCompiler.TryGetType(trig.type);
                if (type != null)
                {
                    var comp = trig.obj.AddComponent(type);
                    trig.ser.ApplyTo(trig.obj);
                }
                else
                {
                    ExtActionInspector.Log("Missing component: " + trig.type, "ExtCompiler:Reassingner");
                }
            }
            foreach (var type in asm.GetTypes())
            {
                var instance = Instantiate(typeListPrefab, typeListView);
                instance.GetComponent<Button>().onClick.AddListener(() =>
                {

                });
                instance.GetComponentInChildren<Text>().text = type.FullName;
                typeListItems.Add(instance);
            }
        }

        public void ReloadFromCurrentAssembly()
        {
            List<ObjectSerialMatch> serializables = new List<ObjectSerialMatch>();
            foreach (var item in typeListItems)
            {
                Destroy(item);
            }
            typeListItems.Clear();

            var projectPath = ExtProjectManager.instance.project.info.levelName + ".csproj";
            var projectData = File.ReadAllText(Path.Combine(ExtProjectManager.instance.directory, projectPath)).XmlDeserializeFromString<ExtModProject>();
            var asm = ExtCompiler.compiledAssembly;

            if (asm == null) return;
            foreach (var modTrig in FindObjectsOfType<LineWorldsMod.ModTrigger>())
            {
                var converter = new Serializables.SerializedModTrigger();
                converter.TakeValues(modTrig.gameObject);
                var ser = new ObjectSerialMatch();
                ser.obj = modTrig.gameObject;
                ser.ser = converter;
                ser.type = modTrig.GetType().FullName;
                serializables.Add(ser);
            }

            foreach (var trig in serializables)
            {
                Destroy(trig.obj.GetComponent<LineWorldsMod.ModTrigger>());
                var type = ExtCompiler.TryGetType(trig.type);
                if (type != null)
                {
                    var comp = trig.obj.AddComponent(type);
                    trig.ser.ApplyTo(trig.obj);
                }
                else
                {
                    ExtActionInspector.Log("Missing component: " + trig.type, "ExtCompiler:Reassingner");
                }
            }
            foreach (var type in asm.GetTypes())
            {
                var instance = Instantiate(typeListPrefab, typeListView);
                instance.GetComponent<Button>().onClick.AddListener(() =>
                {

                });
                instance.GetComponentInChildren<Text>().text = type.FullName;
                typeListItems.Add(instance);
            }
        }

        public class ObjectSerialMatch
        {
            public string type;
            public GameObject obj;
            public ExtSerializables ser;
        }
    }
}
