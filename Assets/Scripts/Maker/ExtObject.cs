using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace ExternMaker
{
	public class ExtObject : MonoBehaviour
	{
        public ObjectType objectType = ObjectType.Default;
        public LiwCustomObject customObject;
		public int instanceID;

        public bool isObstacle
        {
            get
            {
                return tag == "Obstacle";
            }
            set
            {
                tag = value ? "Obstacle" : "Untagged";
            }
        }

		public bool applyNewID;
		public bool isInitialized;

        public bool ignoreHierarchy;

        public List<int> groupID = new List<int>();

        void Start()
        {
            if(!ignoreHierarchy) ExtHierarchy.instance.AddHierarchyItem(this);
            Initialize();
		}
		
		public void Initialize()
        {
            if (isInitialized) return;
            ForceInitialize();
		}

        public void ForceInitialize()
        {
            if (applyNewID)
            {
                instanceID = ExtCore.AddObject(this);
            }
            else
            {
                Initialize(instanceID);
            }
            isInitialized = true;
        }
		
		public void Initialize(int id)
		{
			instanceID = ExtCore.AddObject(this, id);
			isInitialized = true;
        }

        public void NewGroupIDInstance()
        {
            groupID = new List<int>(groupID);
        }

        private void OnDestroy()
        {
            try
            {
                if (!ignoreHierarchy) ExtHierarchy.instance.RemoveHierarchyItem(this);
            }
            catch
            {

            }
        }

        public enum ObjectType
        {
            Default,
            BuiltIn,
            Sprite,
            Custom
        }
    }

}
