using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ExternMaker
{
	public class ExtObjectSelector : MonoBehaviour
	{
		public Transform parent;
		public ExtInsUnityObject host;
		public List<Button> objectButtons = new List<Button>();

        public System.Type inspectedType;

		public Object previousObject;
		public Object selectedObject;
		
		public virtual void Initialize(Object init, ExtInsUnityObject host)
		{
			previousObject = init;
			selectedObject = init;
			this.host = host;
		}
		
		public virtual void ChooseObject(Object obj)
		{
			selectedObject = obj;
			host.SelectObject((Object)(object)selectedObject);
		}
		
		public virtual void Show()
		{
			gameObject.SetActive(true);
		}
		
		public virtual void AddToList(Object obj)
		{
			var i = Instantiate(ExtCore.instance.selectorInstance, parent);
			var b = i.GetComponent<Button>();
			b.onClick.AddListener(() => ChooseObject(obj));
            var o = (Object)(object)obj;
            i.GetComponentInChildren<Text>().text = o.name;
			objectButtons.Add(b);
		}
		
		public void ClearList()
		{
			foreach(var a in objectButtons)
			{
				Destroy(a.gameObject);
			}
            objectButtons.Clear();
		}
		
		public virtual void FinalClose()
		{
            var obj = selectedObject;
            if (previousObject != null)
            {
                if (!previousObject.Equals(selectedObject)) host.EndObject(obj);
            }
            else
            {
                host.EndObject(obj);
            }
			gameObject.SetActive(false);
		}
	}
}