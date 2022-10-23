using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

public class MidiSync : MonoBehaviour
{
    private BeatTiming midiFile;
    public LineMovement mov;
    public AudioSource source;
    public bool autoplayLineWithMidi;
    public string midiFilePath;

    public float[] timingsFound;
    public List<Vector3> positions = new List<Vector3>();
    public int timingIndex;

    void Start()
    {
        ReadFromFile();
    }

    void Update()
    {
        if (autoplayLineWithMidi)
        {
            if (timingIndex < timingsFound.Length)
            {
                if (source.time + Time.deltaTime > timingsFound[timingIndex])
                {
                    TurnLineForce();
                }
            }
        }
    }

    public void TurnLineForce()
    {
        mov.AdjustLine(positions[timingIndex]);
        mov.InputLine();
        timingIndex++;
        bool t = timingIndex < timingsFound.Length && source.time + Time.deltaTime > timingsFound[timingIndex];
        if (t) TurnLineForce();
    }

    private void ReadFromFile()
    {
        midiFile = JsonConvert.DeserializeObject<BeatTiming>(System.IO.File.ReadAllText(midiFilePath));
        GetDataFromMidi();
    }
    public void GetDataFromMidi()
    {
        var timings = new List<float>();
        foreach (var note in midiFile.timings)
        {
            timings.Add(note);
        }
        timingsFound = RemoveEqualTimings(timings.ToArray());
        Array.Sort(timingsFound);
    }
    public static float[] RemoveEqualTimings(float[] origin)
    {
        var newTimings = new List<float>();
        foreach (var num in origin)
        {
            if (!newTimings.Contains(num))
                newTimings.Add(num);
        }
        return newTimings.ToArray();
    }
    #region Algorithm
    public List<Result> results = new List<Result>();
    [System.Serializable]
    public class Result
    {
        [Header("Direction")]
        public Vector3 eulerAngle;
        public Vector3 direction;
        [Header("Point")]
        public Vector3 startPoint;
        public Vector3 endPoint;
        [Header("Result")]
        public Vector3 pos;
        public Vector3 scale;
    }
    public void GenerateLines()
    {
        if (timingsFound.Length <= 0) ReadFromFile();
        var line = mov;
        var timings = timingsFound;
        var speed = mov.lineSpeed;
        if (mov != null)
        {
            var tailParent = transform.Find("TailParent");
            if (tailParent == null)
            {
                tailParent = new GameObject().transform;
                tailParent.SetParent(transform);
                tailParent.gameObject.name = "TailParent";
            }
            var centerTailParent = transform.Find("CenterTailParent");
            if (centerTailParent == null)
            {
                centerTailParent = new GameObject().transform;
                centerTailParent.SetParent(transform);
                centerTailParent.gameObject.name = "CenterTailParent";
            }
            var autoTailParent = transform.Find("AutoTriggerParent");
            if (autoTailParent == null)
            {
                autoTailParent = new GameObject().transform;
                autoTailParent.SetParent(transform);
                autoTailParent.gameObject.name = "AutoTriggerParent";
            }
            var mat = mov.GetComponent<MeshRenderer>().sharedMaterial;
            results.Clear();
            positions.Clear();
            int turn = mov.turnIndex;
            Vector3 lastPosition = line.transform.position;
            for (int i = 0; i < timings.Length; i++)
            {
                float timing = 0;
                float nextTiming = timings[i];
                if (i - 1 < 0) timing = 0; else timing = timings[i - 1];
                float size = speed * Mathf.Abs(nextTiming - timing);
                Vector3 direction = Vector3.zero;
                var angle = Vector3.zero;
                angle = mov.turns[turn];
                string tailTag = "Turn" + (turn + 1).ToString();
                direction = Singularite(angle);
                var result = GetLine(lastPosition, lastPosition + (size * direction), mov.transform.localScale);
                var res = new Result();
                res.startPoint = lastPosition;
                res.eulerAngle = angle;
                res.direction = direction;
                res.endPoint = lastPosition + (size * direction);
                res.pos = result[0];
                res.scale = result[1];
                results.Add(res);
                var tail = DrawCube(result[0], result[1], null, mat);
                tail.transform.SetParent(tailParent);
                lastPosition = res.endPoint;
                positions.Add(lastPosition);
                var autoplay = DrawCube(lastPosition + (direction * 0.9f), Vector3.one);
                autoplay.name = tailTag;
                autoplay.transform.SetParent(autoTailParent);
                DestroyImmediate(autoplay.GetComponent<MeshRenderer>());
                var centerTail = DrawCube(lastPosition, mov.transform.localScale, null, mat);
                centerTail.transform.SetParent(centerTailParent);
                turn = (turn + 1).RevertClamp(0, mov.turns.Length - 1);
            }
        }
    }
    public static GameObject DrawCube(Vector3 pos, Vector3 scale, Transform parent = null, Material material = null)
    {
        var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.GetComponent<BoxCollider>().isTrigger = true;
        if(material != null) obj.GetComponent<MeshRenderer>().sharedMaterial = material;
        obj.transform.position = pos;
        obj.transform.localScale = scale;
        if (parent != null)
        {
            obj.transform.SetParent(parent);
        }
        return obj;
    }
    public static Vector3 Singularite(Vector3 from)
    {
        var rot = Quaternion.Euler(from);

        var forward = Vector3.forward;  // fairly common

        var result = rot * forward;
        return result;
    }
    public static Vector3[] GetLine(Vector3 from, Vector3 to, Vector3 minimumScale)
    {
        var position = Vector3.zero;
        var scale = Vector3.one;
        var centeredPosition = CenterOfVectors(new Vector3[] { from, to });
        var scaledLine = from - to;
        var fixedScaledLine = new Vector3(II(Mathf.Abs(scaledLine.x), minimumScale.x), II(Mathf.Abs(scaledLine.y), minimumScale.y), II(Mathf.Abs(scaledLine.z), minimumScale.z));
        position = centeredPosition;
        scale = fixedScaledLine;
        return new Vector3[] { position, scale };
    }
    public static float II(float val, float limit)
    {
        if (val < limit)
        {
            val = limit;
        }
        return val;
    }
    public static Vector3 CenterOfVectors(Vector3[] vectors)
    {
        Vector3 sum = Vector3.zero;
        if (vectors == null || vectors.Length == 0)
        {
            return sum;
        }

        foreach (Vector3 vec in vectors)
        {
            sum += vec;
        }
        return sum / vectors.Length;
    }
    #endregion
}


public class BeatTiming
{
    public float bpm = 120;
    public List<float> timings = new List<float>();

    public BeatTiming()
    {

    }

    public BeatTiming(float beat, List<float> timings)
    {
        bpm = beat;
        this.timings = timings;
    }
}