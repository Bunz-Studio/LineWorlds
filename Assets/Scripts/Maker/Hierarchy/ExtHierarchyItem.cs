using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace ExternMaker
{
    public class ExtHierarchyItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public ExtObject target;
        public ExtHierarchy hierarchy;
        public Text text;
        public Image image;
        bool notMovable;

        void Start()
        {
            ExtCore.instance.OnHierachyUpdate += UpdateHierarchy;
        }

        public void Initialize(ExtObject obj)
        {
            target = obj;
            text.text = obj.name;

            if (obj.tag == "Player") notMovable = true;
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
        bool isHovered;
        bool isDown;
        bool nowHeld;
        Vector3 pos;

        void Update()
        {
            text.text = target.name;
            if (notMovable) return;

            if (Input.GetMouseButtonDown(0) && isHovered)
            {
                isDown = true;
                pos = Input.mousePosition;
            }

            if (isDown)
            {
                if (pos != Input.mousePosition)
                {
                    if (!nowHeld)
                    {
                        hierarchy.HoldItem(this);
                        nowHeld = true;
                    }
                    pos = Input.mousePosition;
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                if (nowHeld)
                {
                    hierarchy.ReleaseItem(this);
                    nowHeld = false;
                }
                isDown = false;
            }
        }

        public void OnPointerEnter(PointerEventData pointer)
        {
            if (notMovable) return;
            isHovered = true;
            hierarchy.ItemHovered(this);
        }

        public void OnPointerExit(PointerEventData pointer)
        {
            isHovered = false;
        }
    }
}
