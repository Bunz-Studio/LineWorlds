using System.Collections.Generic;
using UnityEngine;
using ExternMaker.Serialization;

namespace ExternMaker
{
    [ExecuteInEditMode]
    public class ExtConverter : MonoBehaviour
    {
        public static bool isCustomMesh
        {
            get
            {
                return myself.customMesh;
            }
        }
        public static ExtConverter myself;

        public string directory;
        public Transform parent;
        public LiwProject project = new LiwProject();
        public BetterCamera cam;
        public bool serialize;
        public bool customMesh;
        public bool includeInactive = true;

        void Update()
        {
            myself = this;
            if (!Application.isPlaying) EditorUpdate();
            else RuntimeUpdate();
        }

        public void EditorUpdate()
        {
            if(serialize)
            {
                Serialize();
                serialize = false;
            }
        }

        public void RuntimeUpdate()
        {

        }

        public void Serialize()
        {
            var proj = new LiwProject();
            RegisterAllObject();
            var objects = parent.GetComponentsInChildren<ExtObject>(includeInactive);

            proj.renderSettings = new Serializables.SerializedRenderSettings();
            proj.renderSettings.backgroundColor = cam.mainCamera.backgroundColor;
            proj.renderSettings.fog = RenderSettings.fog;
            proj.renderSettings.fogDensity = RenderSettings.fogDensity;
            proj.renderSettings.fogColor = RenderSettings.fogColor;

            var mov = FindObjectOfType<LineMovement>().gameObject;
            proj.lineInfo = LiwSerializer.SerializeGameObject(mov.GetComponent<ExtObject>());
            proj.cameraInfo = LiwSerializer.SerializeGameObject(cam.GetComponent<ExtObject>());

            proj.gameObjects.Clear();
            foreach (var obj in objects)
            {
                proj.gameObjects.Add(LiwSerializer.SerializeGameObject(obj));
            }
            UnregisterAllObject();
            LiwSerializer.CreateProject(directory, proj);
        }

        public int last = 2;
        public void RegisterAllObject()
        {
            var transforms = parent.GetComponentsInChildren<Transform>(includeInactive);
            foreach(var tr in transforms)
            {
                last++;
                var ext = tr.gameObject.AddOrGetComponent<ExtObject>();
                ext.instanceID = last;
            }
        }

        public void UnregisterAllObject()
        {
            last = 2;
            var extObjects = parent.GetComponentsInChildren<ExtObject>(includeInactive);
            foreach (var ext in extObjects)
            {
                DestroyImmediate(ext);
            }
        }
    }
}