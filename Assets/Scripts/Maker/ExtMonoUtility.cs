using UnityEngine;
using System.Collections.Generic;

namespace ExternMaker
{
    public class ExtMonoUtility : MonoBehaviour
    {
        public static ExtMonoUtility myself;
        public List<Util_RuntimeLoop> runtimeLoops = new List<Util_RuntimeLoop>();

        private void Start()
        {
            if (myself == null)
            {
                myself = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                if (myself != this)
                {
                    Destroy(gameObject);
                }
            }
        }

        private void Update()
        {
            var destroyables = new List<Util_RuntimeLoop>();
            foreach (var l in runtimeLoops)
            {
                var r = l.Update();
                if (!r) destroyables.Add(l);
            }
            foreach (var d in destroyables)
            {
                runtimeLoops.Remove(d);
            }
            destroyables.Clear();
        }

        public static Texture2D ConvertByteToTexture(byte[] data)
        {
            Texture2D texture = null;
            texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            texture.LoadImage(data);
            return texture;
        }

        public static Util_RuntimeLoop DoRuntimeLoop(int i, int max, int rate = 1)
        {
            return myself.TryRuntimeLoop(i, max, rate);
        }
        public Util_RuntimeLoop TryRuntimeLoop(int i, int max, int rate = 1)
        {
            var cls = new Util_RuntimeLoop();
            cls.i = i;
            cls.max = max;
            cls.rate = rate;
            runtimeLoops.Add(cls);
            return cls;
        }
    }

    [System.Serializable]
    public class Util_RuntimeLoop
    {
        public int i;
        public int max;
        public int rate;

        public System.Action<int> onUpdate;
        public System.Action onComplete;
        bool isComplete;

        public bool Update()
        {
            if(i < max)
            {
                if (onUpdate != null) onUpdate.Invoke(i);
                i += rate;
                return true;
            }

            if (!isComplete) if (onComplete != null) onComplete.Invoke();
            isComplete = true;
            return false;
        }
    }
}
