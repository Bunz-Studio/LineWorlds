using UnityEngine;

public class TriggerEvent : Trigger
{
	public UnityEngine.Events.UnityEvent action;
	public UnityEngine.Events.UnityEvent undo;
	
	public override void OnEnter(Collider other)
	{
		action.Invoke();
	}
	public override void OnUndo()
	{
		undo.Invoke();
	}
}