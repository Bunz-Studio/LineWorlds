using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ExternMaker
{
    public class ExtInsMeshRenderer : ExtInspectable<MeshRenderer>
    {
        public ExtInsMaterial material;
        public ExtInsArrayMaterial materials;
        
    	void Start()
    	{
            if (material != null)
            {
                material.Initialize();
                material.propertyInfo = new ExtProperty(typeof(MeshRenderer), "sharedMaterial");
            }
            if (materials != null)
            {
                materials.Initialize();
                materials.propertyInfo = new ExtProperty(typeof(MeshRenderer), "sharedMaterials");
            }
    	}

        public override void UpdateInspector(List<GameObject> objs)
        {
            base.UpdateInspector(objs);
            var meshes = GetAllComponents(objs);
            inspectedObjects = meshes;
            if (objs.Count > 0)
            {
                var srcs = new List<object>(meshes);
                if (materials != null) materials.UpdateField(srcs);
                if (material != null) material.UpdateField(srcs);
            }
        }

        public override void UpdateIfExist()
		{
			if(inspectedObject != null)
			{
				if (materials != null) materials.ApplyTemp();
                if (material != null) material.ApplyTemp();

                base.UpdateInspector(inspectedObject);
			}
		}
		
		public override void SetInspectorAs(bool active)
		{
			base.SetInspectorAs(active);
            if (material != null) material.gameObject.SetActive(active);
            if (materials != null)
            {
                materials.gameObject.SetActive(active);
                materials.additionalObject.SetActive(active == false ? false : materials.additionalObject.activeSelf);
            }
		}
    }
}