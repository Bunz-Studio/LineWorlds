using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class TextSmoother : MonoBehaviour
{
	public Text[] targets;
	public List<UndoData> undoData = new List<UndoData>();
	public float velocity = 2;
	public bool execute;
	public bool undo;
	
	void Update()
	{
		if(execute)
		{
			undoData.Clear();
			foreach(var targ in targets)
			{
				var data = new UndoData();
				var rt = targ.GetComponent<RectTransform>();
				data.fontSize = targ.fontSize;
				data.size = targ.transform.localScale;
				data.sizeDelta = rt.sizeDelta;
				targ.fontSize = System.Convert.ToInt32(targ.fontSize * velocity);
				var size = rt.rect.size;
				targ.transform.localScale = targ.transform.localScale / velocity;
				rt.sizeDelta = size;
				undoData.Add(data);
			}
			execute = false;
		}
		if(undo)
		{
			foreach(var data in undoData)
			{
				data.ApplyBack();
			}
			undo = false;
		}
	}
	
	[System.Serializable]
	public class UndoData
	{
		public Text target;
		public Vector3 size;
		public Vector3 sizeDelta;
		public int fontSize;
		
		public void ApplyBack()
		{
			target.fontSize = fontSize;
			target.transform.localScale = size;
			var rt = target.GetComponent<RectTransform>();
			rt.sizeDelta = sizeDelta;
		}
	}
}
