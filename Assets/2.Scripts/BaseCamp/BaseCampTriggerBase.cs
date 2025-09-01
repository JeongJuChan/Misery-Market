using UnityEngine;

public abstract class BaseCampTriggerBase : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Toggle();
    }

    protected virtual void Toggle()
    {
        // 아이템 로직
        
    }
}
