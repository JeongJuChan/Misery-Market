using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using PrimeTween;
using UnityEngine;

public class UIBase : MonoBehaviour
{
    [field: SerializeField] public UIId Id { get; private set; }
    [SerializeField] private UIAnimPreset preset;
    [SerializeField] private RectTransform rt;
    [SerializeField] private CanvasGroup cg;

    public UIState State { get; private set; } = UIState.Idle;

    private CancellationTokenSource cts;
    private Tween activeTween;
    private Sequence activeSequence;

    void Awake() {
        UIManager.Instance?.Register(this);
    }
    void OnDestroy()
    {
        UIManager.Instance?.Unregister(this);
        CancelCurrentTween();
    }

    void Reset() {
        rt ??= GetComponent<RectTransform>();
        cg ??= GetComponent<CanvasGroup>();
    }

    public async UniTask ShowAsync(CancellationToken externalCt = default) {
        if (State == UIState.Showing) return;
        
        CancelCurrentTween();
        cts = CancellationTokenSource.CreateLinkedTokenSource(externalCt);
        var ct = cts.Token;

        gameObject.SetActive(true);
        State = UIState.Showing;

        try {
            // 초기 상태 설정
            SetupInitialState();

            // 순차적으로 애니메이션 실행
            await PlayShowAnimation(ct);

            // 최종 상태 설정
            if (preset.useFade) {
                cg.blocksRaycasts = true;
                cg.interactable = true;
            }
        } catch (System.OperationCanceledException) {
            // 캔슬된 경우 처리
            Debug.Log($"[UIBase] Show animation cancelled for {gameObject.name}");
        } finally {
            State = UIState.Idle;
            ClearTweenReferences();
        }
    }

    public async UniTask HideAsync(CancellationToken externalCt = default) {
        if (State == UIState.Hiding || !gameObject.activeSelf) return;
        
        CancelCurrentTween();
        cts = CancellationTokenSource.CreateLinkedTokenSource(externalCt);
        var ct = cts.Token;

        State = UIState.Hiding;

        try {
            // 상호작용 비활성화
            if (preset.useFade) {
                cg.blocksRaycasts = false;
                cg.interactable = false;
            }

            // 순차적으로 애니메이션 실행
            await PlayHideAnimation(ct);

            gameObject.SetActive(false);
        } catch (System.OperationCanceledException) {
            // 캔슬된 경우 처리
            Debug.Log($"[UIBase] Hide animation cancelled for {gameObject.name}");
        } finally {
            State = UIState.Idle;
            ClearTweenReferences();
        }
    }

    private void SetupInitialState() {
        if (preset.useSlide) {
            rt.anchoredPosition = preset.slideFrom;
        }
        if (preset.useScale) {
            rt.localScale = preset.scaleFrom;
        }
        if (preset.useFade) {
            cg.alpha = 0f;
            cg.blocksRaycasts = false;
            cg.interactable = false;
        }
    }

    private async UniTask PlayShowAnimation(CancellationToken ct) {
        var sequence = Sequence.Create();

        if (preset.useFade) {
            sequence = sequence.Chain(Tween.Alpha(cg, 1f, preset.fadeInDuration, preset.easeIn));
        }
        if (preset.useSlide) {
            sequence = sequence.Group(Tween.UIAnchoredPosition(rt, preset.slideTo, preset.slideInDuration, preset.easeIn));
        }
        if (preset.useScale) {
            sequence = sequence.Group(Tween.Scale(rt, preset.scaleTo, preset.scaleInDuration, preset.easeIn));
        }

        activeSequence = sequence;
        await WaitForSequenceCompletion(sequence, ct);
    }

    private async UniTask PlayHideAnimation(CancellationToken ct) {
        var sequence = Sequence.Create();

        if (preset.useFade) {
            sequence = sequence.Chain(Tween.Alpha(cg, 0f, preset.fadeOutDuration, preset.easeOut));
        }
        if (preset.useSlide) {
            sequence = sequence.Group(Tween.UIAnchoredPosition(rt, preset.slideFrom, preset.slideOutDuration, preset.easeOut));
        }
        if (preset.useScale) {
            sequence = sequence.Group(Tween.Scale(rt, preset.scaleFrom, preset.scaleOutDuration, preset.easeOut));
        }

        activeSequence = sequence;
        await WaitForSequenceCompletion(sequence, ct);
    }

    private async UniTask WaitForTweenCompletion(Tween tween, CancellationToken ct) {
        while (tween.isAlive && !ct.IsCancellationRequested) {
            await UniTask.Yield();
        }
        
        if (ct.IsCancellationRequested) {
            throw new System.OperationCanceledException();
        }
    }

    private async UniTask WaitForSequenceCompletion(Sequence sequence, CancellationToken ct) {
        while (sequence.isAlive && !ct.IsCancellationRequested) {
            await UniTask.Yield();
        }
        
        if (ct.IsCancellationRequested) {
            throw new System.OperationCanceledException();
        }
    }

    public void InstantShowForLayout() {
        CancelCurrentTween();
        
        if (preset.useSlide) rt.anchoredPosition = preset.slideTo;
        if (preset.useScale) rt.localScale = preset.scaleTo;
        if (preset.useFade) { 
            cg.alpha = 1f; 
            cg.blocksRaycasts = true; 
            cg.interactable = true; 
        }
        
        gameObject.SetActive(true);
        State = UIState.Idle;
    }

    public void InstantHideForLayout() {
        CancelCurrentTween();
        
        if (preset.useSlide) rt.anchoredPosition = preset.slideFrom;
        if (preset.useScale) rt.localScale = preset.scaleFrom;
        if (preset.useFade) { 
            cg.alpha = 0f; 
            cg.blocksRaycasts = false; 
            cg.interactable = false; 
        }
        
        gameObject.SetActive(false);
        State = UIState.Idle;
    }

    private void CancelCurrentTween() {
        try {
            cts?.Cancel();
        } catch { /* 무시 */ }

        if (activeTween.isAlive) {
            activeTween.Stop();
        }
        
        if (activeSequence.isAlive) {
            activeSequence.Stop();
        }
        
        ClearTweenReferences();
    }

    private void ClearTweenReferences() {
        activeTween = default;
        activeSequence = default;
        cts?.Dispose();
        cts = null;
    }
}
