using System.Collections.Generic;
using UnityEngine;

public class AnimateObject : Trigger
{
	public TweenValues[] tweens = {new TweenValues()};
	
	public override void OnStart()
	{
		foreach(var t in tweens)
		{
			t.target.gameObject.AddKeeper();
		}
	}
	
	public override void OnEnter(Collider other)
	{
		foreach(var t in tweens)
		{
			t.TweenObject();
		}
	}
	
	public void CancelTween(GameObject obj)
	{
		LeanTween.cancel(obj);
	}
}

public static class TweenUtility
{
	public static void AddKeeper(this GameObject obj)
	{
		var k = obj.GetComponent<TransformKeeper>();
		if(k == null) obj.AddComponent<TransformKeeper>();
	}
}

[System.Serializable]
public class TweenValues
{
	public Transform target;
	
	public LeanTweenType type = LeanTweenType.easeOutCubic;
	public float time;
	public float delay;
	
	public bool changePosition;
	public Vector3 targetPosition;
	public bool changeRotation;
	public Vector3 targetRotation;
	public bool changeScale;
	public Vector3 targetScale;
	
	public void TweenObject()
	{
		if(changePosition) target.LeanMoveLocal(targetPosition, time).setEase(type).setDelay(delay);
		if(changeRotation) LeanTween.value(target.gameObject, target.transform.eulerAngles, targetRotation, time).setEase(type).setDelay(delay);
		if(changeScale) target.LeanScale(targetScale, time).setEase(type).setDelay(delay);
	}
}