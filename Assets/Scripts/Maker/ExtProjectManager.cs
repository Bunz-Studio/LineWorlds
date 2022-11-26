using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ExternMaker
{
    public class ExtProjectManager : MonoBehaviour
    {
        private static ExtProjectManager p_instance;
        public static ExtProjectManager instance
        {
            get
            {
                p_instance = ExtUtility.GetStaticInstance(p_instance);
                return p_instance;
            }
        }

        public ExtCore core;
        public ExtProjectExplorer projectExplorer;

        public static string exeDirectory
        {
            get
            {
                /*return ExtUtility.GetRidOfLastPath(
                    ExtUtility.GetRidOfLastPath(
                        ExtUtility.GetRidOfLastPath(System.Reflection.Assembly.GetExecutingAssembly().Location)
                        )
                    );*/
                return Storage.dataPath;
            }
        }

        public string directory;
        public static int compileAttempt;
        public LiwProject project = new LiwProject();
        public LiwbProject liwbProject = new LiwbProject();
        public ExtModProject modProject = new ExtModProject();
        public bool resourcesFromLiwb;

        public ExtProjectSettings settings;
        public ExtModDialog modDialog;
        public ExtAudioManager audioManager
        {
            get
            {
                return ExtCore.instance.audioManager;
            }
        }

        public static System.Action OnAfterLoadingProject;
        public int loadObjectsPerFrame = 25;
        public List<LiwCustomObject> customObjects
        {
            get
            {
                return ExtResourcesManager.instance.customObjects;
            }
            set
            {
                ExtResourcesManager.instance.customObjects = value;
            }
        }
        public int lastCustomObjectID
        {
            get
            {
                return ExtResourcesManager.instance.lastCustomObjectID;
            }
            set
            {
                ExtResourcesManager.instance.lastCustomObjectID = value;
            }
        }

        public List<Transform> selectedObjects = new List<Transform>();
        public static string startupOpen;

        public void Open(string directory)
        {
            StartCoroutine(LoadProjectAsync(directory));
        }

        IEnumerator LoadProjectAsync(string directory)
        {
            ExtSelection.instance.ClearTargets();
            var loadingPanel = LoadingPanelManager.CreatePanel("Reading the project file: " + directory);
            yield return new WaitForSecondsRealtime(0.2f);
            var proj = LiwSerializer.GetProject(directory);
            if (proj != null)
            {
                resourcesFromLiwb = false;
                //ExtHierarchy.instance.ClearItems();
                project = proj;
                this.directory = directory;

                ResetScene();

                loadingPanel.infoLabel.text = "Applying project settings";
                loadingPanel.SetProgress(0.2f);
                yield return new WaitForSecondsRealtime(0.2f);

                proj.renderSettings.ApplyTo(settings.gameObject);
                settings.projectInfo = proj.info;
                settings.UpdateInspectors();

                loadingPanel.infoLabel.text = "Compiling assemblies";
                loadingPanel.SetProgress(0.4f);
                yield return new WaitForSecondsRealtime(0.2f);

                var projectPath = project.info.levelName + ".csproj";
                projectPath = Path.Combine(directory, projectPath);
                if (File.Exists(projectPath))
                {
                    modProject = File.ReadAllText(projectPath).XmlDeserializeFromString<ExtModProject>();

                    try
                    {
                        modDialog.NotAsyncCompile();
                    }
                    catch (System.Exception e)
                    {
                        ExtActionInspector.Log("Failed to compile assembly: " + e.Message, "ExtCompiler", e.StackTrace);
                        try
                        {
                            List<string> allDLLFiles = new List<string>();
                            foreach (var dllFiles in Directory.GetFiles(Path.Combine(directory, "Library")))
                            {
                                if (dllFiles.EndsWith("dll"))
                                {
                                    allDLLFiles.Add(dllFiles);
                                }
                            }
                            ExtCompiler.compiledAssembly = System.Reflection.Assembly.LoadFile(allDLLFiles[allDLLFiles.Count - 1]);
                            ExtCompiler.AnalyzeAssembly();
                            modDialog.ReloadFromCurrentAssembly();
                        }
                        catch (System.Exception ee)
                        {
                            ExtActionInspector.Log("Failed to load pre-compiled assembly: " + ee.Message, "ExtCompiler", ee.StackTrace);
                        }
                    }
                }

                var lineMov = ExtCore.instance.lineMovement.GetComponent<ExtObject>();
                LiwSerializer.ApplyTo(lineMov, proj.lineInfo);
                
                //ExtHierarchy.instance.AddItems(objs);
                loadingPanel.infoLabel.text = "Loading resources";
                loadingPanel.SetProgress(0.6f);
                yield return new WaitForSecondsRealtime(0.2f);

                ExtResourcesManager.instance.RefreshResourceList();

                loadingPanel.infoLabel.text = "Spawning 0/" + proj.gameObjects.Count + " objects";
                loadingPanel.SetProgress(0.8f);
                yield return new WaitForSecondsRealtime(0.2f);

                var objs = new List<ExtObject>();
                int i = 0;
                int c = 0;
                foreach (var obj in proj.gameObjects)
                {
                    c++;
                    loadingPanel.infoLabel.text = "Spawning " + c + "/" + proj.gameObjects.Count + " objects";
                    var inst = LiwSerializer.SpawnObject(obj);
                    inst.transform.SetParent(ExtCore.instance.globalParent);
                    objs.Add(inst);
                    i++;
                    if (i >= loadObjectsPerFrame)
                    {
                        i = 0;
                        yield return new WaitForSecondsRealtime(0.01f);
                    }
                }

                foreach (var file in Directory.GetFiles(Path.Combine(directory, "Resources")))
                {
                    if (!file.Contains(".meta"))
                    {
                        var custom = new LiwCustomObject();
                        custom.LoadObject(file);
                    }
                }

                try
                {
                    var audioFile = Path.Combine(Path.Combine(exeDirectory, directory), "audio.ogg");
                    var mpegAudioFile = Path.Combine(Path.Combine(exeDirectory, directory), "audio.mp3");
                    // Debug.Log(audioFile);
                    if (Storage.FileExists(audioFile))
                    {
                        audioManager.TryImport(audioFile);
                    }
                    else if(Storage.FileExists(mpegAudioFile))
                    {
                        audioManager.TryImport(mpegAudioFile);
                    }
                    else
                    {
                        audioManager.originalAudioPath = null;
                    }

                }
                catch (System.Exception e)
                {
                    ExtActionInspector.Log("Failed importing audio when opening project: " + e.Message, "ExtAudioImporter", e.StackTrace);
                }

                loadingPanel.infoLabel.text = "Fixing inspector";
                yield return new WaitForSecondsRealtime(0.15f);
                ExtSelection.instance.transformGizmo.AddTarget(ExtCore.instance.lineMovement.transform);
                ExtSelection.instance.ClearTargets();
                yield return new WaitForSecondsRealtime(0.2f);
                loadingPanel.infoLabel.text = "Finished loading project";
                Destroy(loadingPanel.gameObject);
                yield return new WaitForSecondsRealtime(0.2f);

                OnAfterLoadingProject.InvokeOnExist();
                OnAfterLoadingProject = null;
            }
            yield return null;
        }

        public void AddAudio(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                var ext = Path.GetExtension(path);
                string check = ext.Contains("mp3") ? "audio.ogg" : "audio.mp3";
                if(Storage.FileExists(Path.Combine(directory, check)))
                {
                    Storage.Delete(Path.Combine(directory, check));
                }
                var bytes = Storage.ReadAllBytes(path);
                Storage.WriteAllBytes(Path.Combine(directory, "audio" + ext), bytes);
            }
        }

        public void OpenLiwb(string filePath)
        {
            var liwbCls = JsonUtility.FromJson<LiwbProject>(File.ReadAllText(filePath));
            if (liwbCls != null)
            {
                resourcesFromLiwb = true;
                liwbProject = liwbCls;
                project = liwbProject.project;

                ResetScene();

                var lineMov = ExtCore.instance.lineMovement.GetComponent<ExtObject>();
                LiwSerializer.ApplyTo(lineMov, project.lineInfo);

                try
                {
                    if(liwbCls.assemblyFile != null)
                    {
                        if(liwbCls.project.info.gameVersion != "LW" + Application.version)
                        {
                            ExtDialogManager.Alert("The version is mismatching\nAssembly may not load");
                        }
                        var bytes = liwbCls.assemblyFile.GetBytes();
                        if (bytes != null)
                        {
                            ExtCompiler.compiledAssembly = System.Reflection.Assembly.Load(bytes);
                            if (ExtCompiler.compiledAssembly != null)
                            {
                                modDialog.ReloadFromCurrentAssembly();
                                compileAttempt++;
                            }
                            else
                            {
                                ExtActionInspector.Log("Assembly isn't loaded without any warning", "ExtCompiler");
                            }
                        }
                    }
                }
                catch (System.Exception e)
                {
                    if (liwbCls.assembly.isCodeEmbedded)
                    {
                        ExtActionInspector.Log("Failed to load assembly, trying to compile from the embedded code: " + e.Message, "ExtCompiler", e.Message + e.StackTrace);
                        ExtCompiler.compiledAssembly = ExtCompiler.CompileLiwb(Storage.dataPath, liwbCls);
                        if (ExtCompiler.compiledAssembly != null)
                        {
                            try
                            {
                                modDialog.ReloadFromCurrentAssembly();
                            }
                            catch (System.Exception ee)
                            {
                                ExtActionInspector.Log("Analyzing error: " + ee.Message, "ExtCompiler", ee.StackTrace);
                            }
                            compileAttempt++;
                        }
                        else
                        {
                            ExtActionInspector.Log("Assembly failed to also compile...", "ExtCompiler");
                        }
                    }
                    else
                    {
                        ExtActionInspector.Log("Failed to load assembly: " + e.Message, "ExtCompiler", e.Message + e.StackTrace);
                    }
                }

                ExtResourcesManager.instance.ClearObjects();
                ExtResourcesManager.instance.ClearResourceList();
                foreach (var liwbFile in liwbProject.resourceFiles)
                {
                    ExtResourcesManager.instance.RegisterObject(liwbFile);
                }

                var objs = new List<ExtObject>();
                foreach (var obj in project.gameObjects)
                {
                    var inst = LiwSerializer.SpawnObject(obj);
                    inst.transform.SetParent(ExtCore.instance.globalParent);
                    objs.Add(inst);
                }

                try
                {
                    if (liwbProject.audioFile != null) audioManager.TryImport(liwbProject.audioFile); else audioManager.originalAudioPath = null;
                }
                catch (System.Exception e)
                {
                    ExtActionInspector.Log("Failed importing audio when opening project: " + e.Message, "ExtAudioImporter", e.StackTrace);
                }

                project.renderSettings.ApplyTo(settings.gameObject);
                /*
                settings.projectInfo = project.info;
                settings.UpdateInspectors();
                */
                if (liwbProject.project.info.gameVersion.Contains("Arphros")) 
					ExtCore.instance.lineMovement.turns = new Vector3[]{
						new Vector3(0, 90, 0),
						new Vector3(0, 0, 0)
					};
                OnAfterLoadingProject.InvokeOnExist();
                OnAfterLoadingProject = null;
            }
        }

        public void Save()
        {
            if(ExtCore.playState != EditorPlayState.Stopped)
            {
                ExtDialogManager.Alert("Project cannot be saved while playing/pausing");
                return;
            }
            selectedObjects = new List<Transform>(ExtSelection.instance.transformSelection);
            ExtSelection.instance.ClearTargets();
            SerializeScene();
            try
            {
                if (!string.IsNullOrEmpty(audioManager.originalAudioPath))
                {
                    var ext = Path.GetExtension(audioManager.originalAudioPath);
                    var bytes = File.ReadAllBytes(audioManager.originalAudioPath);
                    File.WriteAllBytes(Path.Combine(directory, "audio" + ext), bytes);
                    project.info.musicFile = "audio" + ext;
                }
            }
            catch (System.Exception e)
            {
                ExtActionInspector.Log("Failed putting audio when saving project: " + e.Message, "ExtAudioImporter", e.StackTrace);
            }
            LiwSerializer.CreateProject(directory, project);
            ExtSelection.instance.transformSelection = selectedObjects;
            ExtDialogManager.Alert("Project saved");
        }

        public void SaveAs()
        {
            Save();
        }

        public void ResetScene()
        {
            ExtCore.Reset();
            ExtResourcesManager.instance.ClearObjects();
            var objects = ExtCore.instance.globalParent.GetComponentsInChildren<ExtObject>(true);
            foreach (var obj in objects)
            {
                Destroy(obj.gameObject);
            }
            if(ExtSelection.instance != null) ExtSelection.instance.ClearTargets();
        }

        public void SerializeScene()
        {
            var proj = GetProject();
            proj.info = settings.projectInfo;
            project = proj;
        }

        public LiwProject GetProject()
        {
            var proj = new LiwProject();

            var objects = ExtCore.instance.globalParent.GetComponentsInChildren<ExtObject>(true);

            proj.renderSettings = new Serialization.Serializables.SerializedRenderSettings(settings.gameObject);

            var mov = ExtCore.instance.lineMovement;
            proj.lineInfo = LiwSerializer.SerializeGameObject(mov.GetComponent<ExtObject>());
            proj.cameraInfo = LiwSerializer.SerializeGameObject(ExtCore.instance.playmodeCamera.GetComponent<ExtObject>());

            proj.gameObjects.Clear();
            foreach (var obj in objects)
            {
                proj.gameObjects.Add(LiwSerializer.SerializeGameObject(obj));
            }
            
            proj.info.gameVersion = "LW" + Application.version;

            return proj;
        }

        public void ApplyProjectToScene(LiwProject project, bool now = false)
        {
            var lineMov = ExtCore.instance.lineMovement.GetComponent<ExtObject>();
            var playCamera = ExtCore.instance.playmodeCamera.GetComponent<ExtObject>();
            LiwSerializer.ApplyTo(lineMov, project.lineInfo);
            LiwSerializer.ApplyTo(playCamera, project.cameraInfo);

            foreach (var obj in project.gameObjects)
            {
                var inst = ExtCore.GetObject(obj.instanceID);
                if (inst != null)
                {
                    LiwSerializer.ApplyTo(inst, obj, now);
                }
                else
                {
                    ExtActionInspector.Log("Object didn't found", "Project Manager", "Object didn't found when trying to re-assign values", "ID: " + obj.instanceID);
                    //LiwSerializer.ApplyTo(inst, obj, now);
                }
            }

            project.renderSettings.ApplyTo(settings.gameObject);
        }

        private void Start()
        {
            p_instance = this;
            Invoke("StartupCheck", 1);
        }
        bool pState;
        private void Update()
        {
            if (ExtCore.instance != null)
            {
                // bool state = ExtCore.playState == EditorPlayState.Playing ? settings.enablePlaymodeFog : settings.enabledFogInEditing ? settings.enablePlaymodeFog : false;
                if (ExtCore.playState == EditorPlayState.Playing)
                {
                    RenderSettings.fog = settings.enablePlaymodeFog;
                }
                else
                {
                    if(settings.enabledFogInEditing)
                    {
                        RenderSettings.fog = settings.enablePlaymodeFog;
                    }
                    else
                    {
                        RenderSettings.fog = false;
                    }
                }
            }
        }

        public void StartupCheck()
        {
            if (!string.IsNullOrWhiteSpace(startupOpen))
            {
                if (Directory.Exists(startupOpen))
                {
                    Open(startupOpen);
                    projectExplorer.gameObject.SetActive(false);
                    projectExplorer.startup.gameObject.SetActive(false);
                }
            }
            startupOpen = null;
        }

        private void OnDestroy()
        {
            p_instance = null;
        }
    }
}
