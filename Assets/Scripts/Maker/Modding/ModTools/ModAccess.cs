using UnityEngine;
using System.Collections;
using ExternMaker;

namespace LineWorldsMod
{
    public class ModAccess : MonoBehaviour
    {
        public static ModAccess self;

        public static LineMovement line
        {
            get
            {
                return ExtCore.instance.levelManager.lines[0];
            }
        }

        public static LevelManager manager
        {
            get
            {
                return ExtCore.instance.levelManager;
            }
        }

        public static BetterCamera mainCamera
        {
            get
            {
                return BetterCamera.current;
            }
        }

        void Start()
        {
            self = this;
        }

        private void OnDestroy()
        {
            self = null;
        }
    }
}
