using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ExternMaker
{
    public class ExtInsArrayMaterial : ExtFieldInspect
    {
        public override Type type => typeof(Material[]);

        public Button fieldDropper;
        public Text fieldShow;
        public GameObject dropdownMenu;
        public GameObject itemInstance;
        public Transform itemParent;
        public List<GameObject> instances = new List<GameObject>();

        public Color addColor = Color.white;
        public ExtInsColor colorField;
        public bool allowMultiple;

        public Type enumType;

        bool isEditing;
        public int selectedIndex;
        public bool isOpened;

        public override void Initialize()
        {
            base.Initialize();

            colorField.propertyInfo = new ExtProperty(typeof(ExtInsArrayMaterial), "addColor");
            colorField.Initialize();
            colorField.UpdateField(new List<object>(new object[] { this }));
            fieldDropper.onClick.AddListener(() => ToggleDropdown());
        }

        public override void SetAdditionalObject(GameObject obj)
        {
            base.SetAdditionalObject(obj);
            dropdownMenu = obj;
            itemParent = obj.transform.Find("GroupIDDropdown").Find("Viewport").Find("Content");
            colorField = obj.transform.Find("Panel").Find("ColorField").GetComponent<ExtInsColor>();
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
                var list = (Material[])o;
                base.ApplyTemp();
                ClearInstances();

                int i = 0;
                string show = null;
                try
                {
                    foreach (var mat in list)
                    {
                        if (!mat.HasProperty("_Color"))
                        {
                            i++;
                            continue;
                        }
                        show += string.IsNullOrEmpty(show) ? ToHex(mat) : ", " + ToHex(mat);
                        AddListItem(mat, i);
                        i++;
                    }
                }
                catch
                {

                }
                fieldShow.text = show;
            }
        }

        public static string ToHex(Material mat)
        {
            return ToHex(mat.color);
        }

        public static string ToHex(Color color)
        {
            return string.Format("{0:X2}{1:X2}{2:X2}", 
                Convert.ToInt32(color.r * 255),
                Convert.ToInt32(color.g * 255),
                Convert.ToInt32(color.b * 255));
        }

        public void ClearInstances()
        {
            foreach (var inst in instances)
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

        public void AddListItem(Material item, int index)
        {
            if (!item.HasProperty("_Color")) return;
            var instance = Instantiate(itemInstance, itemParent);
            var material = instance.GetComponent<ExtInsMaterial>();
            var indexText = instance.transform.Find("IndexText").GetComponent<Text>();
            material.sourceless = true;
            material.values = new List<object>(new object[] { item });
            material.ApplyTemp();
            material.fieldUpdate += (val) =>
            {
                var o = GetValue();
                var list = (Material[])o;
                var ind = int.Parse(indexText.text);
                list[ind] = (Material)val;
                SetValue(list);
                ApplyInputs(false);
            };
            material.fieldFinishEdit += (val) =>
            {
                var o = GetValue();
                var list = (Material[])o;
                var ind = int.Parse(indexText.text);
                list[ind] = (Material)val;
                SetValue(list);
                ApplyInputs(true);
            };
            indexText.text = index.ToString();
            instance.transform.Find("RemoveButton").GetComponent<Button>().onClick.AddListener(() =>
            {
                var o = GetValue();
                var list = (Material[])o;
                var plist = new List<Material>(list);
                var ind = int.Parse(indexText.text);
                plist.RemoveAt(ind);
                SetValue(plist.ToArray());
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
                var list = (Material[])o;
                var plist = new List<Material>(list);
                var intt = addColor;
                var mat = new Material(ExtInspector.instance.materialSelector.shaders[0]);
                mat.color = intt;
                if (mat.HasProperty("_Glossiness"))
                {
                    mat.SetFloat("_Glossiness", 0);
                }
                plist.Add(mat);
                SetValue(plist.ToArray());
                ApplyInputs(true);
            }
            catch
            {
                ExtDialogManager.Alert("Invalid value");
            }
        }
    }
}