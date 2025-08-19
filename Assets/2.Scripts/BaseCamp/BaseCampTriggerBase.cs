using UnityEngine;

public class BaseCampTriggerBase : MonoBehaviour
{
    private BaseCampToggleAnimation baseCampToggleAnimation;

    private void Awake()
    {
        baseCampToggleAnimation = GetComponent<BaseCampToggleAnimation>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Toggle();
    }

    protected virtual void Toggle()
    {
        // 아이템 로직
        baseCampToggleAnimation.Fade();
    }
}
