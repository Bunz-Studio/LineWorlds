using System;
using System.Reflection;
using UnityEngine;
using PaxScript.Net;

public class ScriptTrigger : Trigger
{
	public string scriptPath;
	public string scriptCache;
	public PaxScripter scriptEngine;
	Type[] AllTypes;
	
	public override void OnStart()
	{
		AllTypes = Assembly.GetExecutingAssembly().GetTypes();
		scriptEngine = new PaxScripter();
		scriptEngine.OnChangeState += OnScripterChangeState;
		if(Storage.FileExistsLocal(scriptPath))
		{
			scriptCache = Storage.ReadAllTextLocal(scriptPath);
			scriptEngine.Compile();
		}
	}
	
	public override void OnEnter(Collider other)
	{
		RunScript();
	}
	
	public void RunScript()
	{
		scriptEngine.Invoke(RunMode.Run, null, "TriggerLine.Main");
		scriptEngine.Reset();
	}

	public void OnScripterChangeState(PaxScripter sender, ChangeStateEventArgs e)
	{
		Debug.Log(name + ": " + e.NewState);

		if (e.OldState == ScripterState.Init)
		{
			sender.AddModule("1");
			sender.AddCode("1", scriptCache);
			foreach(var a in AllTypes)
			{
				sender.RegisterType(a);
			}
			sender.RegisterInstance("trigger", this);
		}
		else if (sender.HasErrors)
		{
			string errs = null;
			foreach(ScriptError er in sender.Error_List)
			{
				errs += er.Message + " | Line: " + er.LineNumber + System.Environment.NewLine;
				
			}
			Debug.LogWarning(name + ": " + errs);
		}
	}
}