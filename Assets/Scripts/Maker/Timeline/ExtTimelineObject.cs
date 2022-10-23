using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

namespace ExternMaker.Timeline
{
    public class ExtTimelineObject : MonoBehaviour
    {
        public ExtTimelineCore core;
        public ExtTimelineLane lane;

        public ExtObject controlledObject;
        
        public object source;
        public ExtProperty property;
        public Action<object> fieldUpdate;

        public void SetValue(object obj)
        {
            property.SetValue(source, obj);
        }
    }
}
