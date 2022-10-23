using UnityEngine;
using LineWorldsMod;

public class PulseBackgroundFog : ModTrigger
{
	public Color startColor = Color.black;
	public Color endColor = Color.black;
	
	public LeanTweenType tweenType = LeanTweenType.easeOutCubic;
	public float time = 0.5f;
	
	public override void OnGameStart()
	{
		ExternMaker.ExtDialogManager.Alert("This level contains\nepilepsy/flashing lights");
	}
	
	public override void OnEnter(Collider other)
	{
		var cam = ModAccess.mainCamera.mainCamera;
		LeanTween.value(gameObject, startColor, endColor, time).setEase(tweenType).setOnUpdate((Color val) =>
		                                                                                       {
		                                                                                       	cam.backgroundColor = val;
		                                                                                       	RenderSettings.fogColor = val;
		                                                                                       }
		                                                                                      );
	}
}
