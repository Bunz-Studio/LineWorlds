using System;
using UnityEngine;

namespace ExternMaker.Easings
{
	public static class EaseFunc
	{
		public static float Linear(float t)
		{
			return t;
		}
		public static float InQuad(float t)
		{
			return t * t;
		}
		public static float OutQuad(float t)
		{
			return 1 - InQuad(1 - t);
		}
		public static float InOutQuad(float t)
		{
			return t < 0.5 ? InQuad(t * 2) / 2 : 1 - InQuad((1 - t) * 2) / 2;
		}
		public static float InCubic(float t)
		{
			return t * t * t;
		}
		public static float OutCubic(float t)
		{
			return 1 - InCubic(1 - t);
		}
		public static float InOutCubic(float t)
		{
			return t < 0.5 ? InCubic(t * 2) / 2 : 1 - InCubic((1 - t) * 2) / 2;
		}
		public static float InQuart(float t)
		{
			return t * t * t * t;
		}
		public static float OutQuart(float t)
		{
			return 1 - InQuart(1 - t);
		}
		public static float InOutQuart(float t)
		{
			return t < 0.5 ? InQuart(t * 2) / 2 : 1 - InQuart((1 - t) * 2) / 2;
		}
		public static float InQuint(float t)
		{
			return t * t * t * t * t;
		}
		public static float OutQuint(float t)
		{
			return 1 - InQuint(1 - t);
		}
		public static float InOutQuint(float t)
		{
			return t < 0.5 ? InQuint(t * 2) / 2 : 1 - InQuint((1 - t) * 2) / 2;
		}
		public static float InSine(float t)
		{
			return (float)-Math.Cos(t * Math.PI / 2);
		}
		public static float OutSine(float t)
		{
			return (float)Math.Sin(t * Math.PI / 2);
		}
		public static float InOutSine(float t)
		{
			return (float)(Math.Cos(t * Math.PI) - 1) / -2;
		}
		public static float InExpo(float t)
		{
			return (float)Math.Pow(2, 10 * (t - 1));
		}
		public static float OutExpo(float t)
		{
			return 1 - InExpo(1 - t);
		}
		public static float InOutExpo(float t)
		{
			return t < 0.5 ? InExpo(t * 2) / 2 : 1 - InExpo((1 - t) * 2) / 2;
		}
		public static float InCirc(float t)
		{
			return -((float)Math.Sqrt(1 - t * t) - 1);
		}
		public static float OutCirc(float t)
		{
			return 1 - InCirc(1 - t);
		}
		public static float InOutCirc(float t)
		{
			return t < 0.5 ? InCirc(t * 2) / 2 : 1 - InCirc((1 - t) * 2) / 2;
		}
		public static float InElastic(float t)
		{
			return 1 - OutElastic(1 - t);
		}
		public static float OutElastic(float t)
		{
			return (float)Math.Pow(2, -10 * t) * (float)Math.Sin((t - 0.3f / 4) * (2 * Math.PI) / 0.3f) + 1;
		}
		public static float InOutElastic(float t)
		{
			return t < 0.5 ? InElastic(t * 2) / 2 : 1 - InElastic((1 - t) * 2) / 2;
		}
		public static float InBack(float t)
		{
			float s = 1.70158f;
			return t * t * ((s + 1) * t - s);
		}
		public static float OutBack(float t)
		{
			return 1 - InBack(1 - t);
		}
		public static float InOutBack(float t)
		{
			return t < 0.5 ? InBack(t * 2) / 2 : 1 - InBack((1 - t) * 2) / 2;
		}
		public static float InBounce(float t)
		{
			return 1 - OutBounce(1 - t);
		}
		const float div = 2.75f;
		const float mult = 7.5625f;
		public static float OutBounce(float t)
		{
			return t < 1 / div ? mult * t * t : 
				t < 2 / div ? OBE1(t) :
				t < 2.5 / div ? OBE2(t) : 
				OBE3(t);
		}
		static float OBE1(float t)
		{
			t -= 1.5f / div;
			return mult * t * t + 0.75f;
		}
		static float OBE2(float t)
		{
			t -= 2.25f / div;
			return mult * t * t + 0.9375f;
		}
		static float OBE3(float t)
		{
			t -= 2.625f / div;
			return mult * t * t + 0.984375f;
		}
		public static float InOutBounce(float t)
		{
			return t < 0.5 ? InBounce(t * 2) / 2 : 1 - InBounce((1 - t) * 2) / 2;
		}
	}
}
