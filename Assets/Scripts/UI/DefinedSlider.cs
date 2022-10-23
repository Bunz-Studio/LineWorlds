using UnityEngine;
using UnityEngine.UI;
using ExternMaker;

public class DefinedSlider : MonoBehaviour
{
    public Slider.SliderEvent onSliderUpdate;

    public Slider slider;
    public InputField field;

    private void Start()
    {
        if (onSliderUpdate != null)
        {
            slider.onValueChanged.AddListener(val => TryUpdate(val, false, true));
            field.onValueChanged.AddListener(val => TryUpdate(Mathf.Clamp(ExtFieldInspect.TryParse(field, slider.value), slider.minValue, slider.maxValue)));
            field.onEndEdit.AddListener(val => TryUpdate(Mathf.Clamp(ExtFieldInspect.TryParse(field, slider.value, true), slider.minValue, slider.maxValue)));
        }
        UpdateField();
    }

    public void UpdateField()
    {
        field.text = slider.value.ToString();
    }

    bool isUpdating;
    public void TryUpdate(float value, bool updateSlider = true, bool updateField = false)
    {
        if (isUpdating) return;

        isUpdating = true;
        if (onSliderUpdate != null) onSliderUpdate.Invoke(value);
        if (updateSlider) slider.value = value;
        if (updateField) field.text = value.ToString();
        isUpdating = false;
    }
}
