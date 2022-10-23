using System.Collections.Generic;
using UnityEngine;

public class GameTracker : MonoBehaviour
{
	public LevelManager manager;
	public static string userInfo;
	
	void Start()
	{
		manager.OnLineKilled.AddListener(GameEnd);
		manager.OnLineFinished.AddListener(GameEnd);
	}
	
	public void GameEnd()
	{
		try
		{
			var dcWeb = new GameTrackPost();
			dcWeb.ProfilePicture = "https://images-ext-1.discordapp.net/external/Tv3UFD587EYr6pVycKGxBllX1r-sAPmDwa0vFG3uRHI/%3Fsize%3D1024/https/cdn.discordapp.com/icons/708145159593918484/2f7079e7c973d8c675a9fa49c1853cdf.png?width=375&height=375";
        	dcWeb.UserName = "Score Poster";
        	dcWeb.WebHook = "https://discord.com/api/webhooks/974649541729087498/UlfsTqsAHxNjRTI6wLImO4vTusQITOTXgW50qiEmVMkqtjHEB62QEAoqOakRgjnjTsmL";
        	string a = null;
        	string playerInfo = string.IsNullOrWhiteSpace(userInfo) ? System.Environment.UserName : userInfo;
        	a = "Player: " + playerInfo + "\n" +
                "Level: " + manager.info.levelName + "\n" +
                "Taps: " + manager.lines[0].taps + "\n" +
	        	"Version: " + Application.version + "\n" +
        		"Progress: " + ((manager.source.time / manager.source.clip.length) * 100).ToString("0") + "%";
        	dcWeb.sndmsgg(a);
		}
		catch (System.Exception e)
		{
			Debug.Log("Failed to post score: " + e.Message);
		}
	}
}
