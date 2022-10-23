using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[ExecuteInEditMode]
public class ViewportTextureManager : MonoBehaviour//, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
	public GameObject viewportObject;
	public RectTransform parent;
	public RectTransform viewport;
	public RectTransform parentCalculation;
	public Camera targetCamera;
	
	public bool autoAdjust = true;
	public Vector2 sizeViewport;
	public Vector2 sizeParent;
	
	public Vector2 positionViewport;
	public Vector2 positionParent;
	
	public Vector2 resultSize;
	public Vector2 resultPos;
	
	void Update()
	{
		if(targetCamera == null) return;
		
		targetCamera.gameObject.SetActive(viewportObject.activeSelf);
		//var size = viewport.rect.size;
		parentCalculation.position = parent.position;
		parentCalculation.sizeDelta = parent.sizeDelta;
		sizeViewport = viewport.rect.size;
		sizeParent = parent.rect.size;
		resultSize = sizeViewport / sizeParent;
		//var position = viewport.rect.position;
		positionViewport = viewport.rect.position;
		positionParent = parentCalculation.rect.position;
		resultPos = -((positionParent - positionViewport) / sizeParent);
		
		//targetCamera.pixelRect = new Rect(position, size);
		targetCamera.rect = new Rect(resultPos, resultSize);
	}
}
