using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RuntimeGizmos;

namespace ExternMaker
{
    public class ExtShortcuts : MonoBehaviour
    {
        public KeyCode actionKey = KeyCode.LeftShift;

        public KeyCode copyKey = KeyCode.C;
        public KeyCode pasteKey = KeyCode.V;
        public KeyCode duplicateKey = KeyCode.D;
        public KeyCode deleteKey = KeyCode.Delete;
        
        public List<GameObject> selection {
        	get
        	{
        		return FilterBlacklist(ExtSelection.instance.gameObjectSelection);
        	}
        }

        public List<GameObject> blackListedObjects = new List<GameObject>();
        public List<GameObject> clipboards = new List<GameObject>();
        public List<Shortcut> shortcuts = new List<Shortcut>();

        private void Update()
        {
            foreach (var s in shortcuts)
            {
                s.Update();
            }

            if (selection == null || selection.Count <= 0) return;

            if (ExtUtility.IsValidShortcutKey())
            {
                if (Input.GetKeyDown(deleteKey))
                {
                	DeleteObjects();
                }

                if (!Input.GetKey(actionKey)) return;

                if (selection != null)
                {
                    if (Input.GetKeyDown(copyKey))
                    {
                    	CopyObject();
                    }
                    if (Input.GetKeyDown(duplicateKey))
                    {
                    	DuplicateObject();
                    }
                }
                if (Input.GetKeyDown(pasteKey))
                {
                	PasteObject();
                }
            }
        }
        
        public void CopyObject()
        {
        	clipboards.Clear();
            foreach (var obj in selection)
            {
                clipboards.Add(obj);
            }
            ExtDialogManager.Alert("Objects copied");
        }
        
        public void PasteObject()
        {
            CloneObjects(clipboards);
            ExtDialogManager.Alert("Objects pasted");
        }
        
        public void DuplicateObject()
        {
            CloneObjects(selection, true);
            ExtDialogManager.Alert("Objects duplicated");
        }
        
        public List<GameObject> FilterBlacklist(List<GameObject> collection)
        {
            var list = new List<GameObject>();
            foreach(var obj in collection)
            {
                if (!blackListedObjects.Contains(obj)) list.Add(obj);
            }
            return list;
        }

        public GameObject CloneObject(GameObject obj, bool stayOnPlace = false)
        {
            var instance = CloneObject(obj, Vector3.zero);

            if (stayOnPlace)
                instance.transform.position = obj.transform.position;
            else
                ExtCore.instance.HitRaycast(instance.transform);

            return instance;
        }

        public List<GameObject> CloneObjects(List<GameObject> objs, bool stayOnPlace = false)
        {
            ExtSelection.instance.ClearTargets();
            var list = new List<GameObject>();
            var exts = new List<ExtObject>();
            var tempObj = new GameObject();
            var firstObj = objs[0];
            if (stayOnPlace)
                tempObj.transform.position = firstObj.transform.position;
            else
                ExtCore.instance.HitRaycast(tempObj.transform);
            var offset = tempObj.transform.position - firstObj.transform.position;
            DestroyImmediate(tempObj);

            foreach (var obj in objs)
            {
                CloneObject(obj, obj.transform.position + offset);
                list.Add(obj);
                exts.Add(obj.GetComponent<ExtObject>());
            }

            ExtSelection.instance.gameObjectSelection = list;
            ExtCore.instance.OnHierachyUpdate.InvokeOnExist(new HierarchyUpdate(exts.ToArray(), new ExtObject[] { }));
            return list;
        }

        public GameObject CloneObject(GameObject obj, Vector3 position)
        {
            ExtSelection.instance.ClearTargets();
            var instance = obj.Instantiate(ExtCore.instance.globalParent);

            var src = instance.GetComponent<ExtObject>();
            src.applyNewID = true;
            src.isInitialized = false;
            src.Initialize();

            instance.transform.position = position;
            var rend = instance.GetComponent<MeshRenderer>();
            if(rend != null) ExtUtility.ReplaceWithNewMaterials(rend);

            return instance;
        }

        public void SetGizmosToolAs(int i)
        {
            TransformType type = i == 0 ? TransformType.Move : i == 1 ? TransformType.Rotate : i == 2 ? TransformType.Scale : TransformType.All;
            ExtSelection.instance.transformGizmo.transformType = type;
        }
        
        public void DeleteObjects()
        {
            var exts = new List<ExtObject>();
            ExtCore.instance.OnHierachyUpdate.InvokeOnExist(new HierarchyUpdate(new ExtObject[] { }, exts.ToArray()));
            foreach (var obj in selection)
            {
                exts.Add(obj.GetComponent<ExtObject>());
                Destroy(obj);
            }
            ExtSelection.instance.ClearTargets();
            ExtDialogManager.Alert("Objects deleted");
        }

        [System.Serializable]
        public class Shortcut
        {
            public KeyCode actionKey = KeyCode.LeftControl;
            public bool requireActionKey;
            public KeyCode keyCode;
            public bool allowRepeat;

            public UnityEngine.Events.UnityEvent action;

            public void Update()
            {
                if (requireActionKey && !Input.GetKey(actionKey)) return;

                if ((allowRepeat && !Input.GetKey(keyCode)) || !Input.GetKeyDown(keyCode)) return;
                action.Invoke();
            }
        }
    }
}
