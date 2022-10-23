using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class AssignEvents : MonoBehaviour {
	public List<EventAssigns> eventAss = new List<EventAssigns>();
	public bool assign;
	
	void Update()
	{
		if(assign)
		{
			Assign();
			assign = false;
		}
	}
	public void Assign()
	{
		var yay = transform.GetComponentsInChildren<TriggerEvent>();
		foreach(var a in yay)
		{
			var ev = GetEvent(a.name);
			if(ev != null){
				a.action = ev.eve;
			}
		}
	}
	public EventAssigns GetEvent (string name)
	{
		foreach(var a in eventAss)
		{
			if(name.StartsWith(a.name, System.StringComparison.CurrentCulture)) return a;
		}
		return null;
	}
}

[System.Serializable]
public class EventAssigns
{
	public string name;
	public UnityEngine.Events.UnityEvent eve;
}