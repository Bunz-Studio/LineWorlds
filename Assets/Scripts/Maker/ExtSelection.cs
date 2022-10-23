using System;
using System.Collections.Generic;
using UnityEngine;
using RuntimeGizmos;

using Object = UnityEngine.Object;

namespace ExternMaker
{
    public class ExtSelection : MonoBehaviour
    {
        private static ExtSelection p_instance;
        public static ExtSelection instance
        {
            get
            {
                p_instance = ExtUtility.GetStaticInstance(p_instance);
                return p_instance;
            }
        }

        public TransformGizmo transformGizmo;
        public List<Transform> transformSelection {
            get
            {
                return transformGizmo.targetRootsOrdered;
            }
            set
            {
                transformGizmo.ClearTargets();
                for (int i = 0; i < value.Count; i++)
                {
                    transformGizmo.AddTarget(value[i]);
                }
            }
        }

        public List<GameObject> gameObjectSelection
        {
            get
            {
                var g = transformGizmo.targetRootsOrdered;
                var l = new List<GameObject>();
                foreach(var o in g)
                {
                    l.Add(o.gameObject);
                }
                return l;
            }
            set
            {
                transformGizmo.ClearTargets();
                for (int i = 0; i < value.Count; i++)
                {
                    transformGizmo.AddTarget(value[i].transform);
                }
            }
        }

        public Transform mainTransformSelected
        {
            get
            {
                return transformGizmo.mainTargetRoot;
            }
            set
            {
                transformGizmo.ClearAndAddTarget(value);
            }
        }

        private List<Object> p_objectsSelection = new List<Object>();
        public List<Object> objectsSelection
        {
            get
            {
                var obj = p_objectsSelection;
                foreach (var a in transformSelection) obj.Add(a);
                return obj;
            }

            set
            {
                transformGizmo.ClearTargets();
                foreach (var a in value) if(a is Transform) transformGizmo.AddTarget(a as Transform);
                p_objectsSelection = value;
            }
        }

        private void Start()
        {
            p_instance = this;
        }

        private void OnDestroy()
        {
            p_instance = null;
        }

        public void ClearTargets()
        {
            transformGizmo.ClearTargets();
            p_objectsSelection.Clear();
            mainTransformSelected = null;
            gameObjectSelection.Clear();
            transformSelection.Clear();
        }
    }
}
