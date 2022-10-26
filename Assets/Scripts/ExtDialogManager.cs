using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace ExternMaker
{
    public class ExtDialogManager : MonoBehaviour
    {
        private static ExtDialogManager p_instance;
        public static ExtDialogManager instance
        {
            get
            {
                p_instance = ExtUtility.GetStaticInstance(p_instance);
                return p_instance;
            }
        }

        public Transform startPosition;
        public Transform endPosition;

        public float dialogDelay = 5;
        public List<ExtDialogInstance> texts = new List<ExtDialogInstance>();

        public GameObject dialogInstance;

        void Update()
        {
            foreach(var text in texts)
            {
                text.Update();
            }
            CheckInstances();
        }

        public void CheckInstances()
        {
            var dInst = new List<ExtDialogInstance>();
            for(int i = 0; i < texts.Count; i++)
            {
                var inst = texts[i];
                if (inst.isDone) dInst.Add(inst);
            }
            for (int i = 0; i < dInst.Count; i++)
            {
                var inst = dInst[i];
                texts.Remove(inst);
            }
        }

        public ExtDialogInstance SpawnDialog(string text, float delayRate = 1)
        {
            var inst = FindInstance(text);
            if (inst == null)
            {
                var instance = dialogInstance.Instantiate(transform);
                instance.transform.position = startPosition.transform.position;
                instance.transform.LeanMove(endPosition.transform.position, 1.5f).setEase(LeanTweenType.easeOutElastic);
                var cls = new ExtDialogInstance();
                cls.delayToFade = 3 * delayRate;
                cls.text = text;
                cls.Initialize(instance);
                texts.Add(cls);
                return cls;
            }
            inst.delayToFade = 3 * delayRate;
            var tween = LeanTween.value(inst.instance, Color.green, Color.white, 1).setOnUpdate((Color val) =>
            {
                inst.textUi.color = val;
            });
            tween.uncancellable = true;
            return inst;
        }

        public void AlertLocal(string text)
        {
            SpawnDialog(text, 1);
        }

        public static ExtDialogInstance Alert(string text)
        {
            return instance.SpawnDialog(text, 1);
        }

        public static ExtDialogInstance Alert(string text, float delayRate)
        {
            return instance.SpawnDialog(text, delayRate);
        }

        public ExtDialogInstance FindInstance (string text)
        {
            foreach(var ext in texts)
            {
                if (ext.text == text) return ext;
            }
            return null;
        }

        private void Start()
        {
            p_instance = this;
        }

        private void OnDestroy()
        {
            p_instance = null;
        }
    }

    [System.Serializable]
    public class ExtDialogInstance
    {
        public ExtDialogManager manager;

        public GameObject instance;
        public CanvasGroup canvasGroup;
        public Text textUi;
        public string text;
        public float delayToFade;
        public bool isDone;

        public void Initialize(GameObject obj)
        {
            instance = obj;
            canvasGroup = instance.GetComponent<CanvasGroup>();
            textUi = instance.GetComponentInChildren<Text>(true);
            instance.GetComponent<Button>().onClick.AddListener(() => {
                delayToFade = 0;
            });
            textUi.text = text;
        }

        public void Update()
        {
            if (isDone) return;

            if (delayToFade > 0)
            {
                delayToFade -= Time.deltaTime;
            }
            else
            {
                var tween = LeanTween.value(1, 0, 1).setOnUpdate((float val) =>
                {
                    if(canvasGroup != null) canvasGroup.alpha = val;
                }
                ).setOnComplete(() =>
                {
                    Object.DestroyImmediate(instance);
                }
                );
                tween.uncancellable = true;
                isDone = true;
            }
        }
    }
}