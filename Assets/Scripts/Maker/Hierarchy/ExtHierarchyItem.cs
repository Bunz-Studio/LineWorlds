using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace ExternMaker
{
    public class ExtHierarchyItem : MonoBehaviour
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
    }
}
