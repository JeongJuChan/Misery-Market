using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CloseUIButton : MonoBehaviour
{
    [SerializeField] private Button closeButton;
    [SerializeField] private UIId targetUIId;
    [SerializeField] private UnityEvent onClose;

    void Awake()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseButtonClick);
        }
    }

    private void OnCloseButtonClick()
    {
        if (targetUIId != UIId.None)
        {
            UIManager.Instance.Hide(targetUIId);
            onClose?.Invoke();
        }
    }
}
