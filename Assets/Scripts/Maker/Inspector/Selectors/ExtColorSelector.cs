using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ExternMaker
{
	public class ExtColorSelector : MonoBehaviour
	{
		public Transform parent;
		public ExtInsColor host;
		public Image colorImage;
		
		public Slider rSlider;
		public InputField rField;
		public Slider gSlider;
		public InputField gField;
		public Slider bSlider;
		public InputField bField;
		public Slider aSlider;
		public InputField aField;
		
		public Color previousColor;
		public Color selectedColor;
		
		public virtual void Initialize(Color init, ExtInsColor host)
		{
			previousColor = init;
			selectedColor = init;
			this.host = host;
			
			UpdateUI();
			
			gameObject.SetActive(true);
		}
		
		public void UpdateUI()
		{
			colorImage.color = selectedColor;
			
			rSlider.value = selectedColor.r;
			gSlider.value = selectedColor.g;
			bSlider.value = selectedColor.b;
			aSlider.value = selectedColor.a;
			
			rField.text = (selectedColor.r * 255).ToString("0");
			gField.text = (selectedColor.g * 255).ToString("0");
			bField.text = (selectedColor.b * 255).ToString("0");
			aField.text = (selectedColor.a * 255).ToString("0");
		}
		
		public virtual void ChooseObject(Color obj)
		{
			selectedColor = obj;
			UpdateUI();
			host.SelectObject(selectedColor);
		}
		
		public virtual void Show()
		{
			gameObject.SetActive(true);
		}
		
		public virtual void FinalClose()
		{
			if(!previousColor.Equals(selectedColor)) host.EndObject(selectedColor);
			gameObject.SetActive(false);
		}
		
		public void ChangeR(float f)
		{
			selectedColor.r = f;
			ChooseObject(selectedColor);
		}
		
		public void ChangeG(float f)
		{
			selectedColor.g = f;
			ChooseObject(selectedColor);
		}
		
		public void ChangeB(float f)
		{
			selectedColor.b = f;
			ChooseObject(selectedColor);
		}
		
		public void ChangeA(float f)
		{
			selectedColor.a = f;
			ChooseObject(selectedColor);
		}
		
		public void ChangeRText()
		{
			selectedColor.r = ExtFieldInspect.TryParse(rField, selectedColor.r * 255) / 255;
			ChooseObject(selectedColor);
		}
		
		public void ChangeGText()
		{
			selectedColor.g = ExtFieldInspect.TryParse(gField, selectedColor.g * 255) / 255;
			ChooseObject(selectedColor);
		}
		
		public void ChangeBText()
		{
			selectedColor.b = ExtFieldInspect.TryParse(bField, selectedColor.b * 255) / 255;
			ChooseObject(selectedColor);
		}
		
		public void ChangeAText()
		{
			selectedColor.a = ExtFieldInspect.TryParse(aField, selectedColor.a * 255) / 255;
			ChooseObject(selectedColor);
		}
		
		public void ChangeRTextFinal()
		{
			selectedColor.r = ExtFieldInspect.TryParse(rField, selectedColor.r * 255, true) / 255;
			ChooseObject(selectedColor);
		}
		
		public void ChangeGTextFinal()
		{
			selectedColor.g = ExtFieldInspect.TryParse(gField, selectedColor.g * 255, true) / 255;
			ChooseObject(selectedColor);
		}
		
		public void ChangeBTextFinal()
		{
			selectedColor.b = ExtFieldInspect.TryParse(bField, selectedColor.b * 255, true) / 255;
			ChooseObject(selectedColor);
		}
		
		public void ChangeATextFinal()
		{
			selectedColor.a = ExtFieldInspect.TryParse(aField, selectedColor.a * 255, true) / 255;
			ChooseObject(selectedColor);
		}
	}
}