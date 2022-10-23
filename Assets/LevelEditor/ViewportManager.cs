using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewportManager : MonoBehaviour
{
	public Camera editorCamera;
	public Camera runtimeCamera;
	public RectTransform viewport;
	
	void Update()
	{
		SetCameraRectAsViewport(editorCamera);
		SetCameraRectAsViewport(runtimeCamera);
	}
	
	public void SetCameraRectAsViewport(Camera cam)
	{
		cam.pixelRect = new Rect(Vector2.zero, viewport.rect.size);
	}
}
