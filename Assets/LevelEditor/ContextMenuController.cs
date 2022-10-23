using UnityEngine;
using UnityEngine.EventSystems;

public class ContextMenuController : MonoBehaviour
{
	public GameObject[] contextMenus;
	public int openedIndex = -1;
	
	void Update()
	{
		if(Input.GetMouseButtonDown(0) && !ExternMaker.ExtUtility.IsInputHoveringUI())
		{
			CloseContextMenus();
		}
	}
	
	public void OpenContextMenu(int index)
	{
		bool result = openedIndex != index;
		openedIndex = result ? index : -1;
		for(int i = 0; i < contextMenus.Length; i++)
		{
			contextMenus[i].SetActive(index == i && result);
		}
	}
	
	public void CloseContextMenus()
	{
		openedIndex = -1;
		for(int i = 0; i < contextMenus.Length; i++)
		{
			contextMenus[i].SetActive(false);
		}
	}
}
