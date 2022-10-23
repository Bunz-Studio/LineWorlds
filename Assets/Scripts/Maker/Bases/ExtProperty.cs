using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace ExternMaker
{
    [Serializable]
    public class ExtProperty
    {
        public Type hostType;
        public Type type;
        public string name;
        public string typeFullName;

        public bool isValid;

        public PropertyInfo propertyInfo;
        public FieldInfo fieldInfo;
        public bool isField;
        public bool isStatic;

        public object lastSource;

        public ExtProperty(Type type, string name)
        {
            Initialize(type, name);
        }

        public ExtProperty(PropertyInfo property)
        {
            Initialize(property);
        }

        public ExtProperty(FieldInfo field)
        {
            Initialize(field);
        }

        internal void Initialize(Type type, string name)
        {
            hostType = type;
            var p = type.GetProperty(name);
            if(p != null)
            {
                Initialize(p);
            }
            else
            {
                var f = type.GetField(name);
                if(f != null)
                {
                    Initialize(f);
                }
                else
                {
                    isValid = false;
                }
            }
        }

        internal void Initialize(PropertyInfo info)
        {
            isValid = info != null;
            if (isValid)
            {
                propertyInfo = info;
                name = info.Name;
                type = info.PropertyType;
                typeFullName = type.FullName;
                isField = false;
                isStatic = info.IsStatic();
            }
        }

        internal void Initialize(FieldInfo info)
        {
            isValid = info != null;
            if (isValid)
            {
                fieldInfo = info;
                name = info.Name;
                type = info.FieldType;
                isField = true;
                isStatic = info.IsStatic;
            }
        }

        public object GetValue(object source = null)
        {
            if (isStatic) lastSource = null; else if (source != null) lastSource = source; 

            if (lastSource != null || isStatic)
            {
                return GetUnfilteredValue(lastSource);
            }
            return GetUnfilteredValue();
        }

        public object GetUnfilteredValue(object source = null)
        {
            try
            {
                if (isField)
                    return fieldInfo.GetValue(source);
                else
                    return propertyInfo.GetValue(source);
            }
            catch
            {
                return null;
            }
        }

        public void SetValue(object source = null, object value = null)
        {
            if (isStatic) lastSource = null; else if (source != null) lastSource = source;

            if (lastSource != null || isStatic)
            {
                SetUnfilteredValue(lastSource, value);
            }
            else
            {
                SetUnfilteredValue(source, value);
            }
        }

        public void SetUnfilteredValue(object source = null, object value = null)
        {
            try
            {
                if (isField)
                    fieldInfo.SetValue(source, value);
                else
                    propertyInfo.SetValue(source, value);
            }
            catch
            {
                UnityEngine.Debug.LogWarning("Unabled to set value for " + name + " of " + hostType.FullName);
            }
        }

        public Attribute GetCustomAttribute(Type type)
        {
            if (isField)
                return fieldInfo.GetCustomAttribute(type);
            return propertyInfo.GetCustomAttribute(type);
        }
    }

    public static class ReflectionExtensions
    {
        public static bool IsStatic(this PropertyInfo source, bool nonPublic = false)
            => source.GetAccessors(nonPublic).Any(x => x.IsStatic);
    }
}
