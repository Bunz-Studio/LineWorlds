using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ExternMaker
{
    public class ExtCore : MonoBehaviour
    {
        // FEATURE TODO : Adding hierarchy
        // FEATURE TODO : Updating TriggerCollection's inspector
        // SUGGESTION TODO : ADDING ARPHROS SUPPORT (might modify Arphros too)

        private static ExtCore p_instance;
        public static ExtCore instance
        {
            get
            {
                p_instance = ExtUtility.GetStaticInstance(p_instance);
                return p_instance;
            }
        }

        // Statics
        public static Dictionary<int, ExtObject> objectDictionary = new Dictionary<int, ExtObject>();
        public static int lastObjectDictionary;
        public static EditorPlayState playState
        {
            get
            {
                return p_state;
            }
            set
            {
                p_state = value;
                instance.shownPlayState = value;
                if (instance.onPlaymodeChanged != null) instance.onPlaymodeChanged.Invoke(p_state);
            }
        }
        public static EditorPlayState p_state = EditorPlayState.Stopped;

        public EditorPlayState shownPlayState = EditorPlayState.Stopped;
        public Action<EditorPlayState> onPlaymodeChanged;
        public static bool isOnlyPlaymode
        {
            get
            {
                return instance.isPlaymodeOnly;
            }
            set
            {
                instance.isPlaymodeOnly = value;
            }
        }

        // MonoBehaviour
        public bool isPlaymodeOnly;
        public Camera editorCamera;
        public BetterCamera playmodeCamera;

        public LevelManager levelManager;
        public LineMovement lineMovement;
        public ExtAudioManager audioManager;

        public Transform globalParent;

        public MeshRenderer[] replaceRequestRenderers;

        public Action<List<GameObject>> OnObjectUpdate;
        public Action OnActionUpdate;
        public Action OnClearObject;
        public Action<HierarchyUpdate> OnHierachyUpdate;
        public Action<ExtFieldInspect> OnInspectorChanged;

        public GameObject selectorInstance;

        public Material defaultMaterial;

        public GameObject[] configurations;
        public GameObject triggerInstance;
        public TriggerCollection.TrigType[] types;

        //UI
        public UIAnimationControl[] animationControls;
        public Image playButtonImage;
        public Image[] pauseButtonImages;

        List<Transform> wasSelectedObjects = new List<Transform>();

        public GameObject[] onlyAndroidUI;
        public GameObject[] onlyDesktopUI;

        //Mesh Collection
        public Mesh[] meshes;
        public static Mesh GetMesh(int index)
        {
            return instance.meshes.Length > index ? instance.meshes[index] : null;
        }

        public static int GetMeshIndex(Mesh mesh)
        {
            for(int i = 0; i < instance.meshes.Length; i++)
            {
                if (mesh == instance.meshes[i]) return i;
            }
            return -1;
        }

        void Start()
        {
            playState = EditorPlayState.Stopped;
            lineMovement.Start();
            p_instance = this;
            //playmodeCamera.gameObject.SetActive(false);
            foreach(var r in replaceRequestRenderers) ExtUtility.ReplaceWithNewMaterials(r);

#if !UNITY_EDITOR && UNITY_ANDROID
            foreach (var ui in onlyAndroidUI)
            {
                ui.SetActive(true);
            }
            foreach(var ui in onlyDesktopUI)
            {
                ui.SetActive(false);
            }
#endif
            if (isPlaymodeOnly)
            {
                PlayGame();
            }
        }

        public static void Reset()
        {
            objectDictionary.Clear();
            lastObjectDictionary = 0;
        }

        public ExtObject[] GetObjects()
        {
            return globalParent.GetComponentsInChildren<ExtObject>();
        }

        public static ExtObject GetObject(int id)
        {
        	if(id == 1) return instance.lineMovement.GetComponent<ExtObject>();
            return objectDictionary.ContainsKey(id) ? objectDictionary[id] : null;
        }

        public static int AddObject(ExtObject obj)
        {
            RemoveObject(obj);
            lastObjectDictionary++;
            objectDictionary.Add(lastObjectDictionary, obj);
            return lastObjectDictionary;
        }

        public static int AddObject(ExtObject obj, int id)
        {
            RemoveObject(obj);
            if (lastObjectDictionary < id) lastObjectDictionary = id;
            if(objectDictionary.ContainsKey(id))
            {
                lastObjectDictionary++;
                id = lastObjectDictionary;
            }
            objectDictionary.Add(id, obj);
            return id;
        }

        public static void RemoveObject(ExtObject obj)
        {
            if (objectDictionary.ContainsKey(obj.instanceID))
            {
                if (objectDictionary[obj.instanceID] == obj)
                {
                    objectDictionary.Remove(obj.instanceID);
                }
            }
        }

        public ExtObject CreateObject(GameObject config)
        {
            var obj = config.Instantiate(globalParent);
            HitRaycast(obj.transform);

            var rend = obj.GetComponent<MeshRenderer>();
            if (rend != null) ExtUtility.ReplaceWithNewMaterials(rend);

            var scr = obj.AddOrGetComponent<ExtObject>();
            scr.applyNewID = true;
            scr.Initialize();
            OnHierachyUpdate.InvokeOnExist(new HierarchyUpdate(new []{ scr }, new ExtObject[] { }));

            return scr;
        }

        public TriggerCollection CreateTrigger(TriggerCollection.TrigType type)
        {
            var obj = CreateObject(triggerInstance);
            var comp = obj.GetComponent<TriggerCollection>();
            comp.TriggerTypes = type;
            comp.SetupColor();
            return comp;
        }

        public void HitRaycast(Transform transf)
        {
            // Try raycasting
            RaycastHit hit;
            var isHitting = Physics.Raycast(editorCamera.transform.position, editorCamera.transform.forward, out hit);

            if (isHitting)
            {
                transf.position = hit.point + new Vector3(0, (transf.localScale.y / 2), 0);
            }
            else
            {
                transf.position = editorCamera.transform.position + (editorCamera.transform.forward * 20);
            }
        }

        public void CreateObject(int index)
        {
            CreateObject(configurations[index]);
        }
        public void CreateTrigger(int index)
        {
            CreateTrigger(types[index]);
        }

        public LiwProject tempProject;
        public TemporaryStates tempLineState = new TemporaryStates();
        public TemporaryStates tempCameraState = new TemporaryStates();
        bool wasPlaying;

        public void PlayGame()
        {
            if (playState == EditorPlayState.Stopped)
            {
                tempProject = ExtProjectManager.instance.GetProject();
                if (tempLineState == null) tempLineState = new TemporaryStates();
                if (tempCameraState == null) tempCameraState = new TemporaryStates();
                tempLineState.SaveState(lineMovement, lineMovement.GetType(), typeof(MonoBehaviour));
                tempCameraState.SaveState(playmodeCamera, playmodeCamera.GetType(), typeof(MonoBehaviour));

                SetPauseIcon(false);
                SetPlayIcon(true);
                SetIsPlaying(true);
                foreach (var trigger in FindObjectsOfType<LineWorldsMod.ModTrigger>())
                {
                    trigger.OnGameStart();
                }

                playmodeCamera.ForceCameraWithCurrent();
            }
            else if (playState == EditorPlayState.Paused)
            {
                LeanTween.resumeAll();
                if (wasPlaying) lineMovement.source.Play();
                SetPauseIcon(false);
                SetPlayIcon(true);
                SetIsPlaying(true);
            }
            playState = EditorPlayState.Playing;
        }

        public void PauseGame()
        {
            if (playState == EditorPlayState.Playing)
            {
                LeanTween.pauseAll();
                wasPlaying = lineMovement.source.isPlaying;
                lineMovement.source.Pause();
                SetIsPlaying(false);
                SetPauseIcon(true);
                SetPlayIcon(false);
            }
            playState = EditorPlayState.Paused;
        }

        public void StopGame()
        {
            if (playState != EditorPlayState.Stopped)
            {
                SetIsPlaying(false);
                foreach (var trigger in FindObjectsOfType<LineWorldsMod.ModTrigger>())
                {
                    trigger.OnGameStop();
                }
                LeanTween.cancelAll();
                tempCameraState.ApplyToSource(playmodeCamera);
                ExtProjectManager.instance.ApplyProjectToScene(tempProject, true);

                SetPauseIcon(false);
                SetPlayIcon(false);
                lineMovement.source.Stop();
                lineMovement.source.volume = 1;
                lineMovement.DestroyAllTail();
                lineMovement.isStarted = false;
                tempLineState.ApplyToSource(lineMovement);
            }
            playState = EditorPlayState.Stopped;
        }

        public void SetIsPlaying(bool to)
        {
            if(editorCamera != null) editorCamera.gameObject.SetActive(!to);
            //playmodeCamera.gameObject.SetActive(to || isPlaymodeOnly);
            playmodeCamera.enabled = to || isPlaymodeOnly;
            playmodeCamera.mainCamera.enabled = to || isPlaymodeOnly;
            // Debug.Log("SetIsPlaying passed camera check");

            if (!isPlaymodeOnly)
            {
                if (to)
                {
                    wasSelectedObjects = new List<Transform>(ExtSelection.instance.transformSelection);
                    ExtSelection.instance.ClearTargets();
                }
                else
                {
                    ExtSelection.instance.transformSelection = wasSelectedObjects;
                }
            }

            lineMovement.enabled = to;
            var rigid = lineMovement.GetComponent<Rigidbody>();
            rigid.useGravity = to;
            rigid.constraints = to ? RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ : RigidbodyConstraints.FreezeAll;
            rigid.isKinematic = !to;
            rigid.velocity = Vector3.zero;
            foreach(var trigger in FindObjectsOfType<TriggerCollection>())
            {
                trigger.enabled = to;
                var col = trigger.GetComponent<Collider>();
                col.enabled = false;
                trigger.GetComponent<MeshRenderer>().enabled = !to;
                col.enabled = true;
                if (to) trigger.FindObject();
            }

            foreach (var trigger in FindObjectsOfType<LineWorldsMod.ModTrigger>())
            {
                trigger.enabled = to;
                var col = trigger.GetComponent<Collider>();
                col.enabled = false;
                trigger.GetComponent<MeshRenderer>().enabled = !to;
                col.enabled = true;
                if (to) trigger.FindObject();
            }
            SetAnimationControls(!to);
        }

        public void SetPlayIcon(bool to)
        {
            if(playButtonImage != null) playButtonImage.color = to ? new Color(0, 1, 0, 1) : Color.white;
        }

        public void SetPauseIcon(bool to)
        {
            foreach(var i in pauseButtonImages)
            {
                playButtonImage.color = to ? new Color(0.6f, 0.6f, 0.6f) : Color.white;
            }
        }

        public void SetAnimationControls(bool to)
        {
            foreach(var ctrl in animationControls)
            {
                if (to)
                    ctrl.MaybeOpen();
                else
                    ctrl.MaybeClose();
            }
        }

        private void OnDestroy()
        {
            Reset();
            p_instance = null;
        }
    }
    
    [System.Serializable]
    public class ObjectConfiguration
    {
    	public string name;
        public GameObject instance;
        public float farDistance = 10;
    }

    public class TemporaryStates
    {
        public List<SourceTemp> sources = new List<SourceTemp>();

        public void SaveState(object source, Type mainType, Type excludeType)
        {
            sources.Clear();
            var exProps = new List<FieldInfo>(excludeType.GetFields());
            var propsDebug = new List<string>();
            var objsDebug = new List<string>();
            bool isSerializingAll = mainType.GetCustomAttribute(typeof(SaveAllStateAttribute)) != null;
            foreach (var p in mainType.GetFields())
            {
                bool isValidField = isSerializingAll ? 
                    p.GetCustomAttribute(typeof(IgnoreSavingStateAttribute)) == null : 
                    p.GetCustomAttribute(typeof(AllowSavingStateAttribute)) != null;
                if (isValidField)
                {
                    try
                    {
                        var cls = new SourceTemp(p, source);
                        sources.Add(cls);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning(p.Name + " -> " + e.Message);
                    }
                }
            }
        }

        public void ApplyToSource(object source)
        {
            foreach (var s in sources)
            {
                try
                {
                    s.ApplyToSource(source);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Failed to apply: " + s.name + " -> " + e.Message);
                }
            }
        }

        public class SourceTemp
        {
            public bool isField;
            public string name;
            public object value;

            public SourceTemp(PropertyInfo info, object source)
            {
                name = info.Name;
                value = info.GetValue(source);
            }
            
            public SourceTemp(FieldInfo info, object source)
            {
                isField = true;
                name = info.Name;
                value = info.GetValue(source);
            }

            public void ApplyToSource(object source)
            {
                if(isField)
                {
                    source.GetType().GetField(name).SetValue(source, value);
                }
                else
                {
                    source.GetType().GetProperty(name).SetValue(source, value);
                }
            }
        }
    }

    public enum EditorPlayState
    {
        Playing,
        Paused,
        Stopped
    }
}
