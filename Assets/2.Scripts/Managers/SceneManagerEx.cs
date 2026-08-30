using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class SceneManagerEx
{
    public static SceneManagerEx Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new SceneManagerEx();
            }

            return instance;
        }
    }
    private static SceneManagerEx instance;

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
