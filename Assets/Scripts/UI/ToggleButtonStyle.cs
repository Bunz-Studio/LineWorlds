using UnityEngine;
using UnityEngine.UI;

public class ToggleButtonStyle : MonoBehaviour
{
    private Button thisButton;
    private Image thisImage;

    public bool isOn;
    public Color offColor = Color.white;
    public Color onColor = Color.gray;
    void Start()
    {
        thisButton = GetComponent<Button>();
        thisImage = GetComponent<Image>();

        thisButton.onClick.AddListener(ToggleColor);
    }

    public void ToggleColor()
    {
        isOn = !isOn;
        thisImage.color = isOn ? onColor : offColor;
    }
}
