using System.Collections.Generic;
using UnityEngine;

public class SpreadAcross : MonoBehaviour
{
	public List<Transform> objs = new List<Transform>();
	public LeanTweenType type = LeanTweenType.easeOutCirc;
	public Vector3 origin;
	public Vector3 spread;
	public float time = 5;
	void Start()
	{
		var c = transform.GetComponentsInChildren<Transform>();
		objs = new List<Transform>(c);
	}
	
	public void Spread()
	{
		int i = -System.Convert.ToInt32((float)objs.Count / 2);
		foreach(var o in objs)
		{
			if(o != transform){
				o.transform.LeanMoveLocal(origin + (spread * i), time).setEase(type);
				i++;
			}
		}
	}
}
