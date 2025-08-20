using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 게임 성능을 모니터링하는 매니저
/// </summary>
public class PerformanceMonitor : MonoBehaviour
{
    public static PerformanceMonitor Instance { get; private set; }
    
    [Header("Monitoring Settings")]
    [SerializeField] private bool enableMonitoring = true;
    [SerializeField] private float updateInterval = 1f;
    [SerializeField] private bool showOnScreenStats = false;
    
    [Header("Performance Metrics")]
    [SerializeField] private float currentFPS;
    [SerializeField] private float averageFPS;
    [SerializeField] private long memoryUsage;
    [SerializeField] private int drawCalls;
    
    private List<float> fpsHistory = new List<float>();
    private float lastUpdateTime;
    
    public System.Action<PerformanceData> OnPerformanceUpdate;
    
    public struct PerformanceData
    {
        public float fps;
        public float averageFps;
        public long memoryUsage;
        public int drawCalls;
        public float frameTime;
    }
    
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    private void Update()
    {
        if (!enableMonitoring) return;
        
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            UpdatePerformanceMetrics();
            lastUpdateTime = Time.time;
        }
    }
    
    private void UpdatePerformanceMetrics()
    {
        // FPS 계산
        currentFPS = 1f / Time.unscaledDeltaTime;
        fpsHistory.Add(currentFPS);
        
        if (fpsHistory.Count > 60) // 최근 60프레임만 유지
        {
            fpsHistory.RemoveAt(0);
        }
        
        // 평균 FPS 계산
        float sum = 0;
        for (int i = 0; i < fpsHistory.Count; i++)
        {
            sum += fpsHistory[i];
        }
        averageFPS = sum / fpsHistory.Count;
        
        // 메모리 사용량
        memoryUsage = System.GC.GetTotalMemory(false);
        
        // 이벤트 발생
        var perfData = new PerformanceData
        {
            fps = currentFPS,
            averageFps = averageFPS,
            memoryUsage = memoryUsage,
            drawCalls = drawCalls,
            frameTime = Time.unscaledDeltaTime * 1000f // ms
        };
        
        OnPerformanceUpdate?.Invoke(perfData);
    }
    
    private void OnGUI()
    {
        if (!showOnScreenStats) return;
        
        GUI.color = Color.white;
        GUILayout.BeginArea(new Rect(10, 10, 200, 100));
        GUILayout.Label($"FPS: {currentFPS:F1}");
        GUILayout.Label($"Avg FPS: {averageFPS:F1}");
        GUILayout.Label($"Memory: {memoryUsage / (1024 * 1024):F1} MB");
        GUILayout.Label($"Frame Time: {Time.unscaledDeltaTime * 1000f:F1} ms");
        GUILayout.EndArea();
    }
    
    public void LogPerformanceWarning(string context, float thresholdFPS = 30f)
    {
        if (currentFPS < thresholdFPS)
        {
            Debug.LogWarning($"[Performance] Low FPS detected in {context}: {currentFPS:F1} FPS");
        }
    }
}
