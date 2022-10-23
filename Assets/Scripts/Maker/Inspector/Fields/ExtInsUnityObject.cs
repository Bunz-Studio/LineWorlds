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
            return AreValuesSimilar() ? ((UnityEngine.Object)values[0]).name : "-";
        }

        public void ApplyInputs(bool reset = false)
        {
            SetValue(fieldShow.text);
        }
        
        public void OpenSelector()
        {
            if (selector == null) selector = (ExtObjectSelector)FindObjectOfType(Type.GetType(selectorType));
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
        }
    }
}