using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace ExternMaker
{
    public class ExtHierarchyItem : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public ExtObject target;
        public ExtHierarchy hierarchy;
        public Text text;
        public Image image;

        const bool interactionAvailable = false;

        void Start()
        {
            ExtCore.instance.OnHierachyUpdate += UpdateHierarchy;
        }

        public void Initialize(ExtObject obj)
        {
            target = obj;

            text.text = obj.name;
        }

        public void UpdateHierarchy(HierarchyUpdate info)
        {
            text.text = target.name;
        }

        public void SelectHierarchy()
        {
            hierarchy.SetSelected(this);
        }

        // Events
        public void OnPointerEnter(PointerEventData pointer)
        {
            if(interactionAvailable) hierarchy.ItemHovered(this);
        }
        
        public void OnPointerExit(PointerEventData pointer)
        {
        }

        public void OnPointerDown(PointerEventData pointer)
        {
            if (interactionAvailable) hierarchy.HoldItem(this);
        }

        public void OnPointerUp(PointerEventData pointer)
        {
            if (interactionAvailable) hierarchy.ReleaseItem(this);
        }
    }
}
