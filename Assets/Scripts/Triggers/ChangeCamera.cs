using UnityEngine;

public class ChangeCamera : MonoBehaviour
{
	public BetterCamera cam;
	public Vector3 TargetAngleRotation = new Vector3(45, 45, 0), TargetPivotOffset;
	public float TargetCamDistance = 20;
	public float TargetSmoothing = 1, TargetRotationSmoothing = 1;
	public float TargetFactor = 1;
	public bool ChangeTargetObject;
	public Transform TargetObjectToSee;
	
	private void OnTriggerEnter(Component other)
	{
		if (other.tag == "Player") {
			cam.ChangeVar(TargetAngleRotation, TargetSmoothing, TargetRotationSmoothing, TargetPivotOffset, TargetCamDistance);
			cam.SmoothFactor = TargetFactor;
			if (ChangeTargetObject) {
				cam.Line = TargetObjectToSee;
			}
		}
	}
}