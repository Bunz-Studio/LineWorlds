using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

namespace ExternMaker.Timeline
{
    public class ExtTimelineLane : MonoBehaviour
    {
        public ExtTimelineCore core;
        public ExtTimelineObject source;
        public float xOffset = 200;
        public List<ExtTimelineKeyframe> keyframes = new List<ExtTimelineKeyframe>();

        public float currentTime;
        public void UpdateTime(float time)
        {
            int index = FindMinimumKey(time);
            if (index > -1)
            {
                if (index + 1 >= keyframes.Count)
                {
                    source.SetValue(keyframes[index].value);
                }
                else
                {
                    var start = keyframes[index];
                    var end = keyframes[index + 1];

                    float duration = end.time - start.time;
                    var value = TimeLineUtility.GetTweenedValue(start.GetType(), start.value, end.value, duration, time, start.easeType);
                    source.SetValue(value);
                }
            }
        }

        public int FindMinimumKey(float time)
        {
            for(int i = 0; i < keyframes.Count; i++)
            {
                if (keyframes[i].time >= time) return i;
            }
            return -1;
        }

        public void RegisterKeyframe(object value)
        {
            var keyframe = keyframes.Find(val => val.time == currentTime);
            if(keyframe == null)
            {
                var inst = Instantiate(core.keyframePrefab, transform);
                keyframe = inst.GetComponent<ExtTimelineKeyframe>();
                keyframe.lane = this;
                keyframe.source = source;
                keyframe.easeType = Easings.EaseType.Linear;
                keyframe.time = currentTime;
                keyframe.value = value;
                keyframes.Add(keyframe);
            }
            else
            {
                keyframe.source = source;
                keyframe.value = value;
                keyframe.time = currentTime;
            }
        }
    }
}
