using System;
using System.Collections.Generic;
using UnityEngine;

namespace ExternMaker
{
    public class ExtInsMeshFilter : ExtInspectable<MeshFilter>
    {
    	public ExtInsUnityObject mesh;
        
    	void Start()
    	{
    		var tr = typeof(MeshFilter);
    		mesh.propertyInfo = new ExtProperty(tr.GetProperty("sharedMesh"));
    		mesh.Initialize();
    	}
    	
		public override void UpdateInspector(MeshFilter obj)
		{
			/*inspectedObject = obj;
			if(obj != null)
			{
				mesh.source = obj;
				mesh.ApplyTemp();
			
				base.UpdateInspector(obj);
			}*/
		}

        public override void UpdateInspector(List<GameObject> objs)
        {
            var meshes = GetAllComponents(objs);
            inspectedObjects = meshes;
            if (objs.Count > 0)
            {
                var srcs = new List<object>(meshes);
                mesh.UpdateField(srcs);

                base.UpdateInspector(objs);
            }
        }

        public override void UpdateIfExist()
		{
			if(inspectedObject != null)
			{
				mesh.ApplyTemp();
			
				base.UpdateInspector(inspectedObject);
			}
		}
		
		public override void SetInspectorAs(bool active)
		{
			base.SetInspectorAs(active);
			mesh.gameObject.SetActive(active);
		}
    }
}