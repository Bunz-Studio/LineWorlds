using System.Collections.Generic;
using UnityEngine;
using ExternMaker;
using LineWorldsMod;

public class ApplyGroupAnimation : ModTrigger
{
	public float groupID;
	public Vector3 startOffset = new Vector3(0, -50, 0);
	public float distance = 15;
	public float speed = 0.5f;
	public LeanTweenType easeType = LeanTweenType.easeOutCubic;
	
	readonly List<ExtObject> objects = new List<ExtObject>();
	public override void OnGameStart()
	{
		foreach(var obj in objects)
		{
			if(obj.groupID.Contains((int)groupID))
			{
				var c = obj.GetComponent<AppearAtDistance>();
				if(c != null)
				{
					obj.GetComponent<MeshRenderer>().enabled = true;
					Destroy(c.instance);
					Destroy(c);
				}
			}
		}
		objects.Clear();
		
		foreach(var obj in FindObjectsOfType<ExternMaker.ExtObject>())
		{
			if(obj.groupID.Contains((int)groupID))
			{
				var c = obj.gameObject.AddOrGetComponent<AppearAtDistance>();
				c.minimalDistance = distance;
				c.startOffset = startOffset;
				c.easeType = easeType;
				c.speed = speed;
				objects.Add(obj);
			}
		}
	}
	
	public override void OnGameStop()
	{
		foreach(var obj in objects)
		{
			if(obj.groupID.Contains((int)groupID))
			{
				var c = obj.GetComponent<AppearAtDistance>();
				if(c != null)
				{
					obj.GetComponent<MeshRenderer>().enabled = true;
					Destroy(c.instance);
					Destroy(c);
				}
			}
		}
		objects.Clear();
	}
}
