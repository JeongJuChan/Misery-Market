using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

/// <summary>
/// 매니저들의 초기화 순서를 관리하는 부트스트래퍼
/// </summary>
public class ManagerBootstrapper : MonoBehaviour
{
    [Header("Manager Initialization Order")]
    [SerializeField] private bool autoInitialize = true;
    [SerializeField] private float initializationTimeout = 10f;
    
    // 초기화 순서 정의
    private readonly System.Type[] managerInitOrder = new System.Type[]
    {
        typeof(LogManager),          // 1. 로그 매니저 (모든 매니저 의존)
        typeof(CoroutineManager),    // 2. 코루틴 매니저 (다른 매니저들이 사용)
        typeof(ResourceManager),     // 3. 리소스 매니저 (데이터 로드)
        typeof(LocalizationManager), // 4. 로컬라이제이션 매니저 (리소스 의존)
        typeof(SoundManager),        // 5. 사운드 매니저 (리소스 의존)
        typeof(DataManager),         // 6. 데이터 매니저 (로컬라이제이션 의존)
        typeof(PerformanceMonitor), // 7. 성능 모니터 (게임 전반 모니터링)
        typeof(SceneManager),       // 8. 씬 매니저 (씬 전환 관리)
    };

    public static ManagerBootstrapper Instance { get; private set; }
    
    [Header("Initialization Status")]
    [SerializeField] private List<string> initializedManagers = new List<string>();
    [SerializeField] private bool isInitializationComplete = false;
    
    public System.Action OnAllManagersInitialized;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        if (autoInitialize)
        {
            StartCoroutine(InitializeManagersCoroutine());
        }
    }

    public System.Collections.IEnumerator InitializeManagersCoroutine()
    {
        Debug.Log("[ManagerBootstrapper] Starting manager initialization...");
        
        float startTime = Time.time;
        
        foreach (var managerType in managerInitOrder)
        {
            yield return StartCoroutine(InitializeManager(managerType));
            
            // 타임아웃 체크
            if (Time.time - startTime > initializationTimeout)
            {
                Debug.LogError($"[ManagerBootstrapper] Initialization timeout after {initializationTimeout} seconds");
                break;
            }
        }
        
        isInitializationComplete = true;
        OnAllManagersInitialized?.Invoke();
        
        Debug.Log($"[ManagerBootstrapper] All managers initialized in {Time.time - startTime:F2} seconds");
    }

    private System.Collections.IEnumerator InitializeManager(System.Type managerType)
    {
        Debug.Log($"[ManagerBootstrapper] Initializing {managerType.Name}...");
        
        try
        {
            // 싱글톤 Instance 프로퍼티 호출로 생성 및 초기화
            var instanceProperty = managerType.GetProperty("Instance");
            if (instanceProperty != null)
            {
                var instance = instanceProperty.GetValue(null);
                if (instance != null)
                {
                    initializedManagers.Add(managerType.Name);
                    Debug.Log($"[ManagerBootstrapper] {managerType.Name} initialized successfully");
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ManagerBootstrapper] Failed to initialize {managerType.Name}: {ex.Message}");
        }
        
        yield return null; // 한 프레임 대기
    }

    public bool IsManagerInitialized<T>() where T : MonoBehaviour
    {
        return initializedManagers.Contains(typeof(T).Name);
    }

    public bool IsInitializationComplete()
    {
        return isInitializationComplete;
    }
}
