using System;
using System.Collections.Generic;
using UnityEngine;

namespace ExternMaker
{
    public class ExtInspector : MonoBehaviour
    {
        private static ExtInspector p_instance;
        public static ExtInspector instance
        {
            get
            {
                p_instance = ExtUtility.GetStaticInstance(p_instance);
                return p_instance;
            }
        }

        public GameObject content;
        public ExtInsString objectNameInput;

        public ExtInspectorSettings settings;

        public List<InspectableExtObjectField> inspectFields = new List<InspectableExtObjectField>();

        public ExtInspectableUnite[] inspectables;
        public Transform publicTransform;

        public bool lockInspector;


        // Selectors
        public ExtColorSelector colorSelector;
        public ExtGameObjectSelector gameObjectSelector;
        public ExtObjectMeshSelector objectMeshSelector;
        public ExtObjectReferenceSelector objectReferenceSelector;
        public ExtMaterialSelector materialSelector;
        
        void Start()
        {
            p_instance = this;
        	var tr = typeof(GameObject);
    		objectNameInput.propertyInfo = new ExtProperty(tr.GetProperty("name"));
			objectNameInput.Initialize();
			
        	ExtCore.instance.OnObjectUpdate += UpdateInspectors;
        	ExtCore.instance.OnActionUpdate += UpdateAction;
        	ExtCore.instance.OnClearObject += ObjectClear;

            foreach (var insp in inspectFields)
            {
                insp.Initialize();
            }

            ObjectClear();
        }
        
        private void OnDestroy()
        {
            p_instance = null;
        }

        public void UpdateInspectors(List<GameObject> objs)
        {
            if (lockInspector) return;
            
            var obj = objs.Count > 0 ? objs[0] : null;
            content.SetActive(obj != null);

            if (obj == null) return;
            objectNameInput.UpdateField(new List<object>(objs));

        	foreach(var i in inspectables)
        	{
        		i.PushObject(obj);
                i.PushObjects(objs);
        	}

            foreach (var insp in inspectFields)
            {
                // insp.extFieldInspect.source = obj.GetComponent<ExtObject>();
                insp.extFieldInspect.UpdateField(new List<object>(GetAllComponentsInList<ExtObject>(objs)));
                // insp.extFieldInspect.ApplyTemp();
            }
        }

        public List<T> GetAllComponentsInList<T>(List<GameObject> objs)
        {
            var list = new List<T>();
            foreach(var obj in objs)
            {
                var comp = obj.GetComponent(typeof(T));
                if (comp != null) list.Add((T)(object)comp);
            }
            return list;
        }
        
        public void UpdateAction()
        {
            if (lockInspector) return;

            if (ExtSelection.instance.transformSelection.Count > 0)
            {
                objectNameInput.UpdateField(new List<object>(ExtSelection.instance.gameObjectSelection));
                foreach (var insp in inspectFields)
                {
                    insp.extFieldInspect.ApplyTemp();
                }
            }
        	foreach(var i in inspectables)
        	{
        		i.UpdateIfExist();
            }
        }
        
        public void ObjectClear()
        {
            if (lockInspector) return;

            if (content != null) content.SetActive(false);
        }

        [Serializable]
        public class ExtInspectableMatch
        {
            public string componentTypeName;
            public string inspectorTypeName;

            public Type componentTypes
            {
                get
                {
                	return Type.GetType(componentTypeName);
                }
            }

            public Type inspectorType
            {
                get
                {
                    return Type.GetType(inspectorTypeName);
                }
            }
        }

        [Serializable]
        public class ExtFieldMatch
        {
            public string fieldTypeName;
            public string inspectorTypeName;
            public GameObject inspectorGameObject;

            public Type fieldType
            {
                get
                {
                    return Type.GetType(fieldTypeName);
                }
            }

            public Type inspectorType
            {
                get
                {
                    return Type.GetType(inspectorTypeName);
                }
            }
        }

        [Serializable]
        public class InspectableExtObjectField : ExtInspectableUnite.InspectField<ExtObject>
        {

        }
    }
}
