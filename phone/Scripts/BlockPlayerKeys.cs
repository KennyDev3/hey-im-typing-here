using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(TMP_InputField))]
public class BlockPlayerKeys : MonoBehaviour
{
    public TMP_InputField inputField;
    private void OnValidate()
    {
       

        if (inputField != null)
        {
            inputField.onValueChanged.AddListener(OnValueChanged);
        }
    }

    private void OnValueChanged(string newValue)
    {
        // Remove all instances of the letter 'r' (case-insensitive)
        string filteredText = newValue.Replace("r", "").Replace("R", "");

        // If the text has changed, update the InputField
        if (filteredText != newValue)
        {
            inputField.text = filteredText;
        }
    }
}
