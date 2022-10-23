using System;
using System.Collections.Generic;
using UnityEngine;

namespace ExternMaker
{
    [ExecuteInEditMode]
    public class ExtTests : MonoBehaviour
    {
        public ExtProjectManager projectManager;

        public string directory;
        public bool serialize;

        private void Update()
        {
            if (serialize)
            {
                projectManager.SerializeScene();
                projectManager.Save();
                serialize = false;
            }
        }
    }
}
