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

        public Transform moveItemBar;
        public ExtHierarchyItem heldItem;
        public ExtHierarchyItem moveSiblingItem;

        public Color normalColor = Color.gray;
        public Color selectedColor = Color.blue;

        void Start()
        {
            //ExtCore.instance.OnHierachyUpdate += UpdateHierarchy;
            ExtCore.instance.OnObjectUpdate += UpdateHierarchy;
            ExtCore.instance.OnClearObject += ObjectClear;
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
                RemoveSelected(s, false);
            }
            selectedItems.Clear();
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

        public void UpdateHierarchy(List<GameObject> objs)
        {
            ClearSelection();
            foreach(var obj in objs)
            {
                var ext = obj.GetComponent<ExtObject>();
                if(ext != null)
                {
                    var item = GetHierarchyItem(ext);
                    if(item != null)
                    {
                        AddSelected(item);
                    }
                }
            }
        }

        public void ObjectClear()
        {
            ClearSelection();
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
            if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                if (selectedItems.Count > 0)
                {
                    var selection = new List<ExtHierarchyItem>();
                    var firstIndex = items.IndexOf(selectedItems[0]);
                    var index = items.IndexOf(item);
                    if(index < firstIndex)
                    {
                        for(int i = firstIndex; i > index - 1; i--)
                        {
                            selection.Add(items[i]);
                        }
                    }
                    else
                    {
                        for (int i = firstIndex; i < index + 1; i++)
                        {
                            selection.Add(items[i]);
                        }
                    }
                    ClearSelection();
                    foreach(var rItem in selection)
                    {
                        AddSelected(rItem);
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
                    RemoveSelected(s, false);
                }
                selectedItems.Clear();
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
            try
            {
                item.image.color = normalColor;

                if (!remove) return;
                if (selectedItems.Contains(item)) selectedItems.Remove(item);
            }
            catch
            {

            }
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

        public void HoldItem(ExtHierarchyItem item)
        {
            if (heldItem == null)
            {
                heldItem = item;
                Debug.Log("Item held " + item.name);
            }
        }

        public void ReleaseItem(ExtHierarchyItem item, UnityEngine.EventSystems.PointerEventData eventData)
        {
            if (heldItem != null)
            {
                ExtHierarchyItem lastItem = null;
                foreach(var it in items)
                {
                    if(eventData.position.y < it.transform.position.y)
                    {
                        lastItem = it;
                    }
                }
                if(lastItem != null)
                {
                    moveSiblingItem = lastItem;
                    var setIndx = moveSiblingItem.target.transform.GetSiblingIndex() + 1;
                    var itemIndx = moveSiblingItem.transform.GetSiblingIndex() + 1;
                    heldItem.transform.SetSiblingIndex(itemIndx);
                    heldItem.target.transform.SetSiblingIndex(setIndx);
                }
            }
            moveItemBar.gameObject.SetActive(false);
            heldItem = null;
        }

        public void ItemHovered(ExtHierarchyItem item)
        {
            if(heldItem != null)
            {
                moveSiblingItem = item;
                moveItemBar.SetSiblingIndex(moveSiblingItem.transform.GetSiblingIndex());
                moveItemBar.gameObject.SetActive(true);
            }
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
            this.removes = removes;
        }
    }
}
