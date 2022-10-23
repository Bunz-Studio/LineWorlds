using System;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LineWorldsMod;

namespace ExternMaker
{
    public class ExtInsMonoBehaviour : ExtInspectableRaw<MonoBehaviour>
    {
        public Text componentNameText;
        public string typeName;
        public bool onlyShowWithAttribute;
        public Transform publicTransform;
        public List<InspectMatch> matchFields = new List<InspectMatch>();
        public List<ExtFieldInspect> fieldInspects = new List<ExtFieldInspect>();

        public List<CustomInspectField> inspectFields = new List<CustomInspectField>();
        public object currentObjectType;
        bool isInitialized;

        void Start()
        {
            var tr = typeof(MonoBehaviour);
            if (!isInitialized) InitializeFields();
        }
        
        public override void PushIfCorrect(object obj)
        {
            if (obj.GetType().FullName == typeName)
            {
                UpdateInspector((MonoBehaviour)obj);
            }
        }

        public override void PushObject(GameObject obj)
        {
            var a = obj.GetComponents(typeof(MonoBehaviour));
            var r = GetValidComponent(a);
            if (r != null)
            {
                SetInspectorAs(true);
                UpdateInspector((MonoBehaviour)(object)r);
            }
            else
            {
                SetInspectorAs(false);
            }
        }

        public Component GetValidComponent(Component[] collection)
        {
            foreach(var col in collection)
            {
                if (col.GetType().FullName == typeName) return col;
            }
            return null;
        }

        public void InitializeFields()
        {
            foreach (var insp in inspectFields)
            {
                insp.Initialize();
            }
            isInitialized = true;
        }

        public void InitializeFieldsType()
        {
            foreach (var inspect in fieldInspects)
            {
                Destroy(inspect.gameObject);
            }
            fieldInspects.Clear();
            inspectFields.Clear();

            // Debug.Log("Cleared list: " + currentObjectType.GetType().FullName);
            if (currentObjectType.GetType() != null)
            {
                int i = 0;
                // Debug.Log("Object isn't null: " + currentObjectType.GetType().FullName);
                foreach (var field in currentObjectType.GetType().GetFields())
                {
                    // Debug.Log("Trying to get field for: " + currentObjectType.GetType().GetType().FullName + " - with name: " + field.Name);
                    var insp = GetInspect(field.FieldType);
                    if (insp != null)
                    {
                        // Debug.Log("Field found for: " + field.Name + " - and type: " + field.FieldType.Name);
                        var inspF = new CustomInspectField(currentObjectType.GetType());
                        inspF.fieldName = field.Name;
                        if (!inspF.IsHidden(onlyShowWithAttribute))
                        {
                            var inspInst = Instantiate(insp.extFieldInspect.gameObject, publicTransform).GetComponent<ExtFieldInspect>();
                            inspInst.gameObject.SetActive(true);
                            inspInst.transform.SetSiblingIndex(transform.GetSiblingIndex() + 1 + i);
                            i++;
                            var isEnum = inspInst.GetComponent<ExtInsEnum>();
                            if (isEnum != null)
                            {
                                var inspMenu = Instantiate(insp.additionalInspect, publicTransform);
                                inspMenu.SetActive(false);
                                inspMenu.transform.SetSiblingIndex(transform.GetSiblingIndex() + 1 + i);
                                i++;
                                isEnum.dropdownMenu = inspMenu;
                                isEnum.itemParent = inspMenu.transform.Find("Viewport").Find("Content");
                            }
                            inspF.extFieldInspect = inspInst;
                            inspF.fieldName = field.Name;
                            inspF.extFieldInspect.fieldText.text = AddSpacesToSentence(field.Name, true);
                            inspF.Initialize();
                            fieldInspects.Add(inspInst);
                            inspectFields.Add(inspF);
                        }
                    }
                }
            }
        }

        public override void UpdateInspector(MonoBehaviour obj)
        {
            /* if (!isInitialized) InitializeFields();
            inspectedObject = obj;
            if (obj != null)
            {
                componentNameText.text = AddSpacesToSentence(obj.GetType().Name, true);
                if (currentObjectType == null || currentObjectType.GetType().FullName != obj.GetType().FullName)
                {
                    // Debug.Log("ChangingType... : " + obj.GetType().FullName);
                    currentObjectType = obj;
                    InitializeFieldsType();
                }

                foreach (var insp in inspectFields)
                {
                    insp.extFieldInspect.source = obj;
                    try
                    {
                        insp.extFieldInspect.ApplyTemp();
                    }
                    catch
                    {
                        currentObjectType = obj;
                        InitializeFieldsType();
                        UpdateInspector(obj);
                    }
                }

                base.UpdateInspector(obj);
            }*/
        }

        public override void UpdateIfExist()
        {
            if (!isInitialized) InitializeFields();
            if (inspectedObject != null)
            {
                componentNameText.text = AddSpacesToSentence(inspectedObject.GetType().Name, true);
                if (currentObjectType == null || currentObjectType.GetType().FullName != inspectedObject.GetType().FullName)
                {
                    // Debug.Log("ChangingType... : " + inspectedObject.GetType().FullName);
                    currentObjectType = inspectedObject;
                    InitializeFieldsType();
                }

                foreach (var insp in inspectFields)
                {
                    insp.extFieldInspect.ApplyTemp();
                }

                base.UpdateInspector(inspectedObject);
            }
        }

        public override void SetInspectorAs(bool active)
        {
            base.SetInspectorAs(active);
            foreach (var insp in inspectFields)
            {
                insp.extFieldInspect.gameObject.SetActive(active);
                var enumIns = insp.extFieldInspect.GetComponent<ExtInsEnum>();
                if (enumIns != null)
                {
                    enumIns.dropdownMenu.SetActive(active ? enumIns.dropdownMenu.activeSelf : active);
                }
            }
        }

        public InspectMatch GetInspect(Type type)
        {
            foreach (var m in matchFields)
            {
                if (m.typeFullName == type.FullName) return m;
                var t = Type.GetType(m.typeFullName);
                if (t == null)
                {
                    // Debug.Log(m.typeFullName + " doesn't exist");
                    continue;
                }
                if (type.IsSubclassOf(t)) return m;
            }
            return null;
        }

        public static string AddSpacesToSentence(string text, bool preserveAcronyms)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            text = text.Substring(0, 1).ToUpper() + text.Substring(1);
            StringBuilder newText = new StringBuilder(text.Length * 2);
            newText.Append(text[0]);
            for (int i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]))
                    if ((text[i - 1] != ' ' && !char.IsUpper(text[i - 1])) ||
                        (preserveAcronyms && char.IsUpper(text[i - 1]) &&
                         i < text.Length - 1 && !char.IsUpper(text[i + 1])))
                        newText.Append(' ');
                newText.Append(text[i]);
            }
            return newText.ToString();
        }

        [Serializable]
        public class InspectMatch
        {
            public string typeFullName;
            public ExtFieldInspect extFieldInspect;
            public UnityEngine.GameObject additionalInspect;
        }
    }
}