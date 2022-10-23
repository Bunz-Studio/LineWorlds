using System;
using UnityEngine;
using UnityEngine.UI;

public class UITextSlider : MonoBehaviour
{
	public enum SliderType
	{
		Resolution,
		Quality,
		FullScreen,
		PostProcessing
	}
	public SliderType type;
	public string[] texts;
	public int value;
	
	public Text text;
	
	void Start()
	{
		switch (type)
		{
			case SliderType.Quality:
				ChangeValueWithoutEffect(QualitySettings.GetQualityLevel());
				break;
			case SliderType.PostProcessing:
				ChangeValueWithoutEffect(SettingsCenter.settings.graphic.postProcessing);
				break;
			case SliderType.FullScreen:
				ChangeValueWithoutEffect(Screen.fullScreen ? 1 : 0);
				break;
            case SliderType.Resolution:
                for(int i = resW.Length - 1; i > -1; i--)
                {
                    var res = resW[i];
                    int selected = resW.Length - 1;

                    if (res >= Screen.width) selected = i;

                    ChangeValueWithoutEffect(selected);
                }
                break;
			default:
				ChangeValueWithoutEffect(value);
				break;
		}
	}
	
	public void ChangeValue(int to)
	{
		ChangeValueWithoutEffect(to);
		switch (type)
		{
			case SliderType.Quality:
				QualitySettings.SetQualityLevel(value);
				break;
			case SliderType.PostProcessing:
				SettingsCenter.settings.graphic.postProcessing = value;
				SettingsCenter.Save();
				break;
			case SliderType.FullScreen:
				Screen.fullScreen = value == 1;
				break;
			case SliderType.Resolution:
				Screen.SetResolution(resW[value], resH[value], Screen.fullScreen);
				break;
		}
	}
	
	public void Increase()
	{
		ChangeValue(value + 1);
	}
	
	public void Descrease()
	{
		ChangeValue(value - 1);
	}
	int[] resW = {640, 960, 1280, 1366, 1920, 99999};
	int[] resH = {360, 540, 720,  768,  1080, 99999};
	
	public void ChangeValueWithoutEffect(int to)
	{
		var a = to >= texts.Length;
		var b = to < 0;
		value = a ? 0 : (b ? to = texts.Length - 1 : to);
		/*string[] infos = {
			"To : " + to.ToString(),
			"Texts length : " + (texts.Length - 1).ToString(),
			"To above length : " + a.ToString(),
			"To below length : " + b.ToString(),
			"Result : " + value
		};
		Debug.Log(string.Join(Environment.NewLine, infos));*/
		text.text = texts[value];
	}
}
