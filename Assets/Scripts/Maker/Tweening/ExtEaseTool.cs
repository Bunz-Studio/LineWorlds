using System;
using System.Collections.Generic;
using UnityEngine;

namespace ExternMaker.Easings
{
	public class EaseTool : MonoBehaviour
	{
		public static EaseTool self;
		public static List<Easeable> easeables = new List<Easeable>();
		public static List<Easeable> onDestroy = new List<Easeable>();
		
		public void Start()
		{
			self = this;
		}
		
		public void Update()
		{
			for (int i = 0; i < easeables.Count; i++)
			{
				var e = easeables[i];
				e.Ease();
				if(e.finished) onDestroy.Add(e);
			}
			for (int i = 0; i < onDestroy.Count; i++)
			{
				easeables.Remove(onDestroy[i]);
			}
		}
		
		public static FloatEaseable TweenFloat(float start, float end, float time)
		{
			var t = new FloatEaseable {
				start = start,
				end = end,
				length = time
			};
			easeables.Add(t);
			return t;
		}
		
		public static Vector2Easeable TweenVector2(Vector2 start, Vector2 end, float time)
		{
			var t = new Vector2Easeable {
				start = start,
				end = end,
				length = time
			};
			easeables.Add(t);
			return t;
		}
		
		public static ColorEaseable TweenFloat(Color start, Color end, float time)
		{
			var t = new ColorEaseable {
				start = start,
				end = end,
				length = time
			};
			easeables.Add(t);
			return t;
		}
	}
	
	public class Easeable
	{
		public EaseType type = EaseType.Linear;
		public object start;
		public object end;
		public object result;
		public bool finished;
		public float current;
		public float length;
		public Action<object> OnUpdate;
		
		public Easeable SetEase(EaseType type)
		{
			this.type = type;
			return this;
		}
		
		public Easeable SetOnUpdate(Action<object> obj)
		{
			OnUpdate = obj;
			return this;
		}
		
		public virtual void Ease()
		{
			var n = (float)current + Time.deltaTime;
			current = n > length ? GetFinal() : n;
		}

        public virtual object GetValueAtTime(object start, object end, float time, float duration)
        {
            return null;
        }
		
		float GetFinal()
		{
			finished = true;
			return length;
		}
		
		public virtual float ThrowValue(float t)
		{
			return type == EaseType.Linear ? EaseFunc.Linear(t) :
				type == EaseType.InQuad ? EaseFunc.InQuad(t) :
				type == EaseType.OutQuad ? EaseFunc.OutQuad(t) :
				type == EaseType.InOutQuad ? EaseFunc.InOutQuad(t) :
				type == EaseType.InCubic ? EaseFunc.InCubic(t) :
				type == EaseType.OutCubic ? EaseFunc.OutCubic(t) :
				type == EaseType.InOutCubic ? EaseFunc.InOutCubic(t) : 
				type == EaseType.InQuart ? EaseFunc.InQuart(t) :
				type == EaseType.OutQuart ? EaseFunc.OutQuart(t) : 
				type == EaseType.InOutQuart ? EaseFunc.InOutQuart(t) : 
				type == EaseType.InQuint ? EaseFunc.InQuint(t) :
				type == EaseType.OutQuint ? EaseFunc.OutQuint(t) :
				type == EaseType.InOutQuint ? EaseFunc.InOutQuint(t) :
				type == EaseType.InSine ? EaseFunc.InSine(t) :
				type == EaseType.OutSine ? EaseFunc.OutSine(t) : 
				type == EaseType.InOutSine ? EaseFunc.InOutSine(t) : 
				type == EaseType.InExpo ? EaseFunc.InExpo(t) : 
				type == EaseType.OutExpo ? EaseFunc.OutExpo(t) :
				type == EaseType.InOutExpo ? EaseFunc.InOutExpo(t) :
				type == EaseType.InCirc ? EaseFunc.InCirc(t) :
				type == EaseType.OutCirc ? EaseFunc.OutCirc(t) :
				type == EaseType.InOutCirc ? EaseFunc.InOutCirc(t) : 
				type == EaseType.InElastic ? EaseFunc.InElastic(t) :
				type == EaseType.OutElastic ? EaseFunc.OutElastic(t) :
				type == EaseType.InOutElastic ? EaseFunc.InOutElastic(t) : 
				type == EaseType.InBack ? EaseFunc.InBack(t) :
				type == EaseType.OutBack ? EaseFunc.OutBack(t) :
				type == EaseType.InOutBack ? EaseFunc.InOutBack(t) : 
				type == EaseType.InBounce ? EaseFunc.InBounce(t) :
				type == EaseType.OutBounce ? EaseFunc.OutBounce(t) : EaseFunc.InOutBounce(t);
		}
	}
	
	public enum EaseType
	{
		Linear,
		InQuad,
		OutQuad,
		InOutQuad,
		InCubic,
		OutCubic,
		InOutCubic,
		InQuart,
		OutQuart,
		InOutQuart,
		InQuint,
		OutQuint,
		InOutQuint,
		InSine,
		OutSine,
		InOutSine,
		InExpo,
		OutExpo,
		InOutExpo,
		InCirc,
		OutCirc,
		InOutCirc,
		InElastic,
		OutElastic,
		InOutElastic,
		InBack,
		OutBack,
		InOutBack,
		InBounce,
		OutBounce,
		InOutBounce
	}

    // Easeable
    public class Vector3Easeable : Easeable
    {
        public override void Ease()
        {
            var a = (Vector3)start;
            var b = (Vector3)end;
            base.Ease();
            var r = ThrowValue(current / length);
            var m = b - a;
            result = a + (m * r);
            if (OnUpdate != null) OnUpdate.Invoke(result);
        }

        public override object GetValueAtTime(object start, object end, float time, float duration)
        {
            var a = (Vector3)start;
            var b = (Vector3)end;
            var r = ThrowValue(time / duration);
            var m = b - a;
            return a + (m * r);
        }
    }

    public class Vector2Easeable : Easeable
	{
		public override void Ease()
		{
			var a = (Vector2)start;
			var b = (Vector2)end;
			base.Ease();
			var r = ThrowValue(current / length);
			var m = b - a;
			result = a + (m * r);
			if(OnUpdate != null) OnUpdate.Invoke(result);
		}

        public override object GetValueAtTime(object start, object end, float time, float duration)
        {
            var a = (Vector2)start;
            var b = (Vector2)end;
            var r = ThrowValue(time / duration);
            var m = b - a;
            return a + (m * r);
        }
    }
	
	public class FloatEaseable : Easeable
	{
		public override void Ease()
		{
			var a = (float)start;
			var b = (float)end;
			base.Ease();
			var r = ThrowValue(current / length);
			var m = b - a;
			result = a + (m * r);
			if(OnUpdate != null) OnUpdate.Invoke(result);
        }

        public override object GetValueAtTime(object start, object end, float time, float duration)
        {
            var a = (float)start;
            var b = (float)end;
            var r = ThrowValue(time / duration);
            var m = b - a;
            return a + (m * r);
        }
    }

    public class IntEaseable : Easeable
    {
        public override void Ease()
        {
            var a = (int)start;
            var b = (int)end;
            base.Ease();
            var r = ThrowValue(current / length);
            var m = b - a;
            result = a + (m * r);
            if (OnUpdate != null) OnUpdate.Invoke(result);
        }

        public override object GetValueAtTime(object start, object end, float time, float duration)
        {
            var a = (int)start;
            var b = (int)end;
            var r = ThrowValue(time / duration);
            var m = b - a;
            return a + (m * r);
        }
    }

    public class ColorEaseable : Easeable
	{
		public override void Ease()
		{
			var a = (Color)start;
			var b = (Color)end;
			base.Ease();
			var r = ThrowValue(current / length);
			var m = b - a;
			result = a + (m * r);
			if(OnUpdate != null) OnUpdate.Invoke(result);
        }

        public override object GetValueAtTime(object start, object end, float time, float duration)
        {
            var a = (Color)start;
            var b = (Color)end;
            var r = ThrowValue(time / duration);
            var m = b - a;
            return a + (m * r);
        }
    }
}
