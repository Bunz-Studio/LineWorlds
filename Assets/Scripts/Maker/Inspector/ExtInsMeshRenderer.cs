using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ExternMaker
{
    public class ExtInsMeshRenderer : ExtInspectable<MeshRenderer>
    {
    	public PropertyInfo sharedMaterialProperty;
    	public PropertyInfo materialColor;
    	// public ExtInsColor mesh;
        public ExtInsMaterial material;
        
    	void Start()
    	{
    		var tr = typeof(MeshRenderer);
    		sharedMaterialProperty = tr.GetProperty("sharedMaterial");
            material.propertyInfo = new ExtProperty(tr, "sharedMaterial");
    	}

        public override void UpdateInspector(List<GameObject> objs)
        {
            base.UpdateInspector(objs);
            var meshes = GetAllComponents(objs);
            inspectedObjects = meshes;
            if (objs.Count > 0)
            {
                var srcs = new List<object>(meshes);
                material.UpdateField(srcs);
            }
        }

        public override void UpdateIfExist()
		{
			if(inspectedObject != null)
			{
				material.ApplyTemp();
			
				base.UpdateInspector(inspectedObject);
			}
		}
		
		public override void SetInspectorAs(bool active)
		{
			base.SetInspectorAs(active);
            material.gameObject.SetActive(active);
		}
    }
}