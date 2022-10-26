using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ExternMaker
{
    public class ExtMaterialSelector : MonoBehaviour
    {
        [Header("Main")]
        public Transform parent;
        public ExtInsMaterial host;
        public Image colorImage;

        [Header("Shader")]
        public List<Shader> shaders = new List<Shader>();
        public Dropdown shaderDropdown;

        [Header("Color")]
        public Slider rSlider;
        public InputField rField;
        public Slider gSlider;
        public InputField gField;
        public Slider bSlider;
        public InputField bField;
        public Slider aSlider;
        public InputField aField;

        [Header("Values")]
        public Material previousMaterial;
        public Material selectedMaterial;

        bool isUpdating;

        public virtual void Initialize(Material init, ExtInsMaterial host)
        {
            previousMaterial = init;
            selectedMaterial = init;
            this.host = host;

            UpdateUI();
            shaderDropdown.onValueChanged.AddListener(val => {
                var color = selectedMaterial.color;
                selectedMaterial = new Material(shaders[val]);
                selectedMaterial.color = color;
                if(selectedMaterial.HasProperty("_Glossiness"))
                {
                    selectedMaterial.SetFloat("_Glossiness", 0);
                }
                ChooseObject(selectedMaterial);
            });

            gameObject.SetActive(true);
        }

        public void UpdateUI()
        {
            isUpdating = true;
            var shader = shaders.Find(val => val == selectedMaterial.shader);
            if(shader != null)
            {
                var dropdownItem = shaderDropdown.options.FindIndex(val => val.text == shader.name);
                if(dropdownItem > -1)
                {
                    shaderDropdown.value = dropdownItem;
                    shaderDropdown.interactable = true;
                }
                else
                {
                    shaderDropdown.captionText.text = shader.name;
                    shaderDropdown.interactable = false;
                }
            }
            else
            {
                if (selectedMaterial.shader != null)
                    shaderDropdown.captionText.text = selectedMaterial.shader.name;
                else
                    shaderDropdown.captionText.text = "No shaders are on this material";
                shaderDropdown.interactable = false;
            }
            colorImage.color = selectedMaterial.color;

            rSlider.value = selectedMaterial.color.r;
            gSlider.value = selectedMaterial.color.g;
            bSlider.value = selectedMaterial.color.b;
            aSlider.value = selectedMaterial.color.a;

            rField.text = (selectedMaterial.color.r * 255).ToString("0");
            gField.text = (selectedMaterial.color.g * 255).ToString("0");
            bField.text = (selectedMaterial.color.b * 255).ToString("0");
            aField.text = (selectedMaterial.color.a * 255).ToString("0");
            isUpdating = false;
        }

        public virtual void ChooseObject(Material obj)
        {
            if (isUpdating) return;
            selectedMaterial = obj;
            UpdateUI();
            host.SelectObject(selectedMaterial);
        }

        public virtual void Show()
        {
            gameObject.SetActive(true);
        }

        public virtual void FinalClose()
        {
            if (!previousMaterial.Equals(selectedMaterial)) host.EndObject(selectedMaterial);
            gameObject.SetActive(false);
        }

        public void ChangeR(float f)
        {
            selectedMaterial.color = new Color(f, selectedMaterial.color.g, selectedMaterial.color.b, selectedMaterial.color.a);
            ChooseObject(selectedMaterial);
        }

        public void ChangeG(float f)
        {
            selectedMaterial.color = new Color(selectedMaterial.color.r, f, selectedMaterial.color.b, selectedMaterial.color.a);
            ChooseObject(selectedMaterial);
        }

        public void ChangeB(float f)
        {
            selectedMaterial.color = new Color(selectedMaterial.color.r, selectedMaterial.color.g, f, selectedMaterial.color.a);
            ChooseObject(selectedMaterial);
        }

        public void ChangeA(float f)
        {
            selectedMaterial.color = new Color(selectedMaterial.color.r,selectedMaterial.color.g, selectedMaterial.color.b, f);
            ChooseObject(selectedMaterial);
        }

        public void ChangeRText()
        {
            float f = ExtFieldInspect.TryParse(rField, selectedMaterial.color.r * 255) / 255;
            selectedMaterial.color = new Color(f, selectedMaterial.color.g, selectedMaterial.color.b, selectedMaterial.color.a);
            ChooseObject(selectedMaterial);
        }

        public void ChangeGText()
        {
            float f = ExtFieldInspect.TryParse(gField, selectedMaterial.color.g * 255) / 255;
            selectedMaterial.color = new Color(selectedMaterial.color.r, f, selectedMaterial.color.b, selectedMaterial.color.a);
            ChooseObject(selectedMaterial);
        }

        public void ChangeBText()
        {
            float f = ExtFieldInspect.TryParse(bField, selectedMaterial.color.b * 255) / 255;
            selectedMaterial.color = new Color(selectedMaterial.color.r, selectedMaterial.color.g, f, selectedMaterial.color.a);
            ChooseObject(selectedMaterial);
        }

        public void ChangeAText()
        {
            float f = ExtFieldInspect.TryParse(aField, selectedMaterial.color.a * 255) / 255;
            selectedMaterial.color = new Color(selectedMaterial.color.r, selectedMaterial.color.g, selectedMaterial.color.b, f);
            ChooseObject(selectedMaterial);
        }

        public void ChangeRTextFinal()
        {
            float f = ExtFieldInspect.TryParse(rField, selectedMaterial.color.r * 255, true) / 255;
            selectedMaterial.color = new Color(f, selectedMaterial.color.g, selectedMaterial.color.b, selectedMaterial.color.a);
            ChooseObject(selectedMaterial);
        }

        public void ChangeGTextFinal()
        {
            float f = ExtFieldInspect.TryParse(gField, selectedMaterial.color.g * 255, true) / 255;
            selectedMaterial.color = new Color(selectedMaterial.color.r, f, selectedMaterial.color.b, selectedMaterial.color.a);
            ChooseObject(selectedMaterial);
        }

        public void ChangeBTextFinal()
        {
            float f = ExtFieldInspect.TryParse(bField, selectedMaterial.color.b * 255, true) / 255;
            selectedMaterial.color = new Color(selectedMaterial.color.r, selectedMaterial.color.g, f, selectedMaterial.color.a);
            ChooseObject(selectedMaterial);
        }

        public void ChangeATextFinal()
        {
            float f = ExtFieldInspect.TryParse(aField, selectedMaterial.color.a * 255, true) / 255;
            selectedMaterial.color = new Color(selectedMaterial.color.r, selectedMaterial.color.g, selectedMaterial.color.b, f);
            ChooseObject(selectedMaterial);
        }
    }
}