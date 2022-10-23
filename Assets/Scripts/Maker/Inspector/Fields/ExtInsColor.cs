using System;
using UnityEngine;
using UnityEngine.UI;

namespace ExternMaker
{
    public class ExtInsColor : ExtFieldInspect
    {
        public override Type type => typeof(Color);
        public Image fieldImage;
        public Text fieldColorValues;
        
        public ExtColorSelector selector;
        
        public override void ApplyTemp()
        {
            values = GetValues();
            if (AreValuesSimilar())
            {
                var t = (Color)values[0];
                base.ApplyTemp();

                fieldImage.color = t;
                fieldColorValues.text = (t.r * 255).ToString("0") + ", " + (t.g * 255).ToString("0") + ", " + (t.b * 255).ToString("0") + ", " + (t.a * 255).ToString("0");
            }
            else
            {
                fieldImage.color = Color.black;
                fieldColorValues.text = "-";
            }
        }
        
        public void OpenSelector()
        {
            if (selector == null) selector = ExtInspector.instance.colorSelector;
        	selector.Initialize((Color)GetValue(), this);
        }
        bool isEditing;

        public void SelectObject(Color obj)
        {
            if (!isEditing)
            {
                StartEdit();
                isEditing = true;
            }
        	SetValue(obj);
        	ApplyTemp();
        }
        
        public void EndObject(Color obj)
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