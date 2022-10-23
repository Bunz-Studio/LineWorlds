using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExternMaker;

public class ReplayManager : MonoBehaviour
{
    public ReplayData currentData = new ReplayData();

    public ReplayData OpenReplay(string path)
    {
        var text = Storage.ReadAllText(path);
        var data = JsonUtility.FromJson<ReplayData>(text);
        if (data != null)
        {
            currentData = data;
            return data;
        }
        return null;
    }

    public bool isRecording;
    public bool startRecording;
    public bool startPlaying;

    public float spentTime;
    public int timingIndex;

    public bool dontSaveMore;

    LineMovement mov;

    private void Start()
    {
        mov = FindObjectOfType<LineMovement>();
        mov.OnLineStart += OnLineStarted;
        mov.OnLineTurns += OnLineTurned;
        mov.OnLineFinished += OnLineFinished;
    }

    public void OnLineStarted()
    {
        if (isRecording)
            startRecording = true;
        else
            startPlaying = true;
    }

    public void OnLineTurned()
    {
        if(startRecording)
        {
            currentData.timings.Add(spentTime);
            currentData.places.Add(mov.transform.position);
        }
    }

    public void OnLineFinished()
    {
        if(startRecording && !dontSaveMore)
        {
            Storage.CheckDirectoryLocal("Replays");
            if (ExtCore.instance != null)
            {
                var repData = JsonUtility.ToJson(currentData);
                Storage.WriteAllText(GetFreeReplayPath(Storage.GetPathLocal("Replays"), ExtProjectManager.instance.project.info.levelName, 0), repData);
                dontSaveMore = true;
            }
        }
    }

    public static string GetFreeReplayPath(string dir, string name, int attempt)
    {
        string t = null;
        t = attempt > 0 ? Path.Combine(dir, name + attempt.ToString() + ".repl") : Path.Combine(dir, name + ".repl");
        if (File.Exists(t))
            t = GetFreeReplayPath(dir, name, attempt + 1);
        return t;
    }

    public void ResetRecording()
    {
        spentTime = 0;
        currentData = new ReplayData();
        startRecording = false;
    }

    public void ResetPlaying()
    {
        spentTime = 0;
        timingIndex = 0;
        startPlaying = false;
    }

    private void Update()
    {
        if (startRecording)
        {
            spentTime += Time.deltaTime;
        }
        else
        {
            if (startPlaying)
            {
                spentTime += Time.deltaTime;
                if (timingIndex < currentData.timings.Count)
                {
                    if (spentTime + Time.deltaTime > currentData.timings[timingIndex])
                    {
                        TurnLineForce();
                    }
                }
            }
        }
    }

    public void TurnLineForce()
    {
        mov.AdjustLine(currentData.places[timingIndex]);
        mov.InputLine();
        timingIndex++;
        bool t = timingIndex < currentData.timings.Count && spentTime + Time.deltaTime > currentData.timings[timingIndex];
        if (t) TurnLineForce();
    }
}

[System.Serializable]
public class ReplayData
{
    public List<float> timings = new List<float>();
    public List<Vector3> places = new List<Vector3>();
}
