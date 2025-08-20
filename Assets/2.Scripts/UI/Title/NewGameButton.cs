using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class NewGameButton : MonoBehaviour
{
    [SerializeField] private Button startButton;

    private void Awake()
    {
        startButton.onClick.AddListener(() => OnStartButtonClicked().Forget());
    }

    private async UniTaskVoid OnStartButtonClicked()
    {
        await SceneManager.Instance.LoadScene(SceneType.BaseCamp);
    }
}
