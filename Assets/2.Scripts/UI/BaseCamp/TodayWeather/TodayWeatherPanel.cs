using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class TodayWeatherPanel : UIBase
{
    [SerializeField] private TodayInfoPanel[] todayInfoPanels;
    [SerializeField] private GameObject backgroundObject;

    void Awake()
    {
        foreach (var todayInfoPanel in todayInfoPanels)
        {
            todayInfoPanel.UpdateActiveState(false);
        }
    }

    public void SetWeatherText(string[] texts)
    {
        for (int i = 0; i < texts.Length; i++)
        {
            TodayInfoPanel todayInfoPanel = todayInfoPanels[i];
            todayInfoPanel.SetWeatherText(texts[i]);
            todayInfoPanel.UpdateActiveState(true);
        }

        for (int i = texts.Length; i < todayInfoPanels.Length; i++)
        {
            TodayInfoPanel todayInfoPanel = todayInfoPanels[i];
            todayInfoPanel.UpdateActiveState(false);
        }
    }

    public override async UniTask ShowAsync(CancellationToken externalCt = default)
    {
        backgroundObject.SetActive(true);
        await base.ShowAsync(externalCt);
    }

    public override async UniTask HideAsync(CancellationToken externalCt = default)
    {
        await base.HideAsync(externalCt);
        backgroundObject.SetActive(false);
    }
}
