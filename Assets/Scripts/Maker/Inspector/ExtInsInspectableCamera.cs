using System;
using System.Collections.Generic;
using UnityEngine;

namespace ExternMaker
{
    public class ExtInsInspectableCamera : ExtInspectable<InspectableCamera>
    {
        public List<InspectableCameraField> inspectFields = new List<InspectableCameraField>();
        bool isInitialized;

        void Start()
        {
            var tr = typeof(InspectableCamera);
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

        public override void UpdateInspector(InspectableCamera obj)
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
            var cams = GetAllComponents(objs);
            inspectedObjects = cams;
            if (objs.Count > 0)
            {
                var srcs = new List<object>(cams);
                foreach (var insp in inspectFields)
                {
                    insp.extFieldInspect.UpdateField(srcs);
                }
                base.UpdateInspector(objs);
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
        public class InspectableCameraField : InspectField<InspectableCamera>
        {
        }
    }
}