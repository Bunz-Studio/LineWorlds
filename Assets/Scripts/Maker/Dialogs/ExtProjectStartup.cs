using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ExternMaker
{
    public class ExtProjectStartup : MonoBehaviour
    {
        public ExtProjectSettings projectSettings;
        public ExtProjectManager projectManager;

        public ExtProjectTemplate[] templates;
        public Image[] templatesBasedUI;

        public Color normalTemplateColor;
        public Color selectedTemplateColor;

        public GameObject[] starterObjects;

        public int selectedTemplate;

        public LiwProjectInfo projectInfo = new LiwProjectInfo();
        public List<ProjectInfoFieldInspect> projectInfoFieldInspects = new List<ProjectInfoFieldInspect>();
        
        private void Start()
        {
            foreach (var insp in projectInfoFieldInspects)
            {
                insp.extFieldInspect.sources = new List<object>(new object[] { projectInfo });
                insp.extFieldInspect.fieldFinishEdit += (aa) =>
                {
                    projectManager.project.info = projectInfo;
                };
                insp.Initialize();
            }

            UpdateInspectors();
        }

        public void UpdateInspectors()
        {
            foreach (var insp in projectInfoFieldInspects)
            {
                insp.extFieldInspect.ApplyTemp();
            }
        }

        public void CreateNew()
        {
            CreateWithTemplate(selectedTemplate);
        }

        public void SelectTemplate(int index)
        {
            selectedTemplate = index;
            for(int i = 0; i < templatesBasedUI.Length; i++)
            {
                templatesBasedUI[i].color = index == i ? selectedTemplateColor : normalTemplateColor;
            }
        }

        public void CreateWithTemplate(int index = 0)
        {
            var path = Path.Combine(Storage.GetPathLocal("Projects"), projectInfo.levelName);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                ConfirmCreate(path, index);
            }
            else
            {
                MessageBox.ShowNondefault("A project with the same name already exists in your projects directory, do you want to replace it? (Your previous project will be lost!)", "Warning!",
                    new MessageBox.MessageBoxButton[] {
                        new MessageBox.MessageBoxButton("Yes", true, () => {
                            ConfirmCreate(path, index);
                        }),
                        new MessageBox.MessageBoxButton("No", true)}, false
                    );
            }
        }

        public void ConfirmCreate(string path, int index = 0)
        {
            projectManager.ResetScene();
            projectManager.directory = path;

            projectSettings.backgroundColor = new Color(15 / 255, 15 / 255, 15 / 255, 1);
            RenderSettings.fog = false;
            RenderSettings.fogDensity = 0.03f;
            RenderSettings.fogColor = new Color(15 / 255, 15 / 255, 15 / 255, 1);

            projectSettings.gameObject.SetActive(true);

            projectSettings.projectInfo = projectInfo;
            projectManager.project = new LiwProject();
            projectManager.project.info = projectInfo;
            projectSettings.UpdateInspectors();

            ExtCore.instance.lineMovement.GetComponent<ExtObject>().Initialize();
            ExtCore.instance.lineMovement.rend.sharedMaterial.color = new Color(1, 1, 1, 1);
            templates[index].onCreate.Invoke();
            LiwSerializer.CreateProject(path, projectManager.project);
            foreach (var obj in templates[index].objects)
            {
                var obji = ExtCore.instance.CreateObject(obj);
                obji.transform.position = obj.transform.position;
            }
            projectManager.Save();

            ExtModProject.Generate(Storage.GetPathLocal("Projects"), projectManager.project.info.levelName);

            gameObject.SetActive(false);
        }
    }

    [System.Serializable]
    public class ExtProjectTemplate
    {
        public string name;
        public Texture thumbnail;
        public GameObject[] objects;
        public UnityEngine.Events.UnityEvent onCreate;
    }
}