using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class SceneManager
{
    public static SceneManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new SceneManager();
            }

            return instance;
        }
    }
    private static SceneManager instance;

    public bool IsBusy { get; private set; }

    public async UniTask LoadScene(SceneType sceneType)
    {
        if (IsBusy) return;

        IsBusy = true;
        await LoadSceneAsync(sceneType.ToString());
    }

    private async UniTask LoadSceneAsync(string sceneName)
    {
        var asyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
        while (!asyncOperation.isDone)
        {
            await UniTask.Yield();
        }

        IsBusy = false;
    }
}
