using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSelector : MonoBehaviour
{
	public LevelListItem instance;
	public List<LevelListItem> instances = new List<LevelListItem>();
	
	public Transform infoParent;
	public List<LevelInfo> informations = new List<LevelInfo>();
	
    void Start()
    {
    	foreach(var info in informations)
    	{
    		var inst = Instantiate(instance.gameObject, infoParent);
    		var item = inst.GetComponent<LevelListItem>();
    		item.Initialize(info, this);
    		instances.Add(item);
    	}
    }

	public void PlayScene(string name)
	{
		CrossSceneManager.LoadLevel(name, Color.black, Color.white);
	}
}
