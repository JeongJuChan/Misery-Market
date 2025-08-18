using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 클립 스튜디오 스타일 Linear Light 효과를 UI 이미지에 적용하는 컴포넌트
/// Unity 6에서 UI Image에 Material로 적용하여 사용
/// </summary>
[RequireComponent(typeof(Image))]
public class LinearLightUI : MonoBehaviour
{
    [Header("Linear Light Settings")]
    [SerializeField] private Color linearLightColor = Color.white;
    [SerializeField] private float intensity = 1.0f;
    [SerializeField] private float opacity = 1.0f;
    
    private Image targetImage;
    private Material linearLightMaterial;
    
    // 셰이더 프로퍼티 ID들 (성능 최적화)
    private static readonly int LinearLightColorId = Shader.PropertyToID("_LinearLightColor");
    private static readonly int LinearLightIntensityId = Shader.PropertyToID("_LinearLightIntensity");
    private static readonly int OpacityId = Shader.PropertyToID("_Opacity");

    void Awake()
    {
        targetImage = GetComponent<Image>();
        
        // Linear Light 머티리얼 로드 (Resources 폴더나 Addressables 사용 권장)
        linearLightMaterial = Resources.Load<Material>("LinearLight_UI_Material");
        
        if (linearLightMaterial == null)
        {
            Debug.LogError("LinearLight_UI_Material을 찾을 수 없습니다! Assets/Resources/ 폴더에 배치해주세요.");
            return;
        }
        
        // 머티리얼 인스턴스 생성 (원본 보호)
        linearLightMaterial = new Material(linearLightMaterial);
        targetImage.material = linearLightMaterial;
    }

    void Start()
    {
        // 초기 설정 적용
        UpdateLinearLightSettings();
    }

    void OnValidate()
    {
        // 인스펙터에서 값 변경시 실시간 업데이트
        if (Application.isPlaying && linearLightMaterial != null)
        {
            UpdateLinearLightSettings();
        }
    }

    /// <summary>
    /// Linear Light 설정을 머티리얼에 적용
    /// </summary>
    private void UpdateLinearLightSettings()
    {
        if (linearLightMaterial == null) return;

        linearLightMaterial.SetColor(LinearLightColorId, linearLightColor);
        linearLightMaterial.SetFloat(LinearLightIntensityId, intensity);
        linearLightMaterial.SetFloat(OpacityId, opacity);
    }

    /// <summary>
    /// Linear Light 색상 설정
    /// </summary>
    /// <param name="color">적용할 Linear Light 색상</param>
    public void SetLinearLightColor(Color color)
    {
        linearLightColor = color;
        UpdateLinearLightSettings();
    }

    /// <summary>
    /// Linear Light 강도 설정
    /// </summary>
    /// <param name="newIntensity">강도 값 (0-2)</param>
    public void SetIntensity(float newIntensity)
    {
        intensity = Mathf.Clamp(newIntensity, 0f, 2f);
        UpdateLinearLightSettings();
    }

    /// <summary>
    /// 투명도 설정
    /// </summary>
    /// <param name="newOpacity">투명도 값 (0-1)</param>
    public void SetOpacity(float newOpacity)
    {
        opacity = Mathf.Clamp01(newOpacity);
        UpdateLinearLightSettings();
    }

    /// <summary>
    /// Linear Light 효과 애니메이션 (코루틴 사용)
    /// </summary>
    /// <param name="targetColor">목표 색상</param>
    /// <param name="targetIntensity">목표 강도</param>
    /// <param name="duration">애니메이션 시간</param>
    public void AnimateLinearLight(Color targetColor, float targetIntensity, float duration)
    {
        StartCoroutine(AnimateLinearLightCoroutine(targetColor, targetIntensity, duration));
    }

    private System.Collections.IEnumerator AnimateLinearLightCoroutine(Color targetColor, float targetIntensity, float duration)
    {
        Color startColor = linearLightColor;
        float startIntensity = intensity;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // 부드러운 보간
            t = Mathf.SmoothStep(0f, 1f, t);

            linearLightColor = Color.Lerp(startColor, targetColor, t);
            intensity = Mathf.Lerp(startIntensity, targetIntensity, t);

            UpdateLinearLightSettings();
            yield return null;
        }

        // 최종 값 설정
        linearLightColor = targetColor;
        intensity = targetIntensity;
        UpdateLinearLightSettings();
    }

    void OnDestroy()
    {
        // 메모리 누수 방지
        if (linearLightMaterial != null)
        {
            if (Application.isPlaying)
            {
                Destroy(linearLightMaterial);
            }
            else
            {
                DestroyImmediate(linearLightMaterial);
            }
        }
    }
}
