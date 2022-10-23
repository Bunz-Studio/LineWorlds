using UnityEngine;
using UnityEngine.UI;

public class LoadingPanelInstance : MonoBehaviour
{
    public Text titleLabel;
    public Text infoLabel;
    public Slider progressBar;

    public Button closeButton;
    public Button cancelButton;

    public void SetProgress(float value)
    {
        progressBar.value = value;
    }
}
