using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using Newtonsoft.Json;

public static class SettingsCenter
{
	static Settings s_settings;
	static bool isFetched;
	
	public static Settings settings
	{
		get
		{
			return isFetched ? s_settings : Fetch();
		}
		set
		{
			s_settings = value;
		}
	}
	
	public static Settings Fetch()
	{
		var s = PlayerPrefs.GetString("Settings", null);
		s_settings = string.IsNullOrWhiteSpace(s) ? new Settings() : s.Deserialize<Settings>();
		isFetched = true;
		return s_settings;
	}
	
	public static void Save()
	{
		PlayerPrefs.SetString("Settings", s_settings.Serialize());
	}
    public static T FetchStaticInstance<T>(T cache)
    {
        return EqualityComparer<T>.Default.Equals(cache) ? (T)(object)UnityEngine.Object.FindObjectOfType(typeof(T)) : cache;
    }
}
public class Settings
{
	public string version = "0.3";
	public Graphic graphic = new Graphic();
	public Gameplay gameplay = new Gameplay();
	public Audio audio = new Audio();
	public Credentials credentials = new Credentials();
	
	public class Graphic
	{
		public int antiAliasing;
		public int postProcessing;
		
		public override string ToString()
		{
			return string.Format("[Graphic AntiAliasing={0}, PostProcessing={1}]", antiAliasing, postProcessing);
		}
	}
	
	public class Audio
	{
		public float volume = 100;
		public float pitch = 1;
		public float delay;
		public override string ToString()
		{
			return string.Format("[Audio Volume={0}, Pitch={1}, Delay={2}]", volume, pitch, delay);
		}
	}
	
	public class Gameplay
	{
		public bool showCredit = true;
		public bool showProgressBar;
		public override string ToString()
		{
			return string.Format("[Gameplay ShowCredit={0}, ShowProgressBar={1}]", showCredit, showProgressBar);
		}
	}
	
	public class Credentials
	{
		public string name;
		public string email;
		public string password;
		public override string ToString()
		{
			return string.Format("[Credentials Name={0}, Email={1}, Password={2}]", name, email, password);
		}
	}
	
	public override string ToString()
	{
		return string.Format("[Settings Version={0}, Graphic={1}, Gameplay={2}, Audio={3}, Credentials={4}]", version, graphic, gameplay, audio, credentials);
	}

}
