using TMPro;
using UnityEngine;

public class LocalizationUIText : MonoBehaviour
{
    [SerializeField] private string key;
     private TMP_Text localizationText;

    private void Start()
    {
        localizationText = GetComponent<TMP_Text>();
        LocalizationManager.Instance.OnUpdateTextsByLanguageChanged += UpdateText;
        UpdateText();
    }

    private void OnDestroy()
    {
        LocalizationManager.Instance.OnUpdateTextsByLanguageChanged -= UpdateText;
    }

    protected virtual void UpdateText()
    {
        string newText = LocalizationManager.Instance.GetLocalizedText(key);
        if (!localizationText.text.Equals(newText))
        {
            localizationText.text = LocalizationManager.Instance.GetLocalizedText(key);
        }
    }
}
