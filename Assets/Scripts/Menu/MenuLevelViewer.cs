using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using ExternMaker;
using ExternMaker.Serialization;

public class MenuLevelViewer : MonoBehaviour
{
    public MenuManager manager;

    public Text levelName;
    public Text musicName;
    public Text musicAuthor;

    public Transform replaysParent;
    public GameObject replayPrefab;
    public List<GameObject> replayPrefabs = new List<GameObject>();

    public static LiwbProject selectedProject;
    public static string selectedProjectPath;

    private void Start()
    {
        if(selectedProject != null)
        {
            OpenLevel(selectedProject, selectedProjectPath);
        }
    }

    public void OpenLevel(LiwbProject project, string path)
    {
        levelName.text = project.project.info.levelName;
        musicName.text = project.project.info.musicName;
        musicAuthor.text = project.project.info.musicArtist;

        foreach(var instance in replayPrefabs)
        {
            Destroy(instance);
        }
        replayPrefabs.Clear();

        foreach(var file in Storage.GetFilesLocal("Replays"))
        {
            if(file.Contains(project.project.info.levelName))
            {
                var instance = Instantiate(replayPrefab, replaysParent);
                instance.GetComponent<Button>().onClick.AddListener(() => {
                    var request = new LevelRequest()
                    {
                        approve = true,
                        isLiwb = true,
                        liwbPath = path,
                        replayPath = file
                    };
                    CallRequest(request);
                });
                instance.GetComponentInChildren<Text>().text = System.IO.Path.GetFileName(file);
                replayPrefabs.Add(instance);
            }
        }

        selectedProject = project;
        selectedProjectPath = path;
    }

    public void PlayLevel()
    {
        var request = new LevelRequest()
        {
            approve = true,
            isLiwb = true,
            liwbPath = selectedProjectPath
        };
        CallRequest(request);
    }

    public void MoveScene(string name)
    {
        LeanTween.value(1, 0, 1f).setOnUpdate((float val) =>
        {
            if (manager.source != null)
            {
                manager.source.volume = val;
            }
        });
        CrossSceneManager.LoadLevel(name, manager.backColor, manager.foreColor);
    }

    public void CallRequest(LevelRequest levelRequest)
    {
        PlaymodeManager.request = levelRequest;
        MoveScene("PlaymodeScene");
    }
}
