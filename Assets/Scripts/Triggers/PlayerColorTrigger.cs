using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerColorTrigger : Trigger
{
	public Color targetColor = Color.white;
	public float tweenTime = 1;
	public LeanTweenType tweenType = LeanTweenType.easeOutCubic;
	public override void OnEnter(Collider other)
	{
		var l = GetLine(other);
		LeanTween.value(gameObject, l.rend.sharedMaterial.color, targetColor, tweenTime).setEase(tweenType).setOnUpdate(l.ChangeColor);
	}
}
