using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace ExternMaker
{
    public class ExtHierarchyItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public ExtObject target;
        public ExtHierarchy hierarchy;
        public Text text;
        public Image image;

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
        bool isHold = true;

        void Update()
        {
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!isHold)
            {
                hierarchy.HoldItem(this);
                isHold = true;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isHold)
            {
                hierarchy.HoldItem(this);
                isHold = true;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (isHold)
            {
                hierarchy.ReleaseItem(this, eventData);
            }
        }
    }
}
