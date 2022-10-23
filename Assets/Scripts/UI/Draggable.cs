using UnityEngine;
using UnityEngine.EventSystems;

public class Draggable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
	
	public Transform target;
	
	bool isMouseDown;
	bool isHovering;
	Vector3 startMousePosition;
	Vector3 startPosition;

	public void OnPointerEnter(PointerEventData dt) {
		isHovering = true;
	}

	public void OnPointerExit(PointerEventData dt) {
		isHovering = false;
	}

    private void OnEnable()
    {
        isMouseDown = false;
    }

    void Update () {
		if(isHovering && Input.GetMouseButtonDown(0))
		{
			isMouseDown = true;
			startPosition = target.position;
			startMousePosition = Input.mousePosition;
            target.transform.SetAsLastSibling();
		}
		isMouseDown &= !Input.GetMouseButtonUp(0);
		if (isMouseDown) {
			Vector3 currentPosition = Input.mousePosition;
			Vector3 diff = currentPosition - startMousePosition;
			Vector3 pos = startPosition + diff;
			target.position = pos;
		}
	}
}