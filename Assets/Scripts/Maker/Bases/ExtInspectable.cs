using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

namespace ExternMaker
{
	public class ExtInspectable<T> : ExtInspectableUnite
    {
		public T inspectedObject;
        public List<T> inspectedObjects = new List<T>();

        public override void PushIfCorrect(object obj)
		{
			if(obj.GetType().FullName == typeof(T).FullName)
			{
				UpdateInspector((T)obj);
			}
		}
		
		public override void PushObject(GameObject obj)
		{
			var r = obj.GetComponent(typeof(T));
			if(r != null)
			{
				SetInspectorAs(true);
				UpdateInspector((T)(object)r);
			}
			else
			{
				SetInspectorAs(false);
			}
		}

        public override void PushObjects(List<GameObject> objs)
        {
            if (AreAllObjectContainComponent(typeof(T), objs))
            {
                SetInspectorAs(true);
                UpdateInspector(objs);
            }
            else
            {
                SetInspectorAs(false);
            }
        }

        public List<T> GetAllComponents(List<GameObject> objs)
        {
            var list = new List<T>();
            foreach(var obj in objs)
            {
                var comp = obj.GetComponent(typeof(T));
                if(comp != null)
                {
                    list.Add((T)(object)comp);
                }
            }
            return list;
        }

        public bool AreAllObjectContainComponent(Type t, List<GameObject> objs)
        {
            foreach (var obj in objs)
            {
                if (obj.GetComponent(t) == null) return false;
            }
            return true;
        }

        public virtual void UpdateInspector(T obj)
		{

        }

        public virtual void UpdateInspector(List<GameObject> objs)
        {

        }
    }

    public class ExtInspectableRaw<T> : ExtInspectableUnite
    {
        public T inspectedObject;

        public virtual void UpdateInspector(T obj)
        {

        }
    }
}
