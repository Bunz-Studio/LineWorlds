using UnityEngine;
using ExternMaker;

public class PlaymodeManager : MonoBehaviour
{
    public static LevelRequest request;

    public LevelRequest defaultRequest = new LevelRequest();

    public ExtCore core;
    public ExtProjectManager projectManager;
    public ExtResourcesManager resourceManager;

    void Start()
    {
        if (request == null)
        {
            if (defaultRequest.approve)
            {
                OpenRequest(defaultRequest);
            }
        }
        else
        {
            if(request.approve) OpenRequest(request);
        }
        core.onPlaymodeChanged += state =>
        {
            if(state == EditorPlayState.Stopped)
            {
                var repManager = FindObjectOfType<ReplayManager>();
                if (repManager.isRecording)
                {
                    repManager.ResetRecording();
                }
                else
                {
                    repManager.ResetPlaying();
                }
                repManager.dontSaveMore = false;
            }
        };
    }

    public void OpenRequest(LevelRequest req)
    {
        core.StopGame();
        if(req.isLiwb)
        {
            projectManager.OpenLiwb(req.liwbPath);
        }
        else
        {
            projectManager.Open(req.directory);
        }

        var repManager = FindObjectOfType<ReplayManager>();
        if (!string.IsNullOrWhiteSpace(req.replayPath) && Storage.FileExists(req.replayPath))
        {
            repManager.OpenReplay(req.replayPath);
            repManager.isRecording = false;
            core.lineMovement.noTurns = true;
            core.lineMovement.immortal = true;
        }
        else
        {
            repManager.isRecording = true;
            core.lineMovement.noTurns = false;
            core.lineMovement.immortal = false;
        }
        //core.tempProject = null;
        core.PlayGame();
    }
}

[System.Serializable]
public class LevelRequest
{
    public bool approve;

    public bool isLiwb;
    public string liwbPath;

    public string directory;

    public string replayPath;
}
