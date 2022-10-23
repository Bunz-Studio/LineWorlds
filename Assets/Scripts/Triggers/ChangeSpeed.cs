using System.Collections.Generic;
using UnityEngine;

public class ChangeSpeed : Trigger
{
	public float targetSpeed = 12;
	LineMovement[] movs;
	
	public override void OnStart()
	{
		movs = FindObjectsOfType<LineMovement>();
	}
	
	public override void OnEnter(Collider other)
	{
		foreach(var a in movs)
		{
			a.lineSpeed = targetSpeed;
		}
	}
}
