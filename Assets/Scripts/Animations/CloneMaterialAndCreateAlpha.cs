using System.Collections.Generic;
using UnityEngine;

public class CloneMaterialAndCreateAlpha : MonoBehaviour
{
	public Transform parent;
	public List<MeshRenderer> renderers = new List<MeshRenderer>();
	
	public Material clonedMaterial;
	
	public LeanTweenType publicEaseType = LeanTweenType.easeOutCubic;
	public float publicTime = 0.5f;
	void Start()
	{
		Fetch();
	}
	
	public void Fetch ()
	{
		renderers = new List<MeshRenderer>(parent.GetComponentsInChildren<MeshRenderer>());
		if(renderers.Count > 0)
		{
			clonedMaterial = renderers[0].material;
			foreach(var i in renderers)
			{
				i.sharedMaterial = clonedMaterial;
			}
		}
	}
	
	public void Pulse(float fr)
	{
		var t = clonedMaterial.color;
		LeanTween.cancel(gameObject);
		LeanTween.value(gameObject, fr, 0, publicTime).setEase(publicEaseType).setOnUpdate(
			(float val) => {
				clonedMaterial.color = new Color(t.r, t.g, t.b, val);
			}
		);
	}
	
	public void PulseReverse(float fr)
	{
		var t = clonedMaterial.color;
		LeanTween.cancel(gameObject);
		LeanTween.value(gameObject, 0, fr, publicTime).setEase(publicEaseType).setOnUpdate(
			(float val) => {
				clonedMaterial.color = new Color(t.r, t.g, t.b, val);
			}
		);
	}
}
