using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameplayUI : MonoBehaviour
{
	public LevelManager manager;
	
	public Animator diePanel;
	
	public TextMeshProUGUI levelTitle;
	public TextMeshProUGUI levelAuthor;
	public TextMeshProUGUI levelDetails;
	
	public Slider levelProgress;
    public TextMeshProUGUI levelPercentage;

    // Static Accessor
    public LevelManager s_manager
	{
		get
		{
			return manager ?? FindObjectOfType<LevelManager>();
		}
	}
	
    void Start()
    {
    	manager = s_manager;
    	manager.OnLineKilled.AddListener(PlayerKilled);
    	levelTitle.SetTextN(manager.info.levelName);
    	levelAuthor.SetTextN(manager.info.levelAuthor);
    }
    
    public void PlayerKilled()
    {
    	diePanel.enabled = true;
    	LeanTween.value(gameObject, 0, manager.source.time / manager.source.clip.length, 1).setEase(LeanTweenType.easeOutCubic).setOnUpdate(TweenFloat).setDelay(0.2f);
    }
    
    public void TweenFloat(float t)
    {
    	levelProgress.value = t;
    	levelPercentage.SetTextN((t * 100).ToString("0") + "%");
    }
}

public static class UIToolkit
{
	public static void SetTextN(this TextMeshProUGUI src, string text)
	{
		if(src != null) src.text = text;
	}
}
