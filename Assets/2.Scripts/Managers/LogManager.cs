using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 게임 전반의 로깅 및 에러 처리를 담당하는 매니저
/// </summary>
public class LogManager : MonoBehaviour
{
    public static LogManager Instance { get; private set; }
    
    public enum LogLevel
    {
        Debug = 0,
        Info = 1,
        Warning = 2,
        Error = 3,
        Fatal = 4
    }
    
    [Header("Log Settings")]
    [SerializeField] private LogLevel minLogLevel = LogLevel.Debug;
    [SerializeField] private bool enableFileLogging = false;
    [SerializeField] private int maxLogEntries = 1000;
    
    private Queue<string> logHistory = new Queue<string>();
    
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Unity 로그 캐치
        Application.logMessageReceived += OnUnityLogReceived;
    }
    
    private void OnDestroy()
    {
        Application.logMessageReceived -= OnUnityLogReceived;
    }
    
    public void Log(LogLevel level, string message, Object context = null)
    {
        if (level < minLogLevel) return;
        
        string formattedMessage = $"[{System.DateTime.Now:HH:mm:ss.fff}] [{level}] {message}";
        
        // Unity 로그로 출력
        switch (level)
        {
            case LogLevel.Debug:
            case LogLevel.Info:
                Debug.Log(formattedMessage, context);
                break;
            case LogLevel.Warning:
                Debug.LogWarning(formattedMessage, context);
                break;
            case LogLevel.Error:
            case LogLevel.Fatal:
                Debug.LogError(formattedMessage, context);
                break;
        }
        
        // 히스토리 저장
        AddToHistory(formattedMessage);
    }
    
    private void AddToHistory(string message)
    {
        logHistory.Enqueue(message);
        
        if (logHistory.Count > maxLogEntries)
        {
            logHistory.Dequeue();
        }
    }
    
    private void OnUnityLogReceived(string logString, string stackTrace, LogType type)
    {
        // Unity 로그를 우리 시스템으로 라우팅
        LogLevel level = type switch
        {
            LogType.Error => LogLevel.Error,
            LogType.Exception => LogLevel.Fatal,
            LogType.Warning => LogLevel.Warning,
            LogType.Log => LogLevel.Info,
            _ => LogLevel.Debug
        };
        
        AddToHistory($"[{System.DateTime.Now:HH:mm:ss.fff}] [UNITY-{level}] {logString}");
    }
    
    public List<string> GetRecentLogs(int count = 50)
    {
        var logs = new List<string>();
        var array = logHistory.ToArray();
        
        int startIndex = Mathf.Max(0, array.Length - count);
        for (int i = startIndex; i < array.Length; i++)
        {
            logs.Add(array[i]);
        }
        
        return logs;
    }
}
