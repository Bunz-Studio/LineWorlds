using System;
using System.Reflection;
using UnityEngine;

public class PostTrigger : Trigger
{
	public string property = "intensity";
	public float valueFrom;
	public float valueTo;
	public float time = 2;
	public LeanTweenType easeType;
	
	FieldInfo field;
	public override void OnStart()
	{
		//field = bloom.GetType().GetField(property);
	}
	
	public override void OnEnter(Collider other)
	{
		LeanTween.value(gameObject, valueFrom, valueTo, time).setEase(easeType).setOnUpdate(SetPropertyValue);
	}
	
	public void SetPropertyValue(float to)
	{
		//field.SetValue(bloom, to);
	}
}
