using System;
using System.Collections.Generic;
using UnityEngine;

namespace ExternMaker
{
    public class ExtInsTriggerCollection : ExtInspectable<TriggerCollection>
    {
        public ExtInsEnum triggerTypeIns;
        public Transform newOrder;
        public Dictionary<TriggerCollection.TrigType, int> triggerTypes = new Dictionary<TriggerCollection.TrigType, int>();

        public List<NewTriggerGroup> newInspectorGroups = new List<NewTriggerGroup>();
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

            foreach(var insp in newInspectorGroups)
            {
                insp.Initialize(newOrder);
                insp.SetActive(false);
            }

            /*foreach (var insp in inspectorGroups)
            {
                insp.Initialize();
            }*/

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
            if (inspectedObjects.Count > 0)
            {
                var index = GetTriggerTypeIndex();
                if (index < 0) return;
                var appropriate = new List<NewTriggerGroup>();
                for (int i = 0; i < newInspectorGroups.Count; i++)
                {
                    if (i == index)
                    {
                        appropriate.Add(newInspectorGroups[i]);
                    }
                    else
                    {
                        newInspectorGroups[i].SetActive(false);
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
            if (inspectedObjects.Count > 0)
            {
                var index = GetTriggerTypeIndex();
                if (index < 0) return;
                for (int i = 0; i < newInspectorGroups.Count; i++)
                {
                    newInspectorGroups[i].Update(inspectedObjects);
                }
            }
        }

        public int GetTriggerTypeIndex()
        {
            if (inspectedObjects.Count < 1) return -1;
            TriggerCollection.TrigType type = inspectedObjects[0].TriggerTypes;
            foreach(var obj in inspectedObjects)
            {
                if (obj.TriggerTypes != type) return -1;
            }
            return triggerTypes.ContainsKey(type) ? triggerTypes[type] : -1;
        }

        public override void SetInspectorAs(bool active)
        {
            base.SetInspectorAs(active);
            triggerTypeIns.gameObject.SetActive(active);
            if (!active)
            {
                foreach (var insp in newInspectorGroups)
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
        public class NewTriggerGroup
        {
            public string groupName;
            public List<InfoField> infos = new List<InfoField>();
            public List<CustomInspectField> inspectFields = new List<CustomInspectField>();

            public void Initialize(Transform self)
            {
                for(int i = infos.Count; i > 0; i--)
                {
                    var info = infos[i - 1];
                    var ins = info.GetInstance(typeof(TriggerCollection), self, 3);
                    if (ins != null)
                    {
                        inspectFields.Add(ins);
                    }
                }

                /*foreach (var insp in inspectFields)
                {
                    insp.Initialize();
                }*/
            }

            public void Update(List<TriggerCollection> objs)
            {
                foreach (var insp in inspectFields)
                {
                    if (objs != null && objs.Count > 0) insp.extFieldInspect.sources = new List<object>(objs);
                    insp.extFieldInspect.ApplyTemp();
                    insp.extFieldInspect.UpdateField(new List<object>(objs));
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