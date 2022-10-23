using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;

// disable MemberCanBeMadeStatic.Local
public class UtilitiesFunc : MonoBehaviour
{
	// Application
	
	/// <summary>
	/// Literally just quits the game
	/// </summary>
	public void Exit()
	{
		Application.Quit();
	}
	
	// Screen
	
	/// <summary>
	/// Set the game's resolution depends on a string parse
	/// </summary>
	/// <param name="to">The string with format "width|height"</param>
	public void SetResolution(string to)
	{
		var s = to.Split('|');
		Screen.SetResolution(int.Parse(s[0]), int.Parse(s[1]), Screen.fullScreen);
	}
	
	/// <summary>
	/// Setting the game's fullScreen
	/// </summary>
	/// <param name="to"></param>
	public void SetFullScreen(bool to)
	{
		Screen.fullScreen = to;
	}
	
	/// <summary>
	/// Toggle the game's full screen
	/// </summary>
	public void ToggleFullScreen()
	{
		Screen.fullScreen = !Screen.fullScreen;
	}
	
	// Game
	
	/// <summary>
	/// Setting the game quality
	/// </summary>
	/// <param name="index">Quality index</param>
	public void SetQuality(int index)
	{
		QualitySettings.SetQualityLevel(index);
	}
	
	/// <summary>
	/// Setting the fog
	/// </summary>
	/// <param name="to">To be</param>
	public void SetFog(bool to)
	{
		RenderSettings.fog = to;
	}
	
	/// <summary>
	/// Opens an url
	/// </summary>
	/// <param name="url">The target url</param>
	public void OpenURL(string url)
	{
		Application.OpenURL(url);
	}
}

public static class GameUtil
{
	public static void TryInvoke(this System.Action action)
	{
		if(action != null) action.Invoke();
	}
	public static int RevertClamp (this int v, int min, int max)
	{
		v = v < min ? max : v;
		return v > max ? min : v;
	}
	
	public static string Serialize(this object obj)
	{
		return JsonConvert.SerializeObject(obj);
	}
	
	public static object Deserialize(this string str)
	{
		return JsonConvert.DeserializeObject(str);
	}
	
	public static T Deserialize<T>(this string str)
	{
		return JsonConvert.DeserializeObject<T>(str);
	}
}
