using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineManager : MonoBehaviour
{
    #region Singleton
    public static CoroutineManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("@CoroutineManager");
                CoroutineManager coroutineManager = go.AddComponent<CoroutineManager>();
                instance = coroutineManager;
            }

            return instance;
        }
    }
    private static CoroutineManager instance;
    #endregion

    private Dictionary<float, WaitForSeconds> waitForSecondsDict = new Dictionary<float, WaitForSeconds>();

    private Queue<CoroutineData> coroutineQueue = new Queue<CoroutineData>();
    private bool isRunning = false;

    private Coroutine currentCoroutine;

    #region Unity Methods
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    public void Enqueue(IEnumerator enumerator, Action OnCoroutineFinished = null)
    {
        coroutineQueue.Enqueue(new CoroutineData(enumerator, OnCoroutineFinished));
        TryRunNext();
    }

    private void TryRunNext()
    {
        if (!isRunning && coroutineQueue.Count > 0)
        {
            CoroutineData coroutineData = coroutineQueue.Dequeue();
            StartCoroutine(RunCoroutine(coroutineData.coroutine, coroutineData.onComplete));
        }
    }

    private IEnumerator RunCoroutine(IEnumerator coroutine, Action onCoroutineFinished)
    {
        isRunning = true;
        yield return StartCoroutine(coroutine);
        onCoroutineFinished?.Invoke();
        isRunning = false;
        TryRunNext();  // 다음 코루틴 실행
    }
    public WaitForSeconds GetWaitForSeconds(float duration)
    {
        if (!waitForSecondsDict.ContainsKey(duration))
        {
            WaitForSeconds wait = new WaitForSeconds(duration);
            waitForSecondsDict.Add(duration, wait);
        }

        return waitForSecondsDict[duration];
    }

    public int GetQueueCount()
    {
        return coroutineQueue.Count;
    }
}

public struct CoroutineData
{
    public IEnumerator coroutine;
    public Action onComplete;
    public CoroutineData(IEnumerator coroutine, Action onComplete = null)
    {
        this.coroutine = coroutine;
        this.onComplete = onComplete;
    }
}