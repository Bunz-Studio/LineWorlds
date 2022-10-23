using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using SimpleFileBrowser;
using System.IO;

namespace ExternMaker
{
    public class ExtExportDialog : MonoBehaviour
    {
        public LevelConverter.ArphrosInformation information = new LevelConverter.ArphrosInformation();
        public LiwbOptions liwbOptions = new LiwbOptions();
        public List<ArphrosInfoFieldInspect> arphrosInfoFieldInspects = new List<ArphrosInfoFieldInspect>();
        public List<LiwbOptionsFieldInspect> liwbOptionsFieldInspects = new List<LiwbOptionsFieldInspect>();

        private void Start()
        {
            // Project Info Fields
            foreach (var insp in arphrosInfoFieldInspects)
            {
                insp.extFieldInspect.sources = new List<object>(new object[] { information });
                insp.Initialize();
            }
            foreach (var insp in liwbOptionsFieldInspects)
            {
                insp.extFieldInspect.sources = new List<object>(new object[] { liwbOptions });
                insp.Initialize();
            }

            UpdateInspectors();
        }

        public void UpdateInspectors()
        {
            foreach (var insp in arphrosInfoFieldInspects)
            {
                insp.extFieldInspect.sources = new List<object>(new object[] { information });
                insp.extFieldInspect.ApplyTemp();
            }
            foreach (var insp in liwbOptionsFieldInspects)
            {
                insp.extFieldInspect.sources = new List<object>(new object[] { liwbOptions });
                insp.extFieldInspect.ApplyTemp();
            }
        }

        public void TryBrowseExport()
        {
            FileBrowser.Skin = ExtResourcesManager.instance.browserSkin;
            FileBrowser.SetFilters(true);
            FileBrowser.ShowLoadDialog(
                (val) =>
                {
                    if(val.Length > 0) ExportToDirectory(val[0]);
                },
                () =>
                {
                },
                FileBrowser.PickMode.Folders,
                false,
                null,
                "",
                "Select the folder to export"
            );
        }

        public void TryBrowseExportLiwb()
        {
            var liwbFilter = new FileBrowser.Filter("Liwb", ".liwb");
            FileBrowser.Skin = ExtResourcesManager.instance.browserSkin;
            FileBrowser.SetFilters(true, liwbFilter);
            FileBrowser.SetDefaultFilter(".liwb");
            FileBrowser.ShowSaveDialog(
                (val) =>
                {
                    if (val.Length > 0) ExportToLiwb(val[0]);
                },
                () =>
                {
                },
                FileBrowser.PickMode.Files,
                false,
                null,
                "",
                "Select path"
            );
        }

        public void ExportToDirectory(string path)
        {
            try
            {
                LevelConverter.SaveLevel(path, information);
                var resFolder = Path.Combine(path, "Resources");
                LiwSerializer.CheckDirectory(resFolder);
                ExtResourcesManager.instance.CopyResourcesTo(resFolder);
                var originalAudio = Path.Combine(ExtProjectManager.instance.directory, "audio.ogg");
                if (File.Exists(originalAudio))
                {
                    var targetAudioPath = Path.Combine(path, "song.ogg");
                    File.WriteAllBytes(targetAudioPath, File.ReadAllBytes(originalAudio));
                }
            }
            catch (System.Exception e)
            {
                ExtActionInspector.Log("Failed when exporting..." , "Project Exporter", "Message: " + e.Message, "StackTrace: " + e.StackTrace);
            }
        }

        public void ExportToLiwb(string path)
        {
            try
            {
                var project = LiwbSerializer.CompileProject(path, ExtProjectManager.instance.directory, liwbOptions);
                if (liwbOptions.externalAudio)
                {
                    var musPath = ExtProjectManager.instance.audioManager.originalAudioPath;
                    project.audioFile.path = Path.GetFileNameWithoutExtension(path) + ".ogg";
                    File.WriteAllBytes(Path.Combine(Storage.GoUpFolder(path), project.audioFile.path), File.ReadAllBytes(musPath));
                }
                File.WriteAllText(path, JsonUtility.ToJson(project));
            }
            catch (System.Exception e)
            {
                ExtActionInspector.Log("Failed when exporting...", "Project Exporter", "Message: " + e.Message, "StackTrace: " + e.StackTrace);
            }
        }
        
        public void ConvertPOPYLToLiwb()
        {
            var popyFilter = new FileBrowser.Filter("Arphros", ".popyl");
            FileBrowser.Skin = ExtResourcesManager.instance.browserSkin;
            FileBrowser.SetFilters(true, popyFilter);
            FileBrowser.SetDefaultFilter(".popyl");
            FileBrowser.ShowLoadDialog(
                (val) =>
                {
                    if (val.Length <= 0) return;
                    var liwbFilter = new FileBrowser.Filter("Liwb", ".liwb");
                    FileBrowser.Skin = ExtResourcesManager.instance.browserSkin;
                    FileBrowser.SetFilters(true, liwbFilter);
                    FileBrowser.SetDefaultFilter(".liwb");
                    FileBrowser.ShowSaveDialog(
                        (vala) =>
                        {
                            if (vala.Length > 0)
                            {
                                Levelprop.LevelMainClass lvl = JsonUtility.FromJson<Levelprop.LevelMainClass>(File.ReadAllText(Path.Combine(val[0])));
                                lvl.popylPath = val[0];
                                lvl.ToLiwb(vala[0]);
                            }
                        },
                        () =>
                        {
                        },
                        FileBrowser.PickMode.Files,
                        false,
                        null,
                        "",
                        "Select liwb path"
                    );
                },
                () =>
                {
                },
                FileBrowser.PickMode.Files,
                false,
                null,
                "",
                "Select popyl path"
            );
        }
        
        public void ConvertLiwbToProject()
        {
            var popyFilter = new FileBrowser.Filter("Liwb", ".liwb");
            FileBrowser.Skin = ExtResourcesManager.instance.browserSkin;
            FileBrowser.SetFilters(true, popyFilter);
            FileBrowser.SetDefaultFilter(".liwb");
            FileBrowser.ShowLoadDialog(
                (val) =>
                {
                    if (val.Length <= 0) return;
                    FileBrowser.Skin = ExtResourcesManager.instance.browserSkin;
                    FileBrowser.ShowSaveDialog(
                        (vala) =>
                        {
                            if (vala.Length > 0)
                            {
                            	var liwb = JsonUtility.FromJson<LiwbProject>(File.ReadAllText(val[0]));
                            	liwb.ToLiwProject(vala[0]);
                                /*Levelprop.LevelMainClass lvl = JsonUtility.FromJson<Levelprop.LevelMainClass>(File.ReadAllText(Path.Combine(val[0])));
                                lvl.popylPath = val[0];
                                lvl.ToLiwb(vala[0]);*/
                            }
                        },
                        () =>
                        {
                        },
                        FileBrowser.PickMode.Folders,
                        false,
                        null,
                        "",
                        "Select project path"
                    );
                },
                () =>
                {
                },
                FileBrowser.PickMode.Files,
                false,
                null,
                "",
                "Select liwb path"
            );
        }

        [System.Serializable]
        public class ArphrosInfoFieldInspect : ExtInspectableUnite.InspectField<LevelConverter.ArphrosInformation>
        {
        }
        [System.Serializable]
        public class LiwbOptionsFieldInspect : ExtInspectableUnite.InspectField<LiwbOptions>
        {
        }
    }
}