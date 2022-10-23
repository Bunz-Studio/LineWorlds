using UnityEngine;

namespace ExternMaker
{
	public class ExtObjectMeshSelector : ExtObjectSelector
	{
		public Mesh[] defaults;
		public bool isInitialized;
		
		public override void Initialize(Object init, ExtInsUnityObject host)
		{
			base.Initialize(init, host);
            ClearList();
			foreach(var d in defaults)
			{
				AddToList(d);
			}
			isInitialized = true;
            foreach (var c in ExtResourcesManager.instance.customObjects)
            {
                if (c.mesh != null) AddToList(c.mesh);
            }
            gameObject.SetActive(true);
		}
	}
}
