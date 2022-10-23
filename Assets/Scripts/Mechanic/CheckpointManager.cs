using System;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
	public Trigger[] checkpoints;
	public int revivePoint;
	public int checkpointResult;
	public Action<int> OnObtained;
	public Action<int> OnUndo;
	
	void Start()
	{
		var trig = GameObject.FindGameObjectsWithTag("Trigger");
		foreach(var a in trig)
		{
			a.GetComponent<MeshRenderer>().enabled = false;
		}
	}
}
