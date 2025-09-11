using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("@UIManager");
                UIManager uiManager = go.AddComponent<UIManager>();
                instance = uiManager;
            }

            return instance;
        }
    }
    private static UIManager instance;

    [Header("Global Canvas Root (required)")]
    [SerializeField] private Transform uiRoot; // 상주 Canvas의 적절한 자식(예: Panels)

    [SerializeField] private List<UIBase> panels = new();
    private readonly Dictionary<UIId, UIBase> _map = new();
    private readonly Dictionary<UIId, UIBase> _prefabMap = new();

    // 모달 1개 정책(선택): 새로 열 때 기존 모달들 닫기
    [SerializeField] private bool singleModal = true;
    [SerializeField] private UIId[] modalWhitelist; // 여기에 해당하는 것만 모달로 취급

    private Dictionary<UIId, Action> OnHideActionDict = new Dictionary<UIId, Action>();

    void Awake()
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

        foreach (var p in panels) if (p != null) _map[p.Id] = p;

    }

    // 패널 자가 등록/해제용
    public void Register(UIBase panel) {
        if (panel == null) return;
        _map[panel.Id] = panel;
        // 전역 루트로 강제 귀속(옵션)
        if (uiRoot != null && panel.transform.parent != uiRoot)
            panel.transform.SetParent(uiRoot, worldPositionStays: false);
    }
    public void Unregister(UIBase panel) {
        if (panel == null) return;
        if (_map.TryGetValue(panel.Id, out var cur) && cur == panel) _map.Remove(panel.Id);
    }

    // 없으면 프리팹에서 소환
    private UIBase Ensure(UIId id) {
        if (_map.TryGetValue(id, out var p) && p != null) return p;
        if (_prefabMap.TryGetValue(id, out var prefab) && prefab != null) {
            var inst = Instantiate(prefab, uiRoot);
            _map[id] = inst;
            return inst;
        }
        Debug.LogWarning($"[UIManager] Panel not found: {id}");
        return null;
    }

    public UniTask Show(UIId id, CancellationToken ct = default) {
        var p = Ensure(id);
        return p != null ? p.ShowAsync(ct) : UniTask.CompletedTask;
    }
    public UniTask Hide(UIId id, CancellationToken ct = default) {
        var p = Ensure(id);
        return p != null ? p.HideAsync(ct) : UniTask.CompletedTask;
    }

    
    public async UniTask ShowModal(UIId id, CancellationToken ct = default) {
        if (singleModal) {
            foreach (var kv in _map) {
                if (kv.Value.gameObject.activeSelf && IsModal(kv.Key) && kv.Key != id) {
                    await kv.Value.HideAsync(ct);
                }
            }
        }
        await Show(id, ct);
    }
    private bool IsModal(UIId id) {
        if (modalWhitelist == null || modalWhitelist.Length == 0) return true;
        foreach (var m in modalWhitelist) if (m == id) return true;
        return false;
    }

    public bool TryGet(UIId id, out UIBase panel) => _map.TryGetValue(id, out panel);

    public async UniTask Swap(UIId hideId, UIId showId, CancellationToken ct = default)
    {
        _map.TryGetValue(hideId, out var a);
        _map.TryGetValue(showId, out var b);
        if (a == null && b == null) return;

        using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var lct = linked.Token;

        // 동시 전환 (퇴장과 입장을 겹쳐 빠릿하게)
        var t1 = a != null ? a.HideAsync(lct) : UniTask.CompletedTask;
        var t2 = b != null ? b.ShowAsync(lct) : UniTask.CompletedTask;
        await UniTask.WhenAll(t1, t2);
    }
}
