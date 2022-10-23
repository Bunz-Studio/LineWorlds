using System;
using UnityEngine.UI;
using SimpleFileBrowser;

namespace ExternMaker
{
    public class ExtInsString : ExtFieldInspect
    {
        public override Type type => typeof(string);

        public InputField fieldInput;

        bool isEditing;

        public override void Initialize()
        {
            base.Initialize();
            
            fieldInput.onEndEdit.AddListener(val => FinishEdit());

            fieldInput.onValueChanged.AddListener(val => ApplyInputs(false));
        }

        public void FinishEdit()
        {
            ApplyInputs(true);
        }

        public override void ApplyTemp()
        {
        	if(source != null || isStatic)
            {
                values = GetValues();
                var t = GetViewString();
                base.ApplyTemp();

                isUpdatingField = true;
                fieldInput.text = t;
                isUpdatingField = false;
            }
        }

        public string GetViewString()
        {
            return AreValuesSimilar() ? (string)values[0] : "-";
        }

        public void ApplyInputs(bool reset = false)
        {
            if (isUpdatingField) return;
            if (!isEditing && !reset)
            {
                isEditing = true;
                StartEdit();
            }
            SetValue(fieldInput.text);
            if (reset && isEditing)
            {
                FinalizeEdit();
                isEditing = false;
            }
        }
    }
}