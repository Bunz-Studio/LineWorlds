using System;
using UnityEngine;
using UnityEngine.UI;

namespace ExternMaker
{
    public class ExtInsMaterial : ExtFieldInspect
    {
        public override Type type => typeof(Material);
        public Image fieldImage;
        public Text fieldColorValues;

        public ExtMaterialSelector selector;

        public override void ApplyTemp()
        {
            isUpdatingField = true;
            values = GetValues();
            if (AreValuesSimilar())
            {
                var t = ((Material)values[0]).color;
                base.ApplyTemp();

                fieldImage.color = t;
                fieldColorValues.text = (t.r * 255).ToString("0") + ", " + (t.g * 255).ToString("0") + ", " + (t.b * 255).ToString("0") + ", " + (t.a * 255).ToString("0");
            }
            else
            {
                fieldImage.color = Color.black;
                fieldColorValues.text = "-";
            }
            isUpdatingField = false;
        }

        public void OpenSelector()
        {
            ExtInspector.instance.materialSelector.Initialize((Material)GetValue(), this);
        }
        bool isEditing;

        public void SelectObject(Material obj)
        {
            if (!isEditing)
            {
                StartEdit();
                isEditing = true;
            }
            SetValue(obj);
            fieldUpdate.InvokeOnExist(obj);
            ApplyTemp();
        }

        public void EndObject(Material obj)
        {
            SetValue(obj);
            ApplyTemp();
            fieldFinishEdit.InvokeOnExist(obj);
            if (isEditing)
            {
                FinalizeEdit();
                isEditing = false;
            }
        }
    }
}