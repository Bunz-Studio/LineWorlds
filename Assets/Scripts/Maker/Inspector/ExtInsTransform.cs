using System;
using System.Collections.Generic;
using UnityEngine;

namespace ExternMaker
{
    public class ExtInsTransform : ExtInspectable<Transform>
    {
    	public ExtInsVector3 position;
    	public ExtInsVector3 rotation;
    	public ExtInsVector3 scale;
        
    	void Start()
    	{
    		var tr = typeof(Transform);
    		position.propertyInfo = new ExtProperty(tr.GetProperty("localPosition"));
			position.Initialize();
    		rotation.propertyInfo = new ExtProperty(tr.GetProperty("localEulerAngles"));
			rotation.Initialize();
    		scale.propertyInfo = new ExtProperty(tr.GetProperty("localScale"));
    		scale.Initialize();
    	}
    	
		public override void UpdateInspector(Transform obj)
		{
			/*inspectedObject = obj;
			if(obj != null)
			{
				position.source = obj;
				rotation.source = obj;
				scale.source = obj;
			
				position.ApplyTemp();
				rotation.ApplyTemp();
				scale.ApplyTemp();
			
				base.UpdateInspector(obj);
			}*/
		}

        public override void UpdateInspector(List<GameObject> objs)
        {
            Debug.Log("Updating Transform Inspector");
            base.UpdateInspector(objs);
            var trans = GetAllComponents(objs);
            inspectedObjects = trans;
            if (objs.Count > 0)
            {
                var srcs = new List<object>(trans);
                position.UpdateField(srcs);
                rotation.UpdateField(srcs);
                scale.UpdateField(srcs);
            }
        }

        public override void UpdateIfExist()
        {
            Debug.Log("Updating Transform If Exist");
            if (inspectedObjects.Count > 0)
		    {
                position.ApplyTemp();
                rotation.ApplyTemp();
                scale.ApplyTemp();
			
				base.UpdateInspector(inspectedObject);
			}
		}
		
		public override void SetInspectorAs(bool active)
		{
			base.SetInspectorAs(active);
			position.gameObject.SetActive(active);
			rotation.gameObject.SetActive(active);
			scale.gameObject.SetActive(active);
		}
    }
}