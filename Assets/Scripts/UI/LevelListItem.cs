using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LevelListItem : MonoBehaviour
{
	public TextMeshProUGUI levelName;
	public LevelSelector manager;
	public LevelInfo info;
	
	public void Initialize(LevelInfo info, LevelSelector manager)
	{
		this.info = info;
		this.manager = manager;
		
		levelName.text = info.levelName;
	}
}
