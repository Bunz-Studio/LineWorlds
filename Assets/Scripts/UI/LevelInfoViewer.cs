using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelInfoViewer : MonoBehaviour
{
    public LevelInfo info;
    public TextMeshProUGUI levelTitle;
    public TextMeshProUGUI levelDescription;
    public TextMeshProUGUI levelAuthor;

    public void Initialize(LevelInfo info)
    {
        this.info = info;
        levelTitle.text = info.levelName;
        levelDescription.text = info.levelDescription;
        levelAuthor.text = info.levelAuthor;
    }

    public void PlayLevel()
    {
        CrossSceneManager.LoadLevel(info.sceneName, Color.black, Color.white);
    }
}