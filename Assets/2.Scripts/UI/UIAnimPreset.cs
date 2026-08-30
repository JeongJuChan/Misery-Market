using UnityEngine;
using PrimeTween;

/// <summary>
/// UI 애니메이션 프리셋을 정의하는 ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "UIAnimPreset", menuName = "UI/Animation Preset")]
public class UIAnimPreset : ScriptableObject
{
    [Header("Animation Settings")]
    
    [Header("Fade Animation")]
    public bool useFade = true;
    public float fadeInDuration = 0.3f;
    public float fadeOutDuration = 0.2f;
    
    [Header("Slide Animation")]
    public bool useSlide = false;
    public Vector2 slideFrom = Vector2.zero;
    public Vector2 slideTo = Vector2.zero;
    public float slideInDuration = 0.3f;
    public float slideOutDuration = 0.2f;
    
    [Header("Scale Animation")]
    public bool useScale = false;
    public Vector3 scaleFrom = Vector3.one;
    public Vector3 scaleTo = Vector3.one;
    public float scaleInDuration = 0.3f;
    public float scaleOutDuration = 0.2f;
    
    [Header("Easing")]
    public Ease easeIn = Ease.OutCubic;
    public Ease easeOut = Ease.InCubic;
    
    // 통합된 duration 프로퍼티들 (하위 호환성을 위해)
    public float inDuration => Mathf.Max(fadeInDuration, slideInDuration, scaleInDuration);
    public float outDuration => Mathf.Max(fadeOutDuration, slideOutDuration, scaleOutDuration);
}
