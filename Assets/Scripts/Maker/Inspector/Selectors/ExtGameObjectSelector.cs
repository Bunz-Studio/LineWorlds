using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ExternMaker
{
	public class ExtGameObjectSelector : ExtObjectSelector
	{
		public ExtObject[] objects;
		public bool isInitialized;

        public InputField searchInput;

        public CanvasGroup canvasGroup;
        public CanvasGroup buttonsCanvasGroup;
        public ToggleButtonStyle toggleButton;

        public List<Transform> selectedObjects = new List<Transform>();
        public bool isListening;

        private void Start()
        {
            ExtCore.instance.OnObjectUpdate += SelectNew;
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.Return))
            {
                var selected = ExtUtility.GetSelectedUI();
                if (selected != null && selected == searchInput.gameObject)
                {
                    Search();
                }
            }
        }

        public override void Initialize(Object init, ExtInsUnityObject host)
		{
			base.Initialize(init, host);
            if (isListening)
            {
                SetSelection(false);
                toggleButton.ToggleColor();
            }
			isInitialized = true;
			gameObject.SetActive(true);
		}

        public void Search()
        {
            ClearList();
            objects = ExtCore.instance.GetObjects();
            foreach (var o in objects)
            {
                if(string.IsNullOrWhiteSpace(searchInput.text) || o.name.ToLower().Contains(searchInput.text.ToLower()))
                    AddToList(o.gameObject);
            }
        }

        public void SelectNew(List<GameObject> objs)
        {
            if(isListening)
            {
                var obj = objs.Count > 0 ? objs[0] : null;
                if(obj != null)
                {
                    ChooseObject(obj);
                    SetSelection(false);
                    toggleButton.ToggleColor();
                }
            }
        }

        public void ToggleSelection()
        {
            SetSelection(!isListening);
        }

        public void SetSelection(bool to)
        {
            isListening = to;
            if (to)
            {
                selectedObjects = new List<Transform>(ExtSelection.instance.transformSelection);
                canvasGroup.alpha = 0.5f;
            }
            else
            {
                ExtSelection.instance.transformSelection = selectedObjects;
                canvasGroup.alpha = 1;
            }
            buttonsCanvasGroup.interactable = !to;
            ExtInspector.instance.lockInspector = to;
            if(to)
                ExtSelection.instance.ClearTargets();
        }
	}
}