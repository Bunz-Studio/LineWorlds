using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ExternMaker
{
    public class ExtInsEnum : ExtFieldInspect
    {
        public override Type type => typeof(Enum);
        
        // public Text fieldInput;

        public Button fieldDropper;
        public Text fieldShow;
        public GameObject dropdownMenu;
        public GameObject itemInstance;
        public Transform itemParent;
        public List<GameObject> instances = new List<GameObject>();
        public Dictionary<int, object> triggerTypes = new Dictionary<int, object>();
        public Dictionary<object, int> triggerTypesIndex = new Dictionary<object, int>();

        public Type enumType;
        
        bool isEditing;
        public int selectedIndex;
        public bool isOpened;

        public override void Initialize()
        {
            base.Initialize();

            fieldDropper.onClick.AddListener(() => ToggleDropdown());
            int i = 0;
            enumType = propertyInfo.type;
            foreach (var name in Enum.GetNames(enumType))
            {
                if (!triggerTypes.ContainsKey(i))
                {
                    triggerTypes.Add(i, Enum.Parse(enumType, name));
                    triggerTypesIndex.Add(Enum.Parse(enumType, name), i);
                }
                AddEnumItem(name, i);
                i++;
            }
        }

        public override void SetAdditionalObject(GameObject obj)
        {
            base.SetAdditionalObject(obj);
            dropdownMenu = obj;
            itemParent = obj.transform.Find("Viewport").Find("Content");
        }

        public void FinishEdit()
        {
            ApplyInputs(true);
        }

        public override void ApplyTemp()
        {
            if (source != null || isStatic)
            {
                var o = GetValues();
                if (AreValuesSimilar())
                {
                    var ind = triggerTypesIndex.ContainsKey(o[0]) ? triggerTypesIndex[o[0]] : -1;
                    base.ApplyTemp();
                    if (ind > -1)
                    {
                        var t = Enum.GetName(enumType, o[0]);

                        selectedIndex = ind;
                        fieldShow.text = ExtInsModTrigger.AddSpacesToSentence(t.ToString(), true);
                    }
                    else
                    {
                        fieldShow.text = "(Invalid Enum)";
                    }
                }
                else
                {
                    fieldShow.text = "-";
                }
            }
        }

        public void ApplyInputs(bool reset = false)
        {
            if (!isEditing)
            {
                isEditing = true;
                StartEdit();
            }
            SetValue(triggerTypes[selectedIndex]);
            if (reset && isEditing)
            {
                FinalizeEdit();
                isEditing = false;
            }
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

        public void AddEnumItem(string item, int index)
        {
            var instance = Instantiate(itemInstance, itemParent);
            instance.GetComponent<Button>().onClick.AddListener(() =>
            {
                selectedIndex = index;
                fieldShow.text = ExtInsModTrigger.AddSpacesToSentence(Enum.GetName(enumType, triggerTypes[index]), true);
                SetDropdown(false);
                ApplyInputs(true);
            });
            instance.GetComponentInChildren<Text>().text = ExtInsModTrigger.AddSpacesToSentence(item, true);
        }

        private void OnDestroy()
        {
            Destroy(dropdownMenu);
        }
    }
}