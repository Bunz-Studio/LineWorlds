using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExternMaker;
using ExternMaker.Serialization;

public class Trigger : MonoBehaviour
{
	public string targetTrigger = "Player";
	
	public virtual void OnStart()
	{
		
	}
	
	public virtual void OnEnter (Collider other)
	{
		
	}
	
	public virtual void OnUndo ()
	{
		
	}
	
    [IgnoreSavingState]
	int revivePoint;
    [IgnoreSavingState]
    CheckpointManager manager;

	void Start()
    {
        manager = FindObjectOfType<CheckpointManager>();
		manager.OnUndo += UndoTrigger;
		GetComponent<MeshRenderer>().enabled = false;
		OnStart();
	}
	
	void UndoTrigger(int point)
	{
		if(revivePoint == point) OnUndo();
	}
	
	void OnTriggerEnter(Collider other)
	{
		if(other.tag == targetTrigger)
        {
            if (ExtCore.instance != null && ExtCore.playState != EditorPlayState.Playing) return;

            revivePoint = manager.revivePoint;
            OnEnter(other);
		}
	}
	
	public static LineMovement GetLine(Collider other)
	{
		return other.GetComponent<LineMovement>();
	}
}

public class SerializableTrigger : MonoBehaviour
{
    public Serializables.SerializedUniversalComponent serializer;
    public void Initialize(System.Type type)
    {
        serializer.actualType = type.FullName;
    }
    public string targetTrigger = "Player";

    public virtual void OnStart()
    {

    }

    public virtual void OnEnter(Collider other)
    {

    }

    public virtual void OnUndo()
    {

    }

    [IgnoreSavingState]
    int revivePoint;
    [IgnoreSavingState]
    CheckpointManager manager;

    void Start()
    {
        Initialize(GetType());
        manager = FindObjectOfType<CheckpointManager>();
        manager.OnUndo += UndoTrigger;
        // GetComponent<MeshRenderer>().enabled = false;
        OnStart();
    }

    void UndoTrigger(int point)
    {
        if (revivePoint == point) OnUndo();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == targetTrigger)
        {
            if (ExtCore.instance != null && ExtCore.playState != EditorPlayState.Playing) return;

            revivePoint = manager.revivePoint;
            OnEnter(other);
        }
    }

    public static LineMovement GetLine(Collider other)
    {
        return other.GetComponent<LineMovement>();
    }
}
