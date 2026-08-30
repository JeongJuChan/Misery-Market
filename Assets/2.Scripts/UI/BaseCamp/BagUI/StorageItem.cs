using System;
using UnityEngine;
using UnityEngine.UI;

public class StorageItem : MonoBehaviour, IPoolable<StorageItem>
{
    public event Action<StorageItem> returnAction;
    [SerializeField] private Button button;
    [SerializeField] private bool isItemExist = false;
    [SerializeField] private Image itemImage;

    public void SetItemInfo(Sprite itemSprite, bool itemExist)
    {
        isItemExist = itemExist;
        itemImage.sprite = itemSprite;
        button.interactable = itemExist;
    }
}
