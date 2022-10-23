using System.Collections.Generic;
using UnityEngine;
using ExternMaker;
using LineWorldsMod;

public class SpreadTrigger : ModTrigger
{
	public GameObject object1;
	public GameObject object2;
	
	public Vector3 spreadOffset = new Vector3(5, 0, 5);
	public LeanTweenType tweenType = LeanTweenType.linear;
	public float time = 5;
	
	public override void OnEnter(Collider other)
	{
		if(object1 == null || object2 == null) return;
		object1.LeanMoveLocal(object1.transform.localPosition + (spreadOffset / 2), time).setEase(tweenType);
		object2.LeanMoveLocal(object2.transform.localPosition + -(spreadOffset / 2), time).setEase(tweenType);
	}
}