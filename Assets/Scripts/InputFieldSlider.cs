using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class InputFieldSlider : MonoBehaviour
{
    public InputField inputField;
    public Slider slider;
    public UnityEvent<float> onValueChanged = new UnityEvent<float>();

    void Start()
    {
        inputField.text = slider.value.ToString();
        inputField.onEndEdit.AddListener(UpdateSliderFromInputField);
        slider.onValueChanged.AddListener(UpdateInputFieldFromSlider);
    }

    void UpdateInputFieldFromSlider(float value)
    {
        inputField.text = value.ToString();
        onValueChanged.Invoke(value);
    }

    void UpdateSliderFromInputField(string value)
    {
        if (float.TryParse(value, out float floatValue))
            slider.value = floatValue;

        inputField.text = slider.value.ToString();
    }
}
