using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ExternMaker
{
    public class ExtProjectExplorer : MonoBehaviour
    {
        public Transform content;
        public GameObject directoryInstance;
        public ExtProjectStartup startup;

        public List<GameObject> projectInstances = new List<GameObject>();
        public List<string> projectDirectories = new List<string>();

        void OnEnable()
        {
            GetDirectories();
            ShowDirectories();
        }

        public void GetDirectories()
        {
            projectDirectories.Clear();
            foreach(var directory in Storage.GetDirectoriesLocal("Projects"))
            {
                if(File.Exists(Path.Combine(directory, "Project.liw")))
                {
                    projectDirectories.Add(directory);
                }
            }
        }

        public void ShowDirectories()
        {
            foreach(var inst in projectInstances)
            {
                Destroy(inst);
            }

            foreach(var directory in projectDirectories)
            {
                var info = Path.Combine(directory, "ProjectInfo.liwf");
                if (File.Exists(info))
                {
                    var obj = Instantiate(directoryInstance, content);
                    var button = obj.GetComponent<Button>();
                    var decInfo = LiwSerializer.yamlDeserializer.Deserialize<LiwProjectInfo>(File.ReadAllText(info));

                    button.onClick.AddListener(() =>
                    {
                        ExtProjectManager.instance.Open(directory);
                        gameObject.SetActive(false);
                        startup.gameObject.SetActive(false);
                    });

                    var projectName = obj.transform.Find("ProjectName");
                    var projectPath = obj.transform.Find("ProjectPath");
                    var projectOwner = obj.transform.Find("ProjectOwner");
                    projectName.GetComponent<Text>().text = decInfo.levelName;
                    projectPath.GetComponent<Text>().text = directory;
                    projectOwner.GetComponent<Text>().text = decInfo.authorName;
                    var deleteButton = obj.transform.Find("DeleteButton");
                    var deleteComp = deleteButton.GetComponent<Button>();
                    deleteComp.onClick.AddListener(() =>
                    {
                        ExtUtility.RecursiveDelete(new DirectoryInfo(directory));
                        Destroy(obj);
                        projectInstances.Remove(obj);
                    });
                    projectInstances.Add(obj);
                }
            }
        }
    }
}