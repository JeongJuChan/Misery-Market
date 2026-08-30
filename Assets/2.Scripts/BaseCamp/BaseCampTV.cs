using UnityEngine;

public class BaseCampTV : BaseCampTriggerBase
{
    protected override void Toggle()
    {
        // TV 로직
        base.Toggle();
        UIManager.Instance.Show(UIId.TodayWeatherPanel);
    }
}
