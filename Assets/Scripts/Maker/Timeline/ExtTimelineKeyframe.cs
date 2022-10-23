using UnityEngine;
using System.Collections;
using ExternMaker.Easings;

namespace ExternMaker.Timeline
{
    public class ExtTimelineKeyframe : MonoBehaviour
    {
        public ExtTimelineObject source;
        public ExtTimelineLane lane;
        public EaseType easeType = EaseType.Linear;

        public float time
        {
            get
            {
                return (rect.anchoredPosition.x / (lane.core.timePerCoordinate * lane.core.timeScale)) - (lane.xOffset) - (rect.sizeDelta.x / 2);
            }
            set
            {
                rect.anchoredPosition = new Vector2((time * (lane.core.timePerCoordinate * lane.core.timeScale)) + (lane.xOffset) + (rect.sizeDelta.x / 2), rect.anchoredPosition.y);
            }
        }

        private RectTransform r;
        public RectTransform rect
        {
            get
            {
                r = r == null ? gameObject.GetRectTransform() : r;
                return r;
            }
            set
            {
                r = value;
            }
        }

        public object value;
    }
}
