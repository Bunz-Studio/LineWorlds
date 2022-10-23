using UnityEngine;
using UnityEngine.UI;

public class MessageBoxItem : MonoBehaviour
{
	public Text title;
	public Text content;
	public CanvasGroup canvasGroup;
	
	public GameObject closeButton;
	
	public Transform parent;
	public GameObject buttonPrefab;
	
	public void AddButton(MessageBox.MessageBoxButton button)
	{
		var obj = Instantiate(buttonPrefab, parent);
		obj.GetComponent<Button>().onClick.AddListener(() => {
		                                               	if(button.onClick != null) button.onClick.Invoke();
		                                               });
		obj.GetComponentInChildren<Text>().text = button.name;
	}
	
	public void SetCloseButton(bool to)
	{
		closeButton.SetActive(to);
	}
	
	public void DestroyMyself()
	{
		Destroy(GetComponent<Animator>());
		canvasGroup.interactable = false;
		canvasGroup.blocksRaycasts = false;
		LeanTween.value(gameObject, 1, 0.9f, 0.5f).setEase(LeanTweenType.easeOutCubic).setOnUpdate((float val) =>
			transform.localScale = new Vector3(val, val, 1)
		);
		LeanTween.value(gameObject, 1, 0, 0.5f).setEase(LeanTweenType.easeOutCubic).setOnUpdate((float val) => {
		                                                    	canvasGroup.alpha = val;
		                                                    });//.setOnComplete(() => DestroyImmediate(this.gameObject));
		Destroy(gameObject, 0.5f);
	}
}
