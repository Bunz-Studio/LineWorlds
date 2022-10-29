using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ExternMaker
{
    public class ExtInsListInt : ExtFieldInspect
    {
        public override Type type => typeof(List<int>);

        public Button fieldDropper;
        public Text fieldShow;
        public GameObject dropdownMenu;
        public GameObject itemInstance;
        public Transform itemParent;
        public List<GameObject> instances = new List<GameObject>();

        public InputField addField;
        public bool allowMultiple;

        public Type enumType;

        bool isEditing;
        public int selectedIndex;
        public bool isOpened;

        public override void Initialize()
        {
            base.Initialize();

            fieldDropper.onClick.AddListener(() => ToggleDropdown());
            /*int i = 0;
            enumType = property == null ? field.FieldType : property.PropertyType;
            foreach (var name in Enum.GetNames(enumType))
            {
                if (!triggerTypes.ContainsKey(i))
                {
                    triggerTypes.Add(i, Enum.Parse(enumType, name));
                    triggerTypesIndex.Add(Enum.Parse(enumType, name), i);
                }
                AddEnumItem(name, i);
                i++;
            }*/
        }

        public override void SetAdditionalObject(GameObject obj)
        {
            base.SetAdditionalObject(obj);
            dropdownMenu = obj;
            itemParent = obj.transform.Find("GroupIDDropdown").Find("Viewport").Find("Content");
            addField = obj.transform.Find("Panel").Find("ListIntDropdownItem").Find("InputField").GetComponent<InputField>();
            var addButton = obj.transform.Find("Panel").Find("ListIntDropdownItem").Find("AddButton").GetComponent<Button>();
            addButton.onClick.AddListener(() => AddValueList());
        }

        public void FinishEdit()
        {
            ApplyInputs(true);
        }

        public override void ApplyTemp()
        {
            if (source != null || isStatic)
            {
                var o = GetValue();
                var list = (List<int>)o;
                base.ApplyTemp();
                ClearInstances();

                int i = 0;
                string show = null;
                foreach(var num in list)
                {
                    show += string.IsNullOrEmpty(show) ? num.ToString() : ", " + num.ToString();
                    AddListItem(num, i);
                    i++;
                }
                fieldShow.text = show;
            }
        }

        public void ClearInstances()
        {
            foreach(var inst in instances)
            {
                if (inst != null) DestroyImmediate(inst);
            }
            instances.Clear();
        }

        public void ApplyInputs(bool reset = false)
        {
            if (!isEditing)
            {
                isEditing = true;
                StartEdit();
            }
            //SetValue(triggerTypes[selectedIndex]);
            if (reset && isEditing)
            {
                FinalizeEdit();
                isEditing = false;
            }
            ApplyTemp();
        }

        public void ToggleDropdown()
        {
            SetDropdown(!isOpened);
        }

        public void SetDropdown(bool to)
        {
            dropdownMenu.SetActive(to);
            isOpened = to;
        }

        public void AddListItem(int item, int index)
        {
            var instance = Instantiate(itemInstance, itemParent);
            var inputField = instance.GetComponentInChildren<InputField>();
            var indexText = instance.transform.Find("IndexText").GetComponent<Text>();
            inputField.text = item.ToString();
            inputField.onEndEdit.AddListener((val) =>
            {
                var o = GetValue();
                var list = (List<int>)o;
                var ind = int.Parse(indexText.text);
                list[ind] = Convert.ToInt32(TryParse(inputField, list[ind], true));
                SetValue(list);
                ApplyInputs(true);
            });
            indexText.text = index.ToString();
            instance.transform.Find("RemoveButton").GetComponent<Button>().onClick.AddListener(() =>
            {
                var o = GetValue();
                var list = (List<int>)o;
                var ind = int.Parse(indexText.text);
                list.RemoveAt(ind);
                SetValue(list);
                ApplyInputs(true);
                instances.Remove(instance);
                Destroy(instance);
                UpdateIndexes();
            });
            instances.Add(instance);
        }

        public void UpdateIndexes()
        {
            for (int i = 0; i < instances.Count; i++)
            {
                instances[i].transform.Find("IndexText").GetComponent<Text>().text = i.ToString();
            }
        }

        public void AddValueList()
        {
            try
            {
                var o = GetValue();
                var list = (List<int>)o;
                var intt = int.Parse(addField.text);
                if (!allowMultiple && list.Contains(intt))
                {
                    ExtDialogManager.Alert("Cannot add the same value");
                    return;
                }
                else
                {
                    list.Add(intt);
                    SetValue(list);
                    ApplyInputs(true);
                }
            }
            catch
            {
                ExtDialogManager.Alert("Invalid value");
            }
        }
    }
}