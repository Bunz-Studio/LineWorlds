using System;
using System.Data;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ExternMaker
{
    public class ExtFieldInspect : MonoBehaviour
    {
        public virtual Type type
        {
            get
            {
                return null;
            }
        }

        public string fieldName;
        public bool editAffectHierarchy;

        public List<object> starters = new List<object>();
        public object starter
        {
            get
            {
                return starters.Count > 0 ? starters[0] : null;
            }
        }

        public List<object> values = new List<object>();
        public object value
        {
            get
            {
                return values.Count > 0 ? values[0] : null;
            }
        }

        private ExtObject obj;
        public ExtObject sourceObject
        {
            get
            {
                return obj == null ? TryGet() : obj;
            }
            set
            {
                obj = value;
            }
        }

        public List<object> sources = new List<object>();
        public object source
        {
            get
            {
                return sources.Count > 0 ? sources[0] : null;
            }
        }

        public Text fieldText;

        public ExtProperty propertyInfo;
        public bool isStatic {
            get
            {
                return propertyInfo == null ? false : propertyInfo.isStatic;
            }
            set
            {
                if (propertyInfo != null) propertyInfo.isStatic = true;
            }
        }

        public Action<object> fieldUpdate;
        public Action<object[]> multiFieldUpdate;
        public Action<object> fieldFinishEdit;
        public Action<object[]> multiFieldFinishEdit;

        public GameObject additionalObject;

        public bool isUpdatingField;
        public bool sourceless;

        public virtual void Initialize()
        {
            SetName();
        }
        
        public virtual void SetName()
        {
            fieldName = propertyInfo.name;
        }

        public virtual void Close()
        {

        }

        public virtual void SetAdditionalObject(GameObject obj)
        {
            additionalObject = obj;
        }

        public virtual bool AreValuesSimilar()
        {
            if (isStatic || sourceless) return true;
            
            if (values.Count < 1) return false;
            for (int i = 1; i < values.Count; i++)
            {
                if (!object.Equals(values[0], values[1])) return false;
            }
            return true;
        }

        public virtual void UpdateField(List<object> sources)
        {
            this.sources = sources;
            values = GetValues();
            ApplyTemp();
        }

        public virtual List<object> GetValues()
        {
            if (sourceless) return values;
            var l = new List<object>();
            if (isStatic) l.Add(propertyInfo.GetValue(null));
            foreach (var s in sources)
            {
                l.Add(propertyInfo.GetValue(s));
            }
            return l;
        }

        public virtual T GetTemp<T>()
        {
            return value == null ? default(T) : (T)value;
        }

        public virtual object GetValue(object src = null)
        {
            if (sourceless) return values.Count > 0 ? values[0] : null;
            values = GetValues();
            src = src ?? source;
            return value;
        }

        public virtual void SetValue(object value)
        {
            if (sourceless) return;
            if (isStatic) propertyInfo.SetValue(null, value);
            for (int i = 0; i < sources.Count; i++)
            {
                propertyInfo.SetValue(sources[i], value);
            }
        }

        public virtual void SetValues(List<object> values)
        {
            if (sourceless) return;
            if (isStatic) propertyInfo.SetValue(null, values[0]);
            for (int i = 0; i < sources.Count; i++)
            {
                propertyInfo.SetValue(sources[i], values[i]);
            }
        }

        public virtual Type GetInspectedType()
        {
            return propertyInfo.type;
        }
        
        public virtual void ApplyTemp()
        {

        }

        public virtual void StartEdit()
        {
            starters = GetValues();
        }

        public virtual void FinalizeEdit()
        {
            var action = new ExtActionInstance();
            var values = GetValues();
            action.objects = new object[] { starters, values, sources };
            action.action += () =>
            {
                var srcs = (List<object>)action.objects[2];
                var vls = (List<object>)action.objects[1];
                for (int i = 0; i < srcs.Count; i++)
                {
                    propertyInfo.SetValue(srcs[i], vls[i]);
                }
            };
            action.undo += () =>
            {
                var srcs = (List<object>)action.objects[2];
                var vls = (List<object>)action.objects[0];
                for (int i = 0; i < srcs.Count; i++)
                {
                    propertyInfo.SetValue(srcs[i], vls[i]);
                }
                ApplyTemp();
            };
            action.AddToManager();
        }

        public virtual void CallUpdate(object value)
        {
            fieldUpdate.InvokeOnExist(value);
        }

        public virtual void CallFinishEdit(object value)
        {
            fieldFinishEdit.InvokeOnExist(value);
        }

        public struct FieldEdit
        {
            public bool succeed;
            public object value;

            public FieldEdit(bool s, object v)
            {
                succeed = s;
                value = v;
            }

            public T GetValue<T>()
            {
                return (T)value;
            }
        }

        public static float TryParse(InputField field, float def, bool reset = false)
        {
            try
            {
                double result = Convert.ToDouble(new DataTable().Compute(field.text, null));
                var v = (float)result;
                if (reset) field.text = v.ToString();
                return v;
            }
            catch
            {
                if (reset) field.text = def.ToString();
                return def;
            }
        }

        public ExtObject TryGet()
        {
            if (sourceless) return null;
            if (source.GetType().FullName == "UnityEngine.GameObject")
            {
                return ((GameObject)source).GetComponent<ExtObject>();
            }
            else if (source.GetType().FullName == "UnityEngine.Transform")
            {
                return ((Transform)source).GetComponent<ExtObject>();
            }
            else if (source.GetType().IsSubclassOf(typeof(MonoBehaviour)))
            {
                return ((MonoBehaviour)source).GetComponent<ExtObject>();
            }
            Debug.Log("Not found source for " + source.GetType().FullName);
            return null;
        }
    }
}
