using UnityEngine;
using ExternMaker;

public class InspectableCamera : MonoBehaviour
{
    public BetterCamera source;
    public Vector3 rotation
    {
        get => new Vector3(source.targetX, source.targetY, source.targetZ);
        set
        {
            source.targetX = value.x;
            source.targetY = value.y;
            source.targetZ = value.z;
        }
    }
    public Vector3 offset
    {
        get => source.pivotOffset;
        set => source.pivotOffset = value;
    }
    public float distance
    {
        get => source.TargetDistance;
        set => source.TargetDistance = value;
    }
    public float SmoothTime
    {
        get => source.SmoothTime;
        set => source.SmoothTime = value;
    }
    public float SmoothFactor
    {
        get => source.SmoothFactor;
        set => source.SmoothFactor = value;
    }
    public float needtime
    {
        get => source.needtime;
        set => source.needtime = value;
    }
    
    void Update()
    {
        if (!ExtCore.isOnlyPlaymode && ExtCore.playState != EditorPlayState.Playing)
        {
            source.UpdateCameraPosition();
        }
    }
}
