using UnityEngine;

public class TransformKeeper : MonoBehaviour
{
	CheckpointManager manager;
	public TransformClass cls;
	int obtained;
	void Start()
	{
		manager = FindObjectOfType<CheckpointManager>();
		manager.OnObtained += a => {
			obtained = a;
			cls = new TransformClass(transform);
		};
		manager.OnUndo += a =>
		{
			if(a == obtained) cls.Apply(transform);
		};
	}
}

public class TransformClass
{
	public Vector3 localPosition;
	public Vector3 localEulerAngles;
	public Vector3 localScale;
	
	public TransformClass()
	{
		
	}
	
	public TransformClass(Transform t)
	{
		localPosition = t.localPosition;
		localEulerAngles = t.localEulerAngles;
		localScale = t.localScale;
	}
	
	public void Apply(Transform to)
	{
		to.localPosition = localPosition;
		to.localEulerAngles = localEulerAngles;
		to.localScale = localScale;
	}
}
