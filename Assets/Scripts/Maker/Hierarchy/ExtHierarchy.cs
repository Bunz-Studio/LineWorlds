using UnityEngine;
using System.Collections.Generic;

namespace ExternMaker
{
    public class ExtHierarchy : MonoBehaviour
    {
        private static ExtHierarchy p_instance;
        public static ExtHierarchy instance
        {
            get
            {
                p_instance = ExtUtility.GetStaticInstance(p_instance);
                return p_instance;
            }
        }

        public Transform itemParent;
        public GameObject itemPrefab;

        public List<ExtHierarchyItem> items = new List<ExtHierarchyItem>();
        public List<ExtHierarchyItem> selectedItems = new List<ExtHierarchyItem>();
        public List<ExtObject> objects = new List<ExtObject>();

        public Color normalColor = Color.gray;
        public Color selectedColor = Color.blue;

        void Start()
        {
            //ExtCore.instance.OnHierachyUpdate += UpdateHierarchy;

            //ExtCore.instance.OnObjectUpdate += SelectionUpdate;
        }

        private void OnDestroy()
        {
            p_instance = null;
        }

        public void SelectionUpdate(GameObject obj)
        {
            var selection = new List<ExtHierarchyItem>();
            foreach(Transform tr in ExtSelection.instance.mainTransformSelected)
            {
                var s = GetSelected(tr.GetComponent<ExtObject>());
                if(s != null) selection.Add(s);
            }
            foreach (var s in selectedItems)
            {
                RemoveSelected(s, false);
            }
            selectedItems.Clear();
            foreach(var s in selection)
            {
                AddSelected(s);
            }
        }

        public void ClearSelection()
        {
            foreach (var s in selectedItems)
            {
                RemoveSelected(s);
            }
        }
        public void ClearItems()
        {
            foreach (var s in items)
            {
                RemoveHierarchyItem(s.target);
            }
        }

        public void AddItems(List<ExtObject> objs)
        {
            foreach (var obj in objs)
            {
                AddHierarchyItem(obj);
            }
        }

        public void InitializeHierarchy()
        {

        }

        public void UpdateHierarchy(HierarchyUpdate info)
        {
            if (info != null)
            {
                for (int i = 0; i < info.adds.Length; i++)
                {
                    var obj = info.adds[i];
                    if (!objects.Contains(obj))
                    {
                        AddHierarchyItem(obj);
                    }
                }

                for (int i = 0; i < info.removes.Length; i++)
                {
                    RemoveHierarchyItem(info.removes[i]);
                }
            }
        }

        public void AddHierarchyItem(ExtObject obj, int index = -1)
        {
            var inst = Instantiate(itemPrefab, itemParent);
            if (index > -1) inst.transform.SetSiblingIndex(index);
            inst.name = "Hierarchy_" + obj.name;

            var item = inst.GetComponent<ExtHierarchyItem>();
            item.Initialize(obj);
            item.hierarchy = this;
            if (index > -1) items.Insert(index, item); else items.Add(item);
            if (!objects.Contains(obj))
                objects.Add(obj);
        }

        public void RemoveHierarchyItem(ExtObject obj)
        {
            var item = GetHierarchyItem(obj);
            if(item != null)
            {
                DestroyImmediate(item.gameObject);
                items.Remove(item);
                if (selectedItems.Contains(item)) RemoveSelected(item);
            }
            if(objects.Contains(obj))
                objects.Remove(obj);
        }

        public ExtHierarchyItem GetHierarchyItem(ExtObject obj)
        {
            foreach(var item in items)
            {
                if (item.target == obj) return item;
            }
            return null;
        }

        public void SetSelected(ExtHierarchyItem item)
        {
            if(Input.GetKey(KeyCode.LeftShift | KeyCode.RightShift))
            {
                if (selectedItems.Count > 0)
                {
                    var selection = new List<ExtHierarchyItem>();
                    var firstIndex = items.FindIndex(val => val == selectedItems[0]);
                    var index = items.FindIndex(val => val == item);

                    for(int i = firstIndex; i < index + 1; i++)
                    {
                        if (selectedItems.Contains(items[i]))
                        {
                            AddSelected(items[i]);
                        }
                        selection.Add(items[i]);
                    }

                    foreach(var s in selectedItems)
                    {
                        if (!selection.Contains(s)) RemoveSelected(s);
                    }
                }
            }
            else if (Input.GetKey(KeyCode.LeftControl))
            {
                AddSelected(item);
            }
            else
            {
                foreach (var s in selectedItems)
                {
                    RemoveSelected(s);
                }
                AddSelected(item);
            }
            UpdateSelections();
        }

        public void AddSelected(ExtHierarchyItem item)
        {
            item.image.color = selectedColor;
            if(!selectedItems.Contains(item)) selectedItems.Add(item);
        }

        public void RemoveSelected(ExtHierarchyItem item, bool remove = true)
        {
            item.image.color = normalColor;

            if (!remove) return;
            if (selectedItems.Contains(item)) selectedItems.Remove(item);
        }

        public void UpdateSelections()
        {
            var objs = new List<Transform>();
            foreach(var item in selectedItems)
            {
                objs.Add(item.target.transform);
            }
            ExtSelection.instance.transformSelection = objs;
        }

        public ExtHierarchyItem GetSelected(ExtObject obj)
        {
            foreach (var item in selectedItems)
            {
                if (item.target == obj) return item;
            }
            return null;
        }
        public ExtHierarchyItem GetSelected(Transform tr)
        {
            foreach (var item in selectedItems)
            {
                if (item.target.transform == tr) return item;
            }
            return null;
        }
    }

    public class HierarchyUpdate
    {
        public ExtObject[] adds;
        public ExtObject[] removes;

        public HierarchyUpdate()
        {

        }

        public HierarchyUpdate(ExtObject[] adds, ExtObject[] removes)
        {
            this.adds = adds;
            this.removes = adds;
        }
    }
}
