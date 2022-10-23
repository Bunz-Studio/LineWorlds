using UnityEngine;

public class LoadingPanelManager : MonoBehaviour
{
    public GameObject instance;
    public Transform parent;

    public static LoadingPanelManager self;
    private void Start()
    {
        self = this;
    }

    private void OnDestroy()
    {
        self = null;
    }

    public static LoadingPanelInstance CreatePanel(string info, string title = "Please wait...", bool isCancellable = false)
    {
        var obj = Instantiate(self.instance, self.parent);
        var instance = obj.GetComponent<LoadingPanelInstance>();
        instance.titleLabel.text = title;
        instance.infoLabel.text = info;

        instance.cancelButton.gameObject.SetActive(isCancellable);
        instance.closeButton.interactable = isCancellable;
        return instance;
    }
}
