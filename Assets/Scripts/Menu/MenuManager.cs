using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ExternMaker;

public class MenuManager : MonoBehaviour
{
	public static MenuManager instance;
    public ExtMonoUtility utility;

    public AudioSource source;
    public Color backColor = Color.blue;
    public Color foreColor = Color.blue;

    public List<LevelRequest> requests = new List<LevelRequest>();

    public List<GameObject> levelInstances = new List<GameObject>();

    public Transform instanceParent;
    public GameObject instancePrefab;

    public MenuLevelViewer levelViewer;
    public int levelViewerIndex = 6;

    public Text[] versionTexts;

    public static bool startupHandled;

    private void Start()
    {
        var res = SimpleFileBrowser.FileBrowser.CheckPermission();
        if (res != SimpleFileBrowser.FileBrowser.Permission.Granted)
        {
            SimpleFileBrowser.FileBrowser.RequestPermission();
        }
        try
        {
            Storage.CheckDirectoryLocal("Cache");
            Storage.CheckDirectoryLocal("Projects");
    		Storage.CheckDirectoryLocal("Levels");
            Storage.CheckDirectoryLocal("Replays");
        }
    	catch
    	{
    		Debug.LogError("Failed to create folders...");
    	}
    	instance = this;
        var monoUtil = ExtMonoUtility.myself ?? utility;
        requests.Clear();
        var files = new List<string>();
#if UNITY_ANDROID && !UNITY_EDITOR
        for (int i = 0; i < 4; i++)
        {
            var text = Resources.Load<TextAsset>("Levels/level" + i); // + ".bytes");
            Storage.WriteAllBytesLocal("Levels/level" + i + ".liwb", text.bytes);
        }
        var texte = Resources.Load<TextAsset>("DefProj"); // + ".bytes");
        Storage.WriteAllTextLocal("DefaultProject.xml", texte.text);
#endif
        foreach (var file in Storage.GetFilesLocal("Levels"))
        {
            if (Path.GetExtension(file).Contains("liwb"))
                files.Add(file);
        }
        var loop = monoUtil.TryRuntimeLoop(0, files.Count, 1);
        loop.onUpdate += i =>
        {
            var liwbProject = JsonUtility.FromJson<LiwbProject>(File.ReadAllText(files[i]));
            var instance = instancePrefab.Instantiate(instanceParent);
            var request = new LevelRequest()
            {
                approve = true,
                isLiwb = true,
                liwbPath = files[i]
            };
            instance.GetComponentInChildren<UnityEngine.UI.Text>().text = liwbProject.project.info.levelName;
            instance.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() =>
            {
                //CallRequest(i);
                levelViewer.OpenLevel(liwbProject, files[i]);
            });
            levelInstances.Add(instance);
            requests.Add(request);
        };
        var args = System.Environment.GetCommandLineArgs();
        if (args.Length > 1 && !startupHandled)
        {
            if(args[1] == "-liwb")
            {
                if (File.Exists(args[2]))
                {
                    Invoke("CallStartup", 0.5f);
                    startupHandled = true;
                }
            }
            else if (args[1] == "-proj")
            {
                if (Directory.Exists(args[2]))
                {
                    Invoke("CallStartup", 0.5f);
                    startupHandled = true;
                }
            }
            else if (Directory.Exists(args[1]))
            {
                //MessageBox.Show("We'll be opening your project in 3 seconds.", "Launcher");
                Invoke("CallStartup", 0.5f);
                startupHandled = true;
            }
        }

        foreach(var text in versionTexts)
        {
            text.text = "v" + Application.version;
        }
    }

    public void CallStartup()
    {
        var args = System.Environment.GetCommandLineArgs();
        if (args.Length > 1)
        {
            if (args[1] == "-liwb")
            {
                PlaymodeManager.request = new LevelRequest();
                PlaymodeManager.request.approve = true;
                PlaymodeManager.request.isLiwb = true;
                PlaymodeManager.request.liwbPath = args[2];
                MoveScene("PlaymodeScene");
            }
            else if (args[1] == "-proj")
            {
                ExtProjectManager.startupOpen = args[2];
                MoveScene("LevelEditor");
            }
            else if (Directory.Exists(args[1]))
            {
                //MessageBox.Show("We'll be opening your project in 3 seconds.", "Launcher");
                ExtProjectManager.startupOpen = args[1];
                MoveScene("LevelEditor");
            }
        }
        string argss = string.Join("\n", args);
        Storage.WriteAllTextLocal("args", argss);
    }

    public void MoveScene(string name)
    {
        LeanTween.value(1, 0, 1f).setOnUpdate((float val) =>
        {
            if (source != null)
            {
                source.volume = val;
            }
        });
        CrossSceneManager.LoadLevel(name, backColor, foreColor);
    }

    public void CallRequest(int index)
    {
        PlaymodeManager.request = requests[index];
        MoveScene("PlaymodeScene");
    }
    
    void OnDestroy()
    {
    	instance = null;
    }
}