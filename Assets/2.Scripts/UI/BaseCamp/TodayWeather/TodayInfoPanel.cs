using TMPro;
using UnityEngine;

public class TodayInfoPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI weatherText;

    public void SetWeatherText(string text)
    {
        if (weatherText != null)
        {
            weatherText.text = text;
        }
    }

    public void UpdateActiveState(bool isActive)
    {
        gameObject.SetActive(isActive);
    }
}
