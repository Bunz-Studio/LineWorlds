using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ExternMaker
{
    public class ExtInsComponent : ExtInspectableRaw<Component>
    {
        public string componentTypeName;
        public List<Component> components = new List<Component>();
        public List<InfoField> infos = new List<InfoField>();
        public List<CustomInspectField> inspectFields = new List<CustomInspectField>();
        bool isInitialized;

        private void Start()
        {
            Initialize();
        }

        public void Initialize()
        {
            if (isInitialized) return;
            InitializeFields(transform);
            isInitialized = true;
        }

        public override void PushObjects(List<GameObject> objs)
        {
            Initialize();
            if (AreAllObjectContainComponent(componentTypeName, objs))
            {
                SetInspectorAs(true);
                UpdateInspector(objs);
            }
            else
            {
                SetInspectorAs(false);
            }
            base.PushObjects(objs);
        }

        public bool AreAllObjectContainComponent(string t, List<GameObject> objs)
        {
            foreach (var obj in objs)
            {
                bool isValid = false;
                var comps = obj.GetComponents(typeof(Component));
                foreach (var comp in comps)
                {
                    if (comp.GetType().FullName == t)
                    {
                        isValid = true;
                    }
                }
                if (!isValid)
                {
                    return false;
                }
            }
            return true;
        }

        public void UpdateInspector(List<GameObject> objs)
        {
            Initialize();
            var c = GetAllComponents(objs, componentTypeName);
            components = c;
            if (c.Count > 0)
            {
                UpdateFields(c);
            }
        }

        public override void UpdateIfExist()
        {
            Initialize();
            base.UpdateIfExist();
            if (components.Count > 0)
            {
                UpdateFields(components);
            }
        }

        public List<Component> GetAllComponents(List<GameObject> objs, string type)
        {
            var list = new List<Component>();
            foreach (var obj in objs)
            {
                var comps = obj.GetComponents(typeof(Component));
                foreach(var comp in comps)
                {
                    if (comp != null)
                    {
                        if (comp.GetType().FullName == type) list.Add((Component)(object)comp);
                    }
                }
            }
            return list;
        }

        public override void SetInspectorAs(bool active)
        {
            Initialize();
            base.SetInspectorAs(active);
            SetFieldsActive(active);
        }

        // Fields
        public void InitializeFields(Transform self)
        {
            for (int i = infos.Count; i > 0; i--)
            {
                var info = infos[i - 1];
                var t = System.Type.GetType(componentTypeName);
                if (t == null)
                {
                    t = typeof(Component);
                }
                var ins = info.GetInstance(t, self, 1);
                if (ins != null)
                {
                    inspectFields.Add(ins);
                }
            }
        }

        public void UpdateFields(List<Component> objs)
        {
            foreach (var insp in inspectFields)
            {
                if (objs != null && objs.Count > 0) insp.extFieldInspect.sources = new List<object>(objs);
                insp.extFieldInspect.ApplyTemp();
                insp.extFieldInspect.UpdateField(new List<object>(objs));
            }
        }

        public void SetFieldsActive(bool to)
        {
            foreach (var insp in inspectFields)
            {
                insp.extFieldInspect.gameObject.SetActive(to);
                if(insp.extFieldInspect.additionalObject != null) insp.extFieldInspect.additionalObject.SetActive(to == false ? to : false);
            }
        }
    }
}