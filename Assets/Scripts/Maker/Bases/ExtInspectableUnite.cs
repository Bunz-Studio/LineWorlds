using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using LineWorldsMod;

namespace ExternMaker
{
	public class ExtInspectableUnite : MonoBehaviour
	{
		public virtual void PushIfCorrect(object obj)
		{
		}
		
		public virtual void PushObject(GameObject obj)
		{
        }

        public virtual void PushObjects(List<GameObject> objs)
        {
        }

        public virtual void UpdateIfExist()
		{
			
		}
		
		public virtual void ClearInspector()
		{
			
		}
		
		public virtual void SetInspectorAs(bool active)
		{
			gameObject.SetActive(active);
		}
		
		public static PropertyInfo GetProperty(object obj, string name)
		{
			return obj.GetType().GetProperty(name);
        }

        [System.Serializable]
        public class InspectField<T>
        {
            public string fieldName;
            public ExtFieldInspect extFieldInspect;

            [System.NonSerialized]
            public PropertyInfo propertyInfo;
            [System.NonSerialized]
            public FieldInfo fieldInfo;

            public void Initialize()
            {
                var t = typeof(T);
                var prop = new ExtProperty(t, fieldName);
                extFieldInspect.propertyInfo = prop;
                extFieldInspect.Initialize();
            }
        }

        [Serializable]
        public class CustomInspectField
        {
            public string fieldName;
            public Type type;
            public ExtFieldInspect extFieldInspect;

            [NonSerialized]
            public PropertyInfo propertyInfo;
            [NonSerialized]
            public FieldInfo fieldInfo;

            public CustomInspectField()
            {

            }

            public CustomInspectField(Type type)
            {
                this.type = type;
            }

            public bool IsHidden(bool onlyShowWithAttribute)
            {
                var t = type;
                propertyInfo = t.GetProperty(fieldName);
                if (propertyInfo == null)
                {
                    fieldInfo = t.GetField(fieldName);
                    if (onlyShowWithAttribute)
                    {
                        return fieldInfo.GetCustomAttribute(typeof(ShowInCustomInspector)) == null;
                    }
                    else
                    {
                        return fieldInfo.GetCustomAttribute(typeof(HideFromInspectorAttribute)) != null ||
                            fieldInfo.GetCustomAttribute(typeof(HideInInspector)) != null;
                    }
                }
                else
                {
                    if (onlyShowWithAttribute)
                    {
                        return propertyInfo.GetCustomAttribute(typeof(ShowInCustomInspector)) == null;
                    }
                    else
                    {
                        return propertyInfo.GetCustomAttribute(typeof(HideFromInspectorAttribute)) != null ||
                            propertyInfo.GetCustomAttribute(typeof(HideInInspector)) != null;
                    }
                }
            }

            public void Initialize()
            {
                var t = type;
                extFieldInspect.propertyInfo = new ExtProperty(t, fieldName);
                extFieldInspect.Initialize();
            }
        }

        [System.Serializable]
        public class InfoField
        {
            public string fieldName;
            public string fieldCustomText;

            public CustomInspectField GetInstance(Type type, Transform self)
            {
                int i = 0;
                var prop = new ExtProperty(type, fieldName);
                var insp = ExtInspector.instance.settings.GetMatch(prop.type);
                if (insp != null)
                {
                    var inspF = new CustomInspectField(type);
                    string name = string.IsNullOrWhiteSpace(fieldCustomText) ? AddSpacesToSentence(prop.name, true) : fieldCustomText;
                    inspF.fieldName = prop.name;
                    if (!inspF.IsHidden(false))
                    {
                        var inspInst = Instantiate(insp.extFieldInspect.gameObject, ExtInspector.instance.publicTransform).GetComponent<ExtFieldInspect>();
                        inspInst.gameObject.SetActive(true);
                        inspInst.transform.SetSiblingIndex(self.GetSiblingIndex() + 1 + i);
                        i++;
                        if (insp.additionalInspect != null)
                        {
                            var inspMenu = Instantiate(insp.additionalInspect, ExtInspector.instance.publicTransform);
                            inspMenu.SetActive(false);
                            inspMenu.transform.SetSiblingIndex(self.GetSiblingIndex() + 1 + i);
                            i++;
                            inspInst.SetAdditionalObject(inspMenu);
                        }
                        inspF.extFieldInspect = inspInst;
                        inspF.extFieldInspect.fieldText.text = name;
                        inspF.Initialize();
                        return inspF;
                    }
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
        }
    }
}
