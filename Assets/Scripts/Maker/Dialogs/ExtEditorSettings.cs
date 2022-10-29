using UnityEngine;
using System.Collections.Generic;

namespace ExternMaker
{
    public class ExtEditorSettings : MonoBehaviour
    {
        public static ExtEditorSettings self;
        public EditorSettings settings = new EditorSettings();
        public Canvas canvas;
        public List<EditorSettingsField> fields = new List<EditorSettingsField>();
        const string ePath = "editorPreferences.json";
        bool avoidSave;
        bool isInitialized;
        float prevSpeed;

        public float UIScaleFactor
        {
            get
            {
                return settings.scaleFactor;
            }
            set
            {
                settings.scaleFactor = Mathf.Clamp(value, 0.2f, 3);
                canvas.scaleFactor = settings.scaleFactor;
                Save();
            }
        }

        public bool EnableFogInEditing
        {
            get
            {
                return settings.enableFogInEditing;
            }
            set
            {
                settings.enableFogInEditing = value;
                ExtProjectManager.instance.settings.enabledFogInEditing = value;
                Save();
            }
        }

        public float GridScale
        {
            get
            {
                return settings.Scale;
            }
            set
            {
                settings.Scale = Mathf.Clamp(value, 0.01f, 99999);
                ExtGrid.instance.scale = settings.Scale;
                Save();
            }
        }

        public float GridSnapRate
        {
            get
            {
                return settings.snapDetailRate;
            }
            set
            {
                settings.snapDetailRate = Mathf.Clamp(value, 0.01f, 99999);
                UpdateSnappings();
                Save();
            }
        }

        public float GridBPM
        {
            get
            {
                return settings.BPM;
            }
            set
            {
                settings.BPM = Mathf.Clamp(value, 0.1f, 99999);
                ExtGrid.instance.musicBPM = settings.BPM;
                UpdateSnappings();
                Save();
            }
        }

        public int LoadObjectsPerFrame
        {
            get
            {
                return settings.loadObjectsPerFrame;
            }
            set
            {
                settings.loadObjectsPerFrame = Mathf.Clamp(value, 1, 4096);
                ExtProjectManager.instance.loadObjectsPerFrame = settings.loadObjectsPerFrame;
                Save();
            }
        }

        private void OnEnable()
        {
            Initialize();
            UpdateInspectors();
        }

        private void Update()
        {
            if(prevSpeed != ExtGrid.instance.lineSpeed)
            {
                UpdateSnappings();
                prevSpeed = ExtGrid.instance.lineSpeed;
            }
        }

        public void Initialize()
        {
            self = this;
            if (isInitialized) return;
            // Project Info Fields
            foreach (var insp in fields)
            {
                insp.extFieldInspect.sources = new List<object>(new object[] { this });
                insp.Initialize();
            }
            isInitialized = true;
        }

        public void UpdateInspectors()
        {
            foreach (var insp in fields)
            {
                insp.extFieldInspect.sources = new List<object>(new object[] { this });
                insp.extFieldInspect.ApplyTemp();
            }
        }

        public void Load()
        {
            if (Storage.FileExistsLocal(ePath))
            {
                var data = Storage.ReadAllTextLocal(ePath);
                settings = JsonUtility.FromJson<EditorSettings>(data);
            }
        }

        public void Apply()
        {
            Initialize();
            Load();
            avoidSave = true;
            UIScaleFactor = settings.scaleFactor;
            GridBPM = settings.BPM;
            GridScale = settings.Scale;
            avoidSave = false;
            LoadObjectsPerFrame = settings.loadObjectsPerFrame;
            EnableFogInEditing = settings.enableFogInEditing;
            UpdateInspectors();
        }

        public void Save()
        {
            if (avoidSave) return;
            var data = JsonUtility.ToJson(settings, true);
            Storage.WriteAllTextLocal(ePath, data);
        }

        public void UpdateSnappings()
        {
            ExtSelection.instance.transformGizmo.movementSnap = (settings.BPM / 120 / settings.snapDetailRate) * ExtGrid.instance.lineSpeed;
            ExtSelection.instance.transformGizmo.scaleSnap = (settings.BPM / 120 / settings.snapDetailRate) * ExtGrid.instance.lineSpeed;
        }

        [System.Serializable]
        public class EditorSettingsField : ExtInspectableUnite.InspectField<ExtEditorSettings> {}
    }

    [System.Serializable]
    public class EditorSettings
    {
        // UI
        public float scaleFactor = 1;

        // Gridlines
        public float snapDetailRate = 2;
        public float BPM = 120;
        public float Scale = 10;

        // SavePath
        public string savePath;

        // Editor
        public bool enableFogInEditing = true;
        public int loadObjectsPerFrame = 25;
    }
}
