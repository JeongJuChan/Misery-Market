using UnityEngine;

public class BaseCampUIInitializer : MonoBehaviour
{
    [SerializeField] private TodayWeatherPanel todayWeatherPanel;
    [SerializeField] private RoadMapPanel roadMapPanel;
    [SerializeField] private BagUIPanel bagUIPanel;

    void Awake()
    {
        UIManager.Instance.Register(todayWeatherPanel);
        UIManager.Instance.Register(roadMapPanel);
        UIManager.Instance.Register(bagUIPanel);
    }
}
