using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
	
	int revivePoint;
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
		if(other.tag == targetTrigger) {
			revivePoint = manager.revivePoint;
			OnEnter(other);
		}
	}
	
	public static LineMovement GetLine(Collider other)
	{
		return other.GetComponent<LineMovement>();
	}
}
