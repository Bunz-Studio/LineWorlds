using System.Collections.Generic;
using UnityEngine;

public class ShakeCamera : Trigger
{
	public float duration = 0.3f;
	public float amount = 0.3f;
	public override void OnEnter(Collider other)
	{
		if(other.tag == "Player")
		{
			BetterCamera.current.shakeDuration = duration;
			BetterCamera.current.shakeAmount = amount;
		}
	}
}
