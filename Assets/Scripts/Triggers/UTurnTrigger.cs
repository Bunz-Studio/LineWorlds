using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UTurnTrigger : Trigger
{
	public Vector3[] turns = {
		new Vector3(0, 90, 0),
		Vector3.zero
	};
	public override void OnEnter(Collider other)
	{
		var l = other.GetComponent<LineMovement>();
		l.turns = turns;
	}
}
