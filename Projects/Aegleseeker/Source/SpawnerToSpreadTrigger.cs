using UnityEngine;
using LineWorldsMod;

public class SpawnerToSpreadTrigger : ModTrigger
{
	public GameObject object1;
	public GameObject object2;
	
	public Vector3 spreadOffset = new Vector3(20, 0, -20);
	public LeanTweenType tweenType = LeanTweenType.easeOutCubic;
	public float time = 1;
	public Color instanceColor = Color.black;
	public Vector3 instanceScale = new Vector3(0.2f, 100, 0.2f);
	public float distance = 5;
	
	public override void OnGameStart()
	{
		if(object1 == null) object1 = GetDummyObject(transform.right * distance);
		if(object2 == null) object2 = GetDummyObject(-transform.right * distance);
		base.OnGameStart();
	}
	
	public override void OnGameStop()
	{
		if(object1 != null)
		{
			if(object1.name == "InstancedToBeDestroyed") Destroy(object1);
		}
		if(object2 != null)
		{
			if(object2.name == "InstancedToBeDestroyed") Destroy(object2);
		}
		base.OnGameStop();
	}
	
	public GameObject GetDummyObject(Vector3 offset = default(Vector3))
	{
		var instance = GameObject.CreatePrimitive(PrimitiveType.Cube);
		Destroy(instance.GetComponent<BoxCollider>());
		instance.GetComponent<MeshRenderer>().sharedMaterial = GetComponent<MeshRenderer>().material;
		instance.GetComponent<MeshRenderer>().sharedMaterial.color = instanceColor;
		instance.transform.localScale = instanceScale;
		instance.transform.position = transform.position + offset;
		instance.name = "InstancedToBeDestroyed";
		return instance;
	}
	
	public override void OnEnter(Collider other)
	{
		if(object1 == null || object2 == null) return;
		object1.LeanMoveLocal(object1.transform.localPosition + (spreadOffset / 2), time).setEase(tweenType);
		object2.LeanMoveLocal(object2.transform.localPosition + -(spreadOffset / 2), time).setEase(tweenType);
	}
}