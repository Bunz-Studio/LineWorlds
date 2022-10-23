using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LoopSpawn : MonoBehaviour {
	public Transform parent;
	private Transform _parent
	{
		get
		{
			if(parent == null) {
				parent = transform;
				return transform;
			}
			return parent;
		}
	}
	public GameObject instance;
	[Header("Position")]
	public Vector3 offset;
	public Vector3 randomMin;
	public Vector3 randomMax;
	[Header("Rotation")]
	public Vector3 rotOffset;
	public Vector3 randomRotMin;
	public Vector3 randomRotMax;
	public int total = 20;

	public bool create;

	public void CreateLoop(int i)
	{
		var a = Instantiate(instance);
		a.transform.SetParent(_parent);
		a.transform.localPosition = (i * offset) + GetRandomOffset();
		a.transform.localEulerAngles += (i * rotOffset) + GetRandomRotOffset();
	}
	
	public Vector3 GetRandomOffset()
	{
		return GetRandomOffsetVector(randomMin, randomMax);
	}
	public Vector3 GetRandomRotOffset()
	{
		return GetRandomOffsetVector(randomRotMin, randomRotMax);
	}
	
	public static Vector3 GetRandomOffsetVector(Vector3 min, Vector3 max)
	{
		return new Vector3(Random.Range(min.x, max.x), Random.Range(min.y, max.y), Random.Range(min.z, max.z));
	}

	void Update()
	{
		if(create){
			var ch = _parent.GetComponentsInChildren<Transform>();
			foreach(var c in ch)
			{
				if(c != null)
				{
					if(c != transform) DestroyImmediate(c.gameObject);
				}
			}
			for(int i = 0; i < total; i++){
				CreateLoop(i);
			}
			create = false;
		}
	}	
}
