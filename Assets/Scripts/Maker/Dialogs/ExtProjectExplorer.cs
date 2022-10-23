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
                var obj = Instantiate(directoryInstance, content);

                var button = obj.GetComponent<Button>();
                button.onClick.AddListener(() =>
                {
                    ExtProjectManager.instance.Open(directory);
                    gameObject.SetActive(false);
                    startup.gameObject.SetActive(false);
                });

                var deleteButton = obj.transform.Find("deleteButton");
                var deleteComp = deleteButton.GetComponent<Button>();
                deleteComp.onClick.AddListener(() =>
                {
                    ExtUtility.RecursiveDelete(new DirectoryInfo(directory));
                    Destroy(obj);
                    projectInstances.Remove(obj);
                });

                var text = obj.GetComponentInChildren<Text>();
                text.text = Path.GetFileName(directory);
                projectInstances.Add(obj);
            }
        }
    }
}