using TMPro;
using UnityEngine;

public class LocalizationUIText : MonoBehaviour
{
    [SerializeField] private string key;
     private TMP_Text localizationText;

    private LocalizationManager localizationManager;

    private void Start()
    {
        localizationText = GetComponent<TMP_Text>();
        localizationManager = LocalizationManager.Instance;
        localizationManager.OnUpdateTextsByLanguageChanged += UpdateText;
        UpdateText();
    }

    private void OnDestroy()
    {
        if (localizationManager == null)
        {
            return;
        }

        localizationManager.OnUpdateTextsByLanguageChanged -= UpdateText;
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
