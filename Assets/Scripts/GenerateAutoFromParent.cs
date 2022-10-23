using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GenerateAutoFromParent : MonoBehaviour {
	public Transform parent;
	private Transform _parent
	{
		get
		{
			if(parent == null) parent = transform;
			return parent;
		}
	}
	
	public Transform autoParents;
	
	public GameObject instance;
	public float forwardIncrease = 0.4f;
	public bool generate;
	
	public bool turn;
	public int total;
	public void Generate()
	{
		var a = transform.GetComponentsInChildren<Transform>();
		total = 0;
		foreach(var t in a)
		{
			if(total > 0){
				turn = !turn;
				var coor = (t.forward * t.localScale.z / 2) + t.position + (t.forward * forwardIncrease);
				var obj = Instantiate(instance);
				obj.transform.position = coor;
				obj.transform.eulerAngles = t.eulerAngles;
				obj.name = turn ? "Turn1" : "Turn2";
				if(autoParents != null) obj.transform.SetParent(autoParents);
			}
			total++;
		}
	}
	
	void Update()
	{
		if(generate)
		{
			Generate();
			generate = false;
		}
	}
}
