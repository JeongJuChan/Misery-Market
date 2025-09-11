using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class RoadMapPanel : UIBase
{
    [SerializeField] private GameObject backgroundObject;

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
