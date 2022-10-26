using UnityEngine;

public class MessageBox : MonoBehaviour
{
	public static MessageBox instance;
	
	public Transform parent;
	public GameObject prefab;

	void Start()
	{
		if(instance == null) instance = this;
	}
	
	public static MessageBoxItem Show(string content, string title = null, MessageBoxButton[] buttons = null, bool showCloseButton = true)
	{
		if(buttons == null)
		{
			var butt = new MessageBoxButton("Ok");
			var msgBox = ShowNondefault(content, title, new []{butt}, showCloseButton);
			butt.onClick += msgBox.DestroyMyself;
			return msgBox;
		}
		else
		{
			var msgBox = ShowNondefault(content, title, buttons, showCloseButton);
			return msgBox;
		}
	}
	
	public static MessageBoxItem ShowNondefault(string content, string title = null, MessageBoxButton[] buttons = null, bool showCloseButton = true)
	{
		var obj = Instantiate(instance.prefab, instance.parent);
		var comp = obj.GetComponent<MessageBoxItem>();
		comp.title.text = title;
		comp.content.text = content;
		comp.SetCloseButton(showCloseButton);
		if(buttons != null)
		{
			foreach(var btt in buttons)
			{
				comp.AddButton(btt);
			}
		}
		return comp;
	}
	
	public class MessageBoxButton
	{
		public string name;
        public bool closeBox;
		public System.Action onClick;
		
		public MessageBoxButton(string name)
		{
			this.name = name;
        }

        public MessageBoxButton(string name, bool closeBox)
        {
            this.name = name;
            this.closeBox = closeBox;
        }

        public MessageBoxButton(string name, bool closeBox, System.Action onClick)
        {
            this.name = name;
            this.closeBox = closeBox;
            this.onClick = onClick;
        }

        public MessageBoxButton(string name, System.Action onClick)
        {
            this.name = name;
            this.onClick = onClick;
        }
    }
}
