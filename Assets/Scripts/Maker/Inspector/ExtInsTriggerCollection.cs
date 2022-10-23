using System;
using System.Collections.Generic;
using UnityEngine;

namespace ExternMaker
{
    public class ExtInsTriggerCollection : ExtInspectable<TriggerCollection>
    {
        public ExtInsEnum triggerTypeIns;
        public Dictionary<TriggerCollection.TrigType, int> triggerTypes = new Dictionary<TriggerCollection.TrigType, int>();
        public List<TriggerGroup> inspectorGroups = new List<TriggerGroup>();
        bool isInitialized;

        void Start()
        {
            var tr = typeof(TriggerCollection);
            if (!isInitialized) InitializeFields();
        }

        public void InitializeFields()
        {
            triggerTypeIns.propertyInfo = new ExtProperty(typeof(TriggerCollection), "TriggerTypes");
            triggerTypeIns.Initialize();
            triggerTypeIns.fieldFinishEdit += (obj) =>
            {
                UpdateIfExist();
            };
            foreach (var insp in inspectorGroups)
            {
                insp.Initialize();
            }

            int i = 0;
            foreach (var name in Enum.GetNames(typeof(TriggerCollection.TrigType)))
            {
                triggerTypes.Add((TriggerCollection.TrigType)Enum.Parse(typeof(TriggerCollection.TrigType), name), i);
                i++;
            }

            isInitialized = true;
        }

        public override void UpdateInspector(List<GameObject> objs)
        {
            var components = GetAllComponents(objs);
            inspectedObjects = components;
            if (objs.Count > 0)
            {
                var srcs = new List<object>(components);
                if (gameObject.activeSelf)
                {
                    OpenAppropiateFields();
                    UpdateAppropiateFields();
                }
                triggerTypeIns.UpdateField(srcs);
                base.UpdateInspector(objs);
            }
        }

        public override void UpdateIfExist()
        {
            if (!isInitialized) InitializeFields();
            if (inspectedObject != null)
            {
                if (gameObject.activeSelf)
                {
                    OpenAppropiateFields();
                    UpdateAppropiateFields();
                }
                triggerTypeIns.sources = new List<object>(inspectedObjects);
                triggerTypeIns.ApplyTemp();

                base.UpdateInspector(inspectedObject);
            }
        }

        public void OpenAppropiateFields()
        {
            if(inspectedObject != null)
            {
                var index = triggerTypes.ContainsKey(inspectedObject.TriggerTypes) ? triggerTypes[inspectedObject.TriggerTypes] : 0;
                var appropriate = new List<TriggerGroup>();
                for (int i = 0; i < inspectorGroups.Count; i++)
                {
                    if (i == index)
                    {
                        appropriate.Add(inspectorGroups[i]);
                    }
                    else
                    {
                        inspectorGroups[i].SetActive(false);
                    }
                }
                foreach (var a in appropriate)
                {
                    a.SetActive(true);
                }
            }
        }

        public void UpdateAppropiateFields()
        {
            if (inspectedObject != null)
            {
                var index = triggerTypes.ContainsKey(inspectedObject.TriggerTypes) ? triggerTypes[inspectedObject.TriggerTypes]: 0;
                for (int i = 0; i < inspectorGroups.Count; i++)
                {
                    inspectorGroups[i].Update(inspectedObjects);
                }
            }
        }

        public override void SetInspectorAs(bool active)
        {
            base.SetInspectorAs(active);
            triggerTypeIns.gameObject.SetActive(active);
            if (!active)
            {
                foreach (var insp in inspectorGroups)
                {
                    insp.SetActive(active);
                }
            }
            else
            {
                OpenAppropiateFields();
            }
        }

        [Serializable]
        public class InspectableTriggerField : InspectField<TriggerCollection>
        {
        }

        [Serializable]
        public class TriggerGroup
        {
            public string groupName;
            public List<InspectableTriggerField> inspectFields = new List<InspectableTriggerField>();

            public void Initialize()
            {
                foreach (var insp in inspectFields)
                {
                    insp.Initialize();
                }
            }

            public void Update(List<TriggerCollection> objs)
            {
                foreach (var insp in inspectFields)
                {
                    if (objs != null && objs.Count > 0) insp.extFieldInspect.sources = new List<object>(objs);
                    insp.extFieldInspect.ApplyTemp();
                }
            }

            public void SetActive(bool to)
            {
                foreach (var insp in inspectFields)
                {
                    insp.extFieldInspect.gameObject.SetActive(to);
                    var enumIns = insp.extFieldInspect.GetComponent<ExtInsEnum>();
                    if (enumIns != null)
                    {
                        enumIns.dropdownMenu.SetActive(to ? enumIns.dropdownMenu.activeSelf : to);
                    }
                }
            }
        }
    }
}