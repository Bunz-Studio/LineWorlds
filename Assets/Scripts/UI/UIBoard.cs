using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBoard : MonoBehaviour
{
	[Header("Inside")]
	public Transform[] panels;
	public Transform panelHide;
	public Transform panelShow;
	
	[Header("Outside")]
	public Transform[] types;
	
	void Start()
	{
		ChangePanel(0);
	}
	
	public void ChangePanel(int i)
	{
		if(i < panels.Length)
		{
			for(int a = 0; a < panels.Length; a++)
			{
				Vector3 target = a == i ? panelShow.transform.localPosition : panelHide.transform.localPosition;
				panels[a].LeanMoveLocal(target, 0.5f).setEase(LeanTweenType.easeOutCubic);
			}
		}
	}
	
	public void ChangeType(int i)
	{
		if(i < types.Length)
		{
			var a = types[i];
			transform.LeanMoveLocal(a.localPosition, 0.5f).setEase(LeanTweenType.easeOutCubic);
			var rt = (RectTransform)a;
			var ro = (RectTransform)transform;
			LeanTween.value(gameObject, ro.sizeDelta, rt.sizeDelta, 0.5f).setOnUpdateVector3(
				c =>
				{
					ro.sizeDelta = c;
				}
			).setEase(LeanTweenType.easeOutCubic);
		}
	}
}
