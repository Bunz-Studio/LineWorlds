using UnityEngine;
using System.Collections.Generic;

namespace ExternMaker
{
    [CreateAssetMenu(fileName = "InspectorSettings", menuName = "LineWorlds/Inspector Settings", order = 0)]
    public class ExtInspectorSettings : ScriptableObject
    {
        public string settingsName;
        public List<InspectMatch> matches = new List<InspectMatch>();

        [System.Serializable]
        public class InspectMatch
        {
            public ExtFieldInspect extFieldInspect;
            public GameObject additionalInspect;
        }

        public InspectMatch GetMatch(System.Type type)
        {
            return matches.Find(val => val.extFieldInspect.type.FullName == type.FullName);
        }
    }
}
