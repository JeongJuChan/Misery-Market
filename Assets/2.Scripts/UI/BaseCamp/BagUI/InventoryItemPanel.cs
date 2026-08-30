using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class InventoryItemPanel : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI itemInfoText;
    [SerializeField] private Button itemButton;

    public void AddButtonListener(UnityAction action)
    {
        itemButton.onClick.AddListener(action);
    }

    public void SetItem(Sprite icon, string itemInfo)
    {
        iconImage.sprite = icon;
        itemInfoText.text = itemInfo;
    }
}
