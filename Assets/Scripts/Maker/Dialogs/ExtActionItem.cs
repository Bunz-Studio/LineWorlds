using UnityEngine;
using UnityEngine.UI;

public class ExtActionItem : MonoBehaviour
{
    public Text titleText;
    public Text contentText;
    public Text counterText;

    public int count;
    public void Add()
    {
        count++;
        counterText.text = count.ToString();
    }
}