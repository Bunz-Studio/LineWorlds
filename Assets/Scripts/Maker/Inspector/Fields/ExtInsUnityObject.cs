using System;
using UnityEngine;
using UnityEngine.UI;

namespace ExternMaker
{
    public class ExtInsUnityObject : ExtFieldInspect
    {
        public override Type type => typeof(UnityEngine.Object);
        
        public InputField fieldShow;
        public string selectorType;
        public ExtObjectSelector selector;

        bool isEditing;
        public override void Initialize()
        {
            base.Initialize();
            
            fieldShow.readOnly = true;
        }

        public void FinishEdit()
        {
            ApplyInputs(true);
        }

        public override void ApplyTemp()
        {
        	if(source != null || isStatic)
        	{
                var v = GetValues();
                base.ApplyTemp();
                if (AreValuesSimilar())
                {
                    var t = (UnityEngine.Object)v[0];
                    if (t != null)
                    {
                        fieldShow.text = GetViewString();
                    }
                    else
                    {
                        fieldShow.text = "None";
                    }
                }
                else
                {
                    fieldShow.text = "-";
                }
        	}
        }

        public string GetViewString()
        {
            var v = values[0];
            if (AreValuesSimilar())
            {
                try
                {
                    return v == null ? "None" : ((UnityEngine.Object)v).name;
                }
                catch
                {
                    return "None";
                }
            }
            else
            {
                return "-";
            }
        }

        public void ApplyInputs(bool reset = false)
        {
            SetValue(fieldShow.text);
        }
        
        public void OpenSelector()
        {
            if (propertyInfo.type.FullName == typeof(GameObject).FullName)
            {
                if (selector == null) selector = ExtInspector.instance.gameObjectSelector;
            }
            else
            {
                if (selector == null) selector = ExtInspector.instance.objectReferenceSelector;
            }
            selector.inspectedType = GetInspectedType();
            selector.Initialize((UnityEngine.Object)GetValue(), this);
        }
        
        public void SelectObject(UnityEngine.Object obj)
        {
            if (!isEditing)
            {
                StartEdit();
                isEditing = true;
            }
            SetValue(obj);
        	ApplyTemp();
            fieldShow.text = obj.name;
        }
        
        public void EndObject(UnityEngine.Object obj)
        {
        	SetValue(obj);
        	ApplyTemp();
            if (isEditing)
            {
                FinalizeEdit();
                isEditing = false;
            }
            fieldShow.text = obj.name;
        }
    }
}