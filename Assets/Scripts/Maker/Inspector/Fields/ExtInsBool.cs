using System;
using UnityEngine.UI;

namespace ExternMaker
{
    public class ExtInsBool : ExtFieldInspect
    {
        public override Type type => typeof(bool);

        public Toggle fieldToggle;
        public UnityEngine.GameObject allDifferent;

        bool isEditing;

        public override void Initialize()
        {
            base.Initialize();

            fieldToggle.onValueChanged.AddListener(FinishEdit);
        }

        public void FinishEdit(bool val)
        {
            ApplyInputs(true, val);
        }

        public override void ApplyTemp()
        {
            if (source != null || isStatic)
            {
                values = GetValues();
                isUpdatingField = true;
                if (AreValuesSimilar())
                {
                    var t = (bool)values[0];
                    base.ApplyTemp();

                    fieldToggle.isOn = t;
                    allDifferent.SetActive(false);
                }
                else
                {
                    fieldToggle.isOn = false;
                    allDifferent.SetActive(true);
                }
                isUpdatingField = false;
            }
        }

        public void ApplyInputs(bool reset = false, bool val = false)
        {
            if (isUpdatingField) return;
            if (!isEditing && !reset)
            {
                isEditing = true;
                StartEdit();
            }
            SetValue(val);
            if (reset && isEditing)
            {
                FinalizeEdit();
                isEditing = false;
            }
        }
    }
}