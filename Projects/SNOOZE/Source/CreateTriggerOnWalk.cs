using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LineWorldsMod;
using ExternMaker;

namespace SNOOZE
{
	public class CustomTrigger : ModTrigger
	{
		// Called when the game starts
		public GameObject triggerInstance;
		public Vector3 offset = new Vector3(1, 0, 1);
		public KeyCode keyCode = KeyCode.C;
		
		void Update()
		{
			if(Input.GetKeyDown(keyCode))
			{
				var obj = ExtCore.instance.CreateObject(triggerInstance);
				obj.transform.position = ModAccess.line.transform.position + offset;
			}
		}
	}
}
