using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace ExternMaker
{
    public class ExtActionInspector : MonoBehaviour
    {
        private static ExtActionInspector p_instance;
        public static ExtActionInspector instance
        {
            get
            {
                p_instance = p_instance == null ? FindObjectOfType<ExtActionInspector>() : p_instance;
                return p_instance;
            }
        }

        public GameObject itemInstance;
        public Transform itemParent;

        public static List<DebugClass> instances = new List<DebugClass>();
        public static bool listen = true;

        // Inspector time
        public Text title;
        public Text content;
        public Transform informationsParent;
        public GameObject informationsInstance;

        public InputField detailsText;
        public List<GameObject> informationsInstances = new List<GameObject>();

        private void Start()
        {
            p_instance = this;
            gameObject.SetActive(false);
        }

        public static void Log(object content, string title = null, params string[] informations)
        {
            if (listen && instance != null)
            {
                var cls = new DebugClass(content.ToString(), title, informations);
                var obj = Instantiate(instance.itemInstance, instance.itemParent);
                var comp = obj.GetComponent<ExtActionItem>();
                comp.contentText.text = content.ToString();
                comp.titleText.text = string.IsNullOrWhiteSpace(title) ? "Untitled" : title;
                comp.Add();
                obj.GetComponent<Button>().onClick.AddListener(() => instance.ShowDebug(cls));
                cls.item = comp;
                instances.Add(cls);
                var alert = ExtDialogManager.Alert("New message in action center", 0.5f);
                alert.instance.GetComponent<Button>().onClick.AddListener(() => {
                    instance.gameObject.SetActive(true);
                });
                Debug.LogWarning(title + ": " + content.ToString() + string.Join("\n", informations));
            }
        }

        public void ShowDebug(DebugClass cls)
        {
            title.text = cls.title;
            content.text = cls.content;

            ClearInspector();
            foreach(var info in cls.informations)
            {
                var obj = Instantiate(informationsInstance, informationsParent);
                obj.GetComponentInChildren<Text>().text = info;
                obj.GetComponent<Button>().onClick.AddListener(() => { detailsText.text = info; });
                informationsInstances.Add(obj);
            }
        }

        public void CopyDebugInfo()
        {
#if UNITY_EDITOR || !UNITY_WEBGL
            string log = GetCopyContent();
            if (string.IsNullOrEmpty(log))
                return;

#if UNITY_EDITOR || UNITY_2018_1_OR_NEWER || (!UNITY_ANDROID && !UNITY_IOS)
            GUIUtility.systemCopyBuffer = log;
#elif UNITY_ANDROID
			AJC.CallStatic( "CopyText", Context, log );
#elif UNITY_IOS
			_DebugConsole_CopyText( log );
#endif
#endif
        }

        public string GetCopyContent()
        {
            if(!string.IsNullOrWhiteSpace(content.text))
            {
                List<string> strings = new List<string>();
                foreach(var infoList in informationsParent.GetComponentsInChildren<Button>())
                {
                    strings.Add(infoList.GetComponentInChildren<Text>().text);
                }
                return title.text + ": " + content.text + " => " + string.Join("\n", strings);
            }
            return null;
        }

        public void Clear()
        {
            foreach(var inst in instances)
            {
                Destroy(inst.item.gameObject);
            }
            instances.Clear();

            ClearInspector();
            title.text = null;
            content.text = null;

            detailsText.text = null;
        }

        public void ClearInspector()
        {
            foreach (var inst in informationsInstances)
            {
                Destroy(inst);
            }
            informationsInstances.Clear();
        }

        public void ToggleListener()
        {
            listen = !listen;
        }
    }

    [System.Serializable]
    public class DebugClass
    {
        public ExtActionItem item;

        public string title;
        public string content;
        public string[] informations;

        public DebugClass()
        {
        }

        public DebugClass(string content, string title = null, string[] information = null)
        {
            this.title = title;
            this.content = content;
            this.informations = information;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            var cls = (DebugClass)obj;
            if (title != cls.title) return false;
            if (content != cls.content) return false;
            if (informations.Length != cls.informations.Length) return false;
            for(int i = 0; i < informations.Length; i++)
            {
                if (informations[i] != cls.informations[i]) return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
