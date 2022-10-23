using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ContextAvailable : MonoBehaviour, IPointerClickHandler
{
    public ContextAvailableMenu contextMenu;

    public void OnPointerClick(PointerEventData data)
    {
        if(data.button == PointerEventData.InputButton.Right)
        {
            OnShowContextMenu();
        }
    }

    public virtual void OnShowContextMenu()
    {
        contextMenu.Show();
    }
}
