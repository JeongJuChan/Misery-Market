using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class NewGameButtonAction : MonoBehaviour, IButtonActionAsync
{
    public async UniTask OnButtonClickedAsync(CancellationToken cancellationToken)
    {
        await SceneManagerEx.Instance.LoadScene(SceneType.BaseCamp);
    }
}
