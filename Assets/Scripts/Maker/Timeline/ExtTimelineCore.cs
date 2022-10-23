using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ExternMaker.Easings;

namespace ExternMaker.Timeline
{
    public class ExtTimelineCore : MonoBehaviour
    {
        public float timelineLimit = 7200;

        public RectTransform timelineContentRectTransform;
        public Scrollbar timelineSlider;

        public GameObject lanePrefab;
        public GameObject keyframePrefab;

        public List<TimelineInstance> lanes = new List<TimelineInstance>();

        public float timePerCoordinate = 5;
        public float timeScale = 5;

        public bool isRecording;

        float previousTime;
        public float currentTime;

        private void Start()
        {
            ExtCore.instance.OnInspectorChanged += OnInspectorChanged;
        }

        public void OnInspectorChanged(ExtFieldInspect inspector)
        {
            if (!isRecording) return;

            var instance = GetInstance(inspector);

            if (instance == null)
            {
                instance = new TimelineInstance();
                instance.inspectedObject = inspector.sourceObject;
                instance.property = inspector.propertyInfo;
                instance.sourceName = inspector.source.GetType().FullName;

                instance.mainInstance = Instantiate(lanePrefab, timelineContentRectTransform);

                instance.timelineObject = instance.mainInstance.GetComponentInChildren<ExtTimelineObject>();
                instance.timelineObject.core = this;
                instance.timelineObject.property = inspector.propertyInfo;
                instance.timelineObject.source = inspector.source;

                instance.timelineLane = instance.mainInstance.GetComponentInChildren<ExtTimelineLane>();
                instance.timelineLane.core = this;
                instance.timelineLane.currentTime = currentTime;
                instance.timelineLane.RegisterKeyframe(inspector.GetValue());

                lanes.Add(instance);
            }
            else
            {
                instance.timelineLane.currentTime = currentTime;
                instance.timelineLane.RegisterKeyframe(inspector.GetValue());
            }
        }

        public TimelineInstance GetInstance(ExtFieldInspect inspector)
        {
            foreach(var timeline in lanes)
            {
                bool req = timeline.inspectedObject == inspector.sourceObject && timeline.sourceName == inspector.source.GetType().FullName && inspector.propertyInfo.name == timeline.property.name;
                return timeline;
            }
            return null;
        }

        void Update()
        {
            UpdateValue();
        }

        public void UpdateValue()
        {
            if(previousTime != currentTime)
            {
                previousTime = currentTime;
                foreach(var lane in lanes)
                {
                    lane.timelineLane.UpdateTime(currentTime);
                }
            }
        }

        [Serializable]
        public class TimelineInstance
        {
            public ExtObject inspectedObject;

            public string sourceName;
            public ExtProperty property;

            public GameObject mainInstance;
            public ExtTimelineObject timelineObject;
            public ExtTimelineLane timelineLane;
        }
    }

    public static class TimeLineUtility
    {
        public static RectTransform GetRectTransform(this GameObject gameObject)
        {
            return gameObject.GetComponent<RectTransform>();
        }

        static TweenMatches[] tweenableTypes = new []{
            new TweenMatches(typeof(Vector3), typeof(Vector3Easeable)),
            new TweenMatches(typeof(Vector2), typeof(Vector2Easeable)),
            new TweenMatches(typeof(float), typeof(FloatEaseable)),
            new TweenMatches(typeof(int), typeof(IntEaseable)),
            new TweenMatches(typeof(Color), typeof(ColorEaseable))
        };

        public static bool IsTweenable(this Type type)
        {
            foreach(var t in tweenableTypes)
            {
                if (t.type.FullName == type.FullName) return true;
            }
            return false;
        }

        public static TweenMatches GetTweenMatch(this Type type)
        {
            foreach (var t in tweenableTypes)
            {
                if (t.type.FullName == type.FullName) return t;
            }
            return null;
        }

        public static T GetTweenedValue<T>(T start, T end, float duration, float time, EaseType type)
        {
            var match = GetTweenMatch(typeof(T));
            if(match != null)
            {
                match.easeable.SetEase(type);
                return (T)match.easeable.GetValueAtTime(start, end, time, duration);
            }
            else
            {
                return time >= duration ? end : start;
            }
        }

        public static object GetTweenedValue(Type type, object start, object end, float duration, float time, EaseType easeType)
        {
            var match = GetTweenMatch(type);
            if (match != null)
            {
                match.easeable.SetEase(easeType);
                return match.easeable.GetValueAtTime(start, end, time, duration);
            }
            else
            {
                return time >= duration ? end : start;
            }
        }

        [Serializable]
        public class TweenMatches
        {
            public Type type;
            public Type easer;

            public Easeable easeable;

            public TweenMatches(Type t, Type e)
            {
                type = t;
                easer = e;
                easeable = (Easeable)Activator.CreateInstance(easer);
            }
        }
    }
}
