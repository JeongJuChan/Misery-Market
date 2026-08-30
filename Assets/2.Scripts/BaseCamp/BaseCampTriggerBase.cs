using UnityEngine;
using UnityEngine.EventSystems;

public abstract class BaseCampTriggerBase : MonoBehaviour
{
    void OnMouseDown()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return; // UI 클릭 중이므로 3D 오브젝트 클릭 로직 스킵
        }
        Toggle();
    }

    protected virtual void Toggle()
    {
        // 아이템 로직
        
    }
}
