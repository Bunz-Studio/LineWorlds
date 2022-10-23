using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class LevelManager : MonoBehaviour
{
    #region Variables
    [Header("Information")]
	public LevelInfo info;
	public LineMovement[] lines;
	public CheckpointManager checkpoint;
	[Header("Audio")]
	public AudioSource source;
	public float startOffset;
	public float endOffset;
	[Header("Events")]
	public UnityEvent OnLineStart;
	public UnityEvent OnLineKilled;
	public UnityEvent OnLineFinished;
	public UnityEvent OnLineRespawned;
    #endregion

    #region Accessors
    public CheckpointManager s_checkpoint
	{
		get
		{
			return checkpoint ?? FindObjectOfType<CheckpointManager>();
		}
	}
	public LineMovement[] s_lines
	{
		get
		{
			return lines.Length < 1 ? FindObjectsOfType<LineMovement>() : lines;
		}
	}
	public AudioSource s_source
	{
		get
		{
			return source ?? FindObjectOfType<AudioSource>();
		}
	}
    #endregion

    #region Unity Functions
    void Start()
	{
		lines = s_lines;
		checkpoint = s_checkpoint;
		source = s_source;
		
		foreach(var l in lines)
		{
			l.OnLineStart += OnLineStart.Invoke;
			l.OnLineKilled += OnLineKilled.Invoke;
			l.OnLineFinished += OnLineFinished.Invoke;
			l.OnLineRespawned += OnLineRespawned.Invoke;
		}
	}
    #endregion

    #region Audio
    public void TryLoadingAudio(string path, System.Action<AudioClip> onSuccess, System.Action onFailed)
    {
        StartCoroutine(LoadAudioFile(path, onSuccess, onFailed));
    }

    IEnumerator LoadAudioFile(string path, System.Action<AudioClip> onSuccess, System.Action onFailed)
    {
        UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.OGGVORBIS);
        yield return request.SendWebRequest();
        if (request.isNetworkError)
        {
            Debug.LogWarning(path + request.error);
            onFailed.TryInvoke();
        }
        else
        {
            AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
            clip.name = "audio";
            if (onSuccess != null) onSuccess.Invoke(clip);
        }
    }
    public void MoveScene(string name)
    {
        LeanTween.value(source.volume, 0, 1f).setOnUpdate((float val) =>
        {
            if (source != null)
            {
                source.volume = val;
            }
        });
        CrossSceneManager.LoadLevel(name);
    }
    #endregion
}

#region Classes
[System.Serializable]
public class LevelInfo
{
	public string levelID;

	public string levelName;
	public string levelAuthor;
	public string levelDescription;
	
	public string levelVersion;
	
	public string musicName;
	public string musicAuthor;
	
	public string difficulty;
	
	[Newtonsoft.Json.JsonIgnore]
	public string sceneName;

	[Newtonsoft.Json.JsonIgnore]
	public Texture levelPreview;

	public float GetProgress()
	{
		return PlayerPrefs.GetFloat("Progress|" + levelID, 0);
	}
	public void SetProgress(float value)
	{
		PlayerPrefs.SetFloat("Progress|" + levelID, value);
	}
}
#endregion