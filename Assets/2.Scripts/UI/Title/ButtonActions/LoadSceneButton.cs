using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class LoadSceneButton : MonoBehaviour, IButtonActionAsync
{
    [SerializeField] private SceneType sceneType;
    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(() => OnButtonClickedAsync(CancellationToken.None).Forget());
    }

    public async UniTask OnButtonClickedAsync(CancellationToken cancellationToken)
    {
        await SceneManagerEx.Instance.LoadScene(sceneType);
    }
}
