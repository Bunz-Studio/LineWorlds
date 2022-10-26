using System;
using UnityEngine.UI;

namespace ExternMaker
{
    public class ExtInsFloat : ExtFieldInspect
    {
        public override Type type => typeof(float);

        // public Text fieldInput;

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
            if (source != null || isStatic)
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
            try
            {
                return AreValuesSimilar() ? ((float)values[0]).ToString() : "-";
            }
            catch
            {
                return "0";
            }
        }

        public void ApplyInputs(bool reset = false)
        {
            if (isUpdatingField) return;
            if(!isEditing && !reset)
            {
                isEditing = true;
                StartEdit();
            }
        	var t = GetTemp<float>();
        	SetValue(TryParse(fieldInput, t, reset));
            if (reset && isEditing)
            {
                FinalizeEdit();
                isEditing = false;
            }
        }
    }
}