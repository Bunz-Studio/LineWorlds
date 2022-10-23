using System;
using System.Collections.Generic;
using UnityEngine;

namespace ExternMaker
{
    public class ExtInsLineMovement : ExtInspectable<LineMovement>
    {
        public List<InspectableLineField> inspectFields = new List<InspectableLineField>();
        bool isInitialized;

        void Start()
        {
            var tr = typeof(LineMovement);
            if (!isInitialized) InitializeFields();
        }

        public void InitializeFields()
        {
            foreach (var insp in inspectFields)
            {
                insp.Initialize();
            }
            isInitialized = true;
        }

        public override void UpdateInspector(LineMovement obj)
        {
            /*if (!isInitialized) InitializeFields();
            inspectedObject = obj;
            if (obj != null)
            {
                foreach (var insp in inspectFields)
                {
                    insp.extFieldInspect.source = obj;
                    insp.extFieldInspect.ApplyTemp();
                }

                base.UpdateInspector(obj);
            }*/
        }

        public override void UpdateInspector(List<GameObject> objs)
        {
            base.UpdateInspector(objs);
            var lines = GetAllComponents(objs);
            inspectedObjects = lines;
            if (objs.Count > 0)
            {
                var srcs = new List<object>(lines);
                foreach (var insp in inspectFields)
                {
                    insp.extFieldInspect.UpdateField(srcs);
                }
            }
        }

        public override void UpdateIfExist()
        {
            if (!isInitialized) InitializeFields();
            if (inspectedObject != null)
            {
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
            }
        }

        [Serializable]
        public class InspectableLineField : InspectField<LineMovement>
        {
        }
    }
}