using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ContextAvailableMenu : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public List<ContextAvailableItem> items = new List<ContextAvailableItem>();
    public bool isHovering;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
        {
            if (!ClickingSelfOrChild() && !isHovering)
            {
                gameObject.SetActive(false);
            }
        }
    }

    public void ShowAndReset()
    {
        Show();
        foreach(var item in items)
        {
            item.onSelected = null;
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public ContextAvailableItem GetItem(string name)
    {
        return items.Find(val => val.text.text == name);
    }

    public void OnPointerEnter(PointerEventData pointer)
    {
        isHovering = true;
    }

    public void OnPointerExit(PointerEventData pointer)
    {
        isHovering = false;
    }
    
    private bool ClickingSelfOrChild()
    {
        RectTransform[] rectTransforms = GetComponentsInChildren<RectTransform>();
        foreach (RectTransform rectTransform in rectTransforms)
        {
            if (EventSystem.current.currentSelectedGameObject == rectTransform.gameObject)
            {
                return true;
            };
        }
        return false;
    }

    [System.Serializable]
    public class ContextAvailableItem
    {
        public Button button;
        public Text text;
        public Action onSelected;
        public bool collapseWhenSelected = true;

        public void Select()
        {

        }
    }
}
