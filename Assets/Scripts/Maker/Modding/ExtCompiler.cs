using System;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_ANDROID
#else
using System.CodeDom.Compiler;
using Microsoft.CSharp;
#endif

namespace ExternMaker
{
    public static class ExtCompiler
    {
#region Main
        public static Assembly compiledAssembly;
        public static Type[] assemblyTypes;
        public static Dictionary<string, Type> assemblyDictionary = new Dictionary<string, Type>();
        public static List<Type> componentTypes = new List<Type>();
        public static List<Type> modTriggerTypes = new List<Type>();

        public static Action<Assembly> onAssemblyCompiled;
        public static Action<string> onAssemblyError;

        public static List<string> forbiddenCodes = new List<string>()
        {
            "C:\\",
            "C:/",
            "WebClient",
            "https://",
            "http://",
            "SpecialFolder.",
            "Process.",
            ".Download",
            ".Upload"
        };

        public static int compileAttempt = 0;
        public static Assembly Compile(string directory, ExtModProject project, bool asExe = false, bool forceByProcess = false)
        {
            var loadingPanel = LoadingPanelManager.CreatePanel("Compiling project: " + directory);
            var codes = new List<string>();
            var paths = new List<string>();
            var items = project.ItemGroup[1];

            if (items.Compile != null)
            {
                foreach (Compile item in items.Compile)
                {
                    string path = Path.Combine(directory, item.Include);
                    if (File.Exists(path))
                    {
                        if (!string.IsNullOrWhiteSpace(item.DependentUpon))
                        {
                            int index = LookForPath(paths, item.DependentUpon);
                            if (index > -1)
                                codes[index] += File.ReadAllText(path);
                        }
                        else
                        {
                            string build = File.ReadAllText(path);
                            codes.Add(build);
                            paths.Add(path);
                        }
                    }
                }
                List<string> forbiddenMatches = new List<string>();
                foreach (var a in codes)
                {
                    foreach (var fr in forbiddenCodes)
                    {
                        if (a.Contains(fr)) forbiddenMatches.Add(fr);
                    }
                }
                if (forbiddenMatches.Count > 0)
                {
                    ExtActionInspector.Log("Can't compile your code because it contains: " + string.Join(";", forbiddenMatches.ToArray()), "ExtCompiler:Guard");
                    return null;
                }
            }
            loadingPanel.infoLabel.text = "Executing compiler...";
            loadingPanel.SetProgress(0.2f);
            // Start the child process.
            Process p = new Process();
            // Redirect the output stream of the child process.
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = Storage.GetPathLocal("Compiler/MicroSharpCompiler.exe");
            p.StartInfo.Arguments = "\"" + directory + "\" \"" + project.PropertyGroup[0].AssemblyName + "\" " + compileAttempt;
            p.Start();
            // Do not wait for the child process to exit before
            // reading to the end of its redirected stream.
            // p.WaitForExit();
            // Read the output stream first and then wait.
            string output = p.StandardOutput.ReadToEnd();
            loadingPanel.infoLabel.text = "Reading compiler...";
            loadingPanel.SetProgress(0.5f);
            p.WaitForExit();
            UnityEngine.Debug.Log("Compiler output: " + output);
            if(output.Contains("AssemblyLocation"))
            {
                var location = output.Remove(output.Length - 2, 2).Replace("AssemblyLocation:", null);
                UnityEngine.Debug.Log("Trying to load assembly at: " + location + " - IsExist: " + File.Exists(location));
                var assembly = Assembly.LoadFile(location);
                compiledAssembly = assembly;
                AnalyzeAssembly();
                compileAttempt++;
                loadingPanel.infoLabel.text = "Finished compiling!";
                loadingPanel.SetProgress(1);
                UnityEngine.Object.Destroy(loadingPanel.gameObject);
                return assembly;
            }
            else
            {
                ExtActionInspector.Log("Failed to compile the assembly", "ExtCompiler", output);
                loadingPanel.infoLabel.text = "Errors found";
                loadingPanel.SetProgress(1);
                UnityEngine.Object.Destroy(loadingPanel.gameObject);
            }
            return null;
        }

        public static Assembly CompileLiwb(string directory, LiwbProject liwb)
        {
            var loadingPanel = LoadingPanelManager.CreatePanel("Compiling project: " + directory);
            
            List<string> forbiddenMatches = new List<string>();
            foreach (var a in liwb.assembly.assemblyCodes)
            {
                foreach (var fr in forbiddenCodes)
                {
                    if (a.Contains(fr)) forbiddenMatches.Add(fr);
                }
            }
            if (forbiddenMatches.Count > 0)
            {
                ExtActionInspector.Log("Can't compile your code because it contains: " + string.Join(";", forbiddenMatches.ToArray()), "ExtCompiler:Guard");
                return null;
            }
            var projectaa = Storage.ReadAllTextLocal("DefaultProject.xml").XmlDeserializeFromString<ExtModProject>();
            var d = Application.isEditor ? Storage.GoUpFolder(AppDomain.CurrentDomain.BaseDirectory) : Path.Combine(ExtSolutionGenerator.dataPath, "Managed");
            var n = Application.isEditor ? Path.Combine(ExtSolutionGenerator.dataPath, "Library", "ScriptAssemblies") : Path.Combine(ExtSolutionGenerator.dataPath, "Managed");
            foreach (var refer in projectaa.ItemGroup[0].Reference)
            {
                if (refer.Include.Contains("Unity"))
                {
                    refer.HintPath = refer.HintPath.Replace("{UnityDirectory}", d);
                }
            }
            projectaa.ItemGroup[0].Reference.Add(new Reference()
            {
                HintPath = Path.Combine(n, "Assembly-CSharp.dll"),
                Include = "Assembly-CSharp"
            });
            var codeData = new Codes();
            codeData.assemblyName = liwb.project.info.levelName;
            codeData.codes = liwb.assembly.assemblyCodes;
            codeData.references = new List<JsonProjectReference>();
            foreach (var refer in projectaa.ItemGroup[0].Reference)
            {
                codeData.references.Add(new JsonProjectReference(refer.RequiredTargetFramework, refer.Include, refer.HintPath));
            }
            Storage.WriteAllTextLocal("Cache/CompileRequest.json", JsonUtility.ToJson(codeData));

            loadingPanel.infoLabel.text = "Executing compiler...";
            loadingPanel.SetProgress(0.2f);
            // Start the child process.
            Process p = new Process();
            // Redirect the output stream of the child process.
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = Storage.GetPathLocal("Compiler/MicroSharpCompiler.exe");
            p.StartInfo.Arguments = "-liwb \"" + directory + "\" \"" + Storage.GetPathLocal("Cache/CompileRequest.json") + "\" " + compileAttempt;
            p.Start();
            // Do not wait for the child process to exit before
            // reading to the end of its redirected stream.
            // p.WaitForExit();
            // Read the output stream first and then wait.
            string output = p.StandardOutput.ReadToEnd();
            loadingPanel.infoLabel.text = "Reading compiler...";
            loadingPanel.SetProgress(0.5f);
            p.WaitForExit();
            // UnityEngine.Debug.Log("Compiler output: " + output);
            if (output.Contains("AssemblyLocation"))
            {
                var location = output.Remove(output.Length - 2, 2).Replace("AssemblyLocation:", null);
                // UnityEngine.Debug.Log("Trying to load assembly at: " + location + " - IsExist: " + File.Exists(location));
                var assembly = Assembly.LoadFile(location);
                compiledAssembly = assembly;
                AnalyzeAssembly();
                compileAttempt++;
                loadingPanel.infoLabel.text = "Finished compiling!";
                loadingPanel.SetProgress(1);
                UnityEngine.Object.Destroy(loadingPanel.gameObject);
                return assembly;
            }
            else
            {
                ExtActionInspector.Log("Failed to compile the assembly", "ExtCompiler", output);
                loadingPanel.infoLabel.text = "Errors found";
                loadingPanel.SetProgress(1);
                UnityEngine.Object.Destroy(loadingPanel.gameObject);
            }
            return null;
        }

        [Serializable]
        public class Codes
        {
            public string assemblyName;
            public List<JsonProjectReference> references = new List<JsonProjectReference>();
            public List<string> codes = new List<string>();
        }
        [Serializable]
        public class StringData
        {
            public string data;
        }

        public static System.Collections.IEnumerator CompileAsynchronously(string directory, ExtModProject project, bool asExe = false)
        {
            var codes = new List<string>();
            var paths = new List<string>();
            var items = project.ItemGroup[1];

            if (items.Compile != null)
            {
                foreach (Compile item in items.Compile)
                {
                    string path = Path.Combine(directory, item.Include);
                    if (File.Exists(path))
                    {
                        if (!string.IsNullOrWhiteSpace(item.DependentUpon))
                        {
                            int index = LookForPath(paths, item.DependentUpon);
                            if (index > -1)
                                codes[index] += File.ReadAllText(path);
                        }
                        else
                        {
                            string build = File.ReadAllText(path);
                            codes.Add(build);
                            paths.Add(path);
                        }
                    }
                }
                List<string> forbiddenMatches = new List<string>();
                foreach (var a in codes)
                {
                    foreach (var fr in forbiddenCodes)
                    {
                        if (a.Contains(fr)) forbiddenMatches.Add(fr);
                    }
                }
                if (forbiddenMatches.Count > 0)
                {
                    ExtActionInspector.Log("Can't compile your code because it contains: " + string.Join(";", forbiddenMatches.ToArray()), "ExtCompiler:Guard");
                    if (onAssemblyError != null) onAssemblyError.Invoke("Can't compile your code because it contains: " + string.Join(";", forbiddenMatches.ToArray()));
                    yield return null;
                }
            }
            var loadingPanel = LoadingPanelManager.CreatePanel("Compiling project: " + directory);
            loadingPanel.infoLabel.text = "Executing compiler...";
            loadingPanel.SetProgress(0.2f);
            // Start the child process.
            Process p = new Process();
            // Redirect the output stream of the child process.
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = Storage.GetPathLocal("Compiler/MicroSharpCompiler.exe");
            p.StartInfo.Arguments = "\"" + directory + "\" \"" + project.PropertyGroup[0].AssemblyName + "\" " + compileAttempt;
            p.Start();
            // Do not wait for the child process to exit before
            // reading to the end of its redirected stream.
            // p.WaitForExit();
            // Read the output stream first and then wait.
            string output = p.StandardOutput.ReadToEnd();
            loadingPanel.infoLabel.text = "Reading compiler...";
            loadingPanel.SetProgress(0.5f);
            p.WaitForExit();
            UnityEngine.Debug.Log("Compiler output: " + output);
            if (output.Contains("AssemblyLocation"))
            {
                var location = output.Remove(output.Length - 2, 2).Replace("AssemblyLocation:", null);
                UnityEngine.Debug.Log("Trying to load assembly at: " + location + " - IsExist: " + File.Exists(location));
                var assembly = Assembly.LoadFile(location);
                compiledAssembly = assembly;
                AnalyzeAssembly();
                compileAttempt++;
                loadingPanel.infoLabel.text = "Finished compiling!";
                loadingPanel.SetProgress(1);
                UnityEngine.Object.Destroy(loadingPanel.gameObject);
                if (onAssemblyCompiled != null) onAssemblyCompiled.Invoke(assembly);
                yield return null;
            }
            else
            {
                ExtActionInspector.Log("Failed to compile the assembly", "ExtCompiler", output);
                loadingPanel.infoLabel.text = "Errors found";
                loadingPanel.SetProgress(1);
                UnityEngine.Object.Destroy(loadingPanel.gameObject);
                if (onAssemblyError != null) onAssemblyError.Invoke(output);
            }
            yield return null;
        }

        public static void ResolveProjectReferences(string csprojPath)
        {
            // CSProj
            var project = Storage.ReadAllTextLocal("DefaultProject.xml").XmlDeserializeFromString<ExtModProject>();
            var d = Application.isEditor ? Storage.GoUpFolder(AppDomain.CurrentDomain.BaseDirectory) : Path.Combine(ExtSolutionGenerator.dataPath, "Managed");
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
            File.WriteAllText(csprojPath, projectData, System.Text.Encoding.UTF8);
        }
        
        public static Type TryGetType(string name)
        {
            try
            {
                var t = Type.GetType(name);
                if (t != null)
                {
                    return t;
                }
                else
                {
                    if (assemblyDictionary.ContainsKey(name)) return assemblyDictionary[name];
                }
            }
            catch
            {
                if (assemblyDictionary.ContainsKey(name)) return assemblyDictionary[name];
            }
            return null;
        }
        public static int LookForPath(List<string> list, string fileName)
        {
            for (int i = 0; i < list.Count; i++)
            {
                Console.WriteLine(list[i]);
                if (Path.GetFileName(list[i]) == fileName)
                    return i;
            }
            return -1;
        }
        public static void AnalyzeAssembly()
        {
            componentTypes.Clear();
            modTriggerTypes.Clear();
            assemblyDictionary.Clear();
            assemblyTypes = compiledAssembly.GetTypes();
            foreach (var a in assemblyTypes)
            {
                assemblyDictionary.Add(a.FullName, a);
                if (a.IsSubclassOf(typeof(MonoBehaviour)))
					componentTypes.Add(a);
                if (a.IsSubclassOf(typeof(LineWorldsMod.ModTrigger)))
                    modTriggerTypes.Add(a);
            }
        }
#endregion
#region Testing
        /*public static void TestMethods()
        {
            MethodInfo function = CreateFunction("x + 2 * y");
            var betterFunction = (Func<double, double, double>)Delegate.CreateDelegate(typeof(Func<double, double, double>), function);
            Func<double, double, double> lambda = (x, y) => x + 2 * y;

            DateTime start;
            DateTime stop;
            double result;
            const int repetitions = 5000000;

            start = DateTime.Now;
            for (int i = 0; i < repetitions; i++)
            {
                result = OriginalFunction(2, 3);
            }
            stop = DateTime.Now;
            Console.WriteLine("Original - time: {0} ms", (stop - start).TotalMilliseconds);

            start = DateTime.Now;
            for (int i = 0; i < repetitions; i++)
            {
                result = (double)function.Invoke(null, new object[] { 2, 3 });
            }
            stop = DateTime.Now;
            Console.WriteLine("Reflection - time: {0} ms", (stop - start).TotalMilliseconds);

            start = DateTime.Now;
            for (int i = 0; i < repetitions; i++)
            {
                result = betterFunction(2, 3);
            }
            stop = DateTime.Now;
            Console.WriteLine("Delegate - time: {0} ms", (stop - start).TotalMilliseconds);

            start = DateTime.Now;
            for (int i = 0; i < repetitions; i++)
            {
                result = lambda(2, 3);
            }
            stop = DateTime.Now;
            Console.WriteLine("Lambda - time: {0} ms", (stop - start).TotalMilliseconds);
        }*/

        public static double OriginalFunction(double x, double y)
        {
            return x + 2 * y;
        }

        /*public static MethodInfo CreateFunction(string function)
        {
            const string code = @"
                using System;
            
                namespace UserFunctions
                {                
                    public class BinaryFunction
                    {                
                        public static double Function(double x, double y)
                        {
                            return func_xy;
                        }
                    }
                }
            ";

            string finalCode = code.Replace("func_xy", function);

            var provider = new CSharpCodeProvider();
            CompilerResults results = provider.CompileAssemblyFromSource(new CompilerParameters(), finalCode);

            Type binaryFunction = results.CompiledAssembly.GetType("UserFunctions.BinaryFunction");
            return binaryFunction.GetMethod("Function");
        }*/
        public static bool IsFileLocked(FileInfo file)
        {
            try
            {
                using (FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                return true;
            }
            return false;
        }
        public static string GetFreeAssemblyPath(string dir, string name, int attempt)
        {
            string t = null;
            if (attempt > 0)
            {
                t = Path.Combine(dir, name + attempt.ToString() + ".dll");
                if (File.Exists(t))
                    if (IsFileLocked(new FileInfo(t)))
                        t = GetFreeAssemblyPath(dir, name, attempt + 1);
            }
            else
            {
                t = Path.Combine(dir, name + ".dll");
                if (File.Exists(t))
                    if (IsFileLocked(new FileInfo(t)))
                        t = GetFreeAssemblyPath(dir, name, attempt + 1);
            }
            return t;
        }
        public static string GetFreeExecutablePath(string dir, string name, int attempt)
        {
            string t = null;
            if (attempt > 0)
            {
                t = Path.Combine(dir, name + attempt.ToString() + ".exe");
                if (File.Exists(t))
                    if (IsFileLocked(new FileInfo(t)))
                        t = GetFreeAssemblyPath(dir, name, attempt + 1);
            }
            else
            {
                t = Path.Combine(dir, name + ".exe");
                if (File.Exists(t))
                    if (IsFileLocked(new FileInfo(t)))
                        t = GetFreeAssemblyPath(dir, name, attempt + 1);
            }
            return t;
        }

        public static void TryCopy(string path, string destination)
        {
            if (!File.Exists(destination))
            {
                File.Copy(path, destination);
            }
        }
#endregion
    }
}