using System.Collections.Generic;
using UnityEngine;

public class TextAnimateItem : MonoBehaviour
{
	public TextMesh mesh;
	
	public Vector3 start;
	public Vector3 offset = new Vector3(0, 20, 0);
	public LeanTweenType tweenType = LeanTweenType.easeOutCubic;
	
	public Vector3 offsetMin;
	public Vector3 offsetMax;
	
	void Start()
	{
		start = transform.position;
		transform.position += offset;
		transform.position += GetRandomVector3(offsetMin, offsetMax);
	}
	
	public void AnimateBack(float time)
	{
		transform.LeanMove(start, time).setEase(tweenType);
	}
	
	public static Vector3 GetRandomVector3(Vector3 min, Vector3 max)
	{
		return new Vector3(
			Random.Range(min.x, max.x),
			Random.Range(min.y, max.y),
			Random.Range(min.z, max.z)
		);
	}
}
