using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LocalizationUISprite : MonoBehaviour
{
    [SerializeField] private string key;
    [SerializeField] private Image localizationImage;

    private void Start()
    {
        LocalizationManager.Instance.OnUpdateTextsByLanguageChanged += UpdateSprite;
        UpdateSprite();
    }

    private void OnDestroy()
    {
        LocalizationManager.Instance.OnUpdateTextsByLanguageChanged -= UpdateSprite;
    }

    private void UpdateSprite()
    {
        // localizationImage.sprite = LocalizationManager.Instance.GetLocalizedSprite(key);
    }
}
