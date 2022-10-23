using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ExternMaker
{
    public class ExtInsVector3 : ExtFieldInspect
    {
        public override Type type => typeof(Vector3);

        public InputField fieldX;
        public InputField fieldY;
        public InputField fieldZ;

        bool isEditing;

        public override void Initialize()
        {
            base.Initialize();

            fieldX.onEndEdit.AddListener(val => FinishEdit(0));
            fieldY.onEndEdit.AddListener(val => FinishEdit(1));
            fieldZ.onEndEdit.AddListener(val => FinishEdit(2));

            fieldX.onValueChanged.AddListener(val => ApplyInputs(0, false));
            fieldY.onValueChanged.AddListener(val => ApplyInputs(1, false));
            fieldZ.onValueChanged.AddListener(val => ApplyInputs(2, false));
        }

        public void FinishEdit(int index)
        {
            ApplyInputs(index, true);
        }

        public override void ApplyTemp()
        {
            if (isUpdatingField) return;
            if (source != null || isStatic)
            {
                values = GetValues();
                base.ApplyTemp();

                isUpdatingField = true;

                fieldX.text = GetViewString(0);
                fieldY.text = GetViewString(1);
                fieldZ.text = GetViewString(2);

                isUpdatingField = false;
            }
        }

        public string GetViewString(int index)
        {
            if (AreVectorAxesSimilar(values, index))
            {
                switch (index)
                {
                    case 0:
                        return ((Vector3)values[0]).x.ToString();
                    case 1:
                        return ((Vector3)values[0]).y.ToString();
                    default:
                        return ((Vector3)values[0]).z.ToString();
                }
            }
            return "-";
        }

        public bool AreVectorAxesSimilar(List<object> values, int index)
        {
            if (values.Count < 1) return false;
            var starter = (Vector3)values[0];
            foreach (var a in values)
            {
                switch (index)
                {
                    case 0:
                        if (((Vector3)a).x != starter.x) return false;
                        break;
                    case 1:
                        if (((Vector3)a).y != starter.y) return false;
                        break;
                    default:
                        if (((Vector3)a).z != starter.z) return false;
                        break;
                }
            }
            return true;
        }

        public void ApplyInputs(int index = 0, bool reset = false)
        {
            if (isUpdatingField) return;

            if (!isEditing && !reset)
            {
                isEditing = true;
                StartEdit();
            }
            var t = GetTemp<Vector3>();
            switch (index)
            {
                case 0:
                    SetValuesOnAxis(TryParse(fieldX, t.x, reset), index);
                    break;
                case 1:
                    SetValuesOnAxis(TryParse(fieldY, t.y, reset), index);
                    break;
                default:
                    SetValuesOnAxis(TryParse(fieldZ, t.z, reset), index);
                    break;
            }
            if (reset && isEditing)
            {
                FinalizeEdit();
                isEditing = false;
            }
        }

        public void SetValuesOnAxis(float value, int axes)
        {
            for (int i = 0; i < sources.Count; i++)
            {
                var vec = (Vector3)values[i];
                switch (axes)
                {
                    case 0:
                        propertyInfo.SetValue(sources[i], new Vector3(value, vec.y, vec.z));
                        break;
                    case 1:
                        propertyInfo.SetValue(sources[i], new Vector3(vec.x, value, vec.z));
                        break;
                    default:
                        propertyInfo.SetValue(sources[i], new Vector3(vec.x, vec.y, value));
                        break;
                }
            }
            values = GetValues();
        }
    }
}