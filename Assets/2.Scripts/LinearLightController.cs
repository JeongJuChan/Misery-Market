using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;

namespace MiseryMarket.Rendering
{
    /// <summary>
    /// 클립 스튜디오 호환 Linear Light 블렌딩 컨트롤러
    /// Unity 6 URP에서 Linear Light, Linear Dodge, Linear Burn 등의 블렌딩 모드를 제공
    /// </summary>
    [System.Serializable]
    public enum LinearLightBlendMode
    {
        LinearLight = 0,    // 클립 스튜디오 호환 Linear Light
        LinearDodge = 1,    // Add 블렌딩과 유사
        Multiply = 2        // Multiply 블렌딩
    }

    [RequireComponent(typeof(Image))]
    public class LinearLightController : MonoBehaviour
    {
        [Header("Linear Light Settings")]
        [SerializeField] private Material linearLightMaterial;
        
        [Space(10)]
        [SerializeField] private Color overlayColor = new Color(1f, 1f, 1f, 0.2f);
        [Range(0f, 1f)]
        [SerializeField] private float opacity = 1f;
        [SerializeField] private LinearLightBlendMode blendMode = LinearLightBlendMode.LinearLight;
        
        [Header("Advanced Settings")]
        [Range(0.1f, 3f)]
        [SerializeField] private float gamma = 2.2f;
        [Range(-1f, 1f)]
        [SerializeField] private float brightness = 0f;
        [Range(0.1f, 3f)]
        [SerializeField] private float contrast = 1f;
        
        [Header("Animation Support")]
        [SerializeField] private bool enableAnimation = false;
        [SerializeField] private AnimationCurve opacityAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private float animationDuration = 1f;
        
        private Image targetImage;
        private Material materialInstance;
        private Coroutine animationCoroutine;
        
        // 쉐이더 프로퍼티 IDs (성능 최적화)
        private static readonly int OverlayColorID = Shader.PropertyToID("_OverlayColor");
        private static readonly int OpacityID = Shader.PropertyToID("_Opacity");
        private static readonly int BlendModeID = Shader.PropertyToID("_BlendMode");
        private static readonly int GammaID = Shader.PropertyToID("_Gamma");
        private static readonly int BrightnessID = Shader.PropertyToID("_Brightness");
        private static readonly int ContrastID = Shader.PropertyToID("_Contrast");

        private void Awake()
        {
            targetImage = GetComponent<Image>();
            InitializeMaterial();
        }

        private void Start()
        {
            UpdateShaderProperties();
            
            if (enableAnimation)
            {
                StartOpacityAnimation();
            }
        }

        private void OnValidate()
        {
            if (Application.isPlaying && materialInstance != null)
            {
                UpdateShaderProperties();
            }
        }

        /// <summary>
        /// 머티리얼 인스턴스 생성 및 초기화
        /// </summary>
        private void InitializeMaterial()
        {
            if (linearLightMaterial == null)
            {
                // 기본 쉐이더로 머티리얼 생성
                Shader linearLightShader = Shader.Find("UI/LinearLightOverlayColor_UI");
                if (linearLightShader != null)
                {
                    linearLightMaterial = new Material(linearLightShader);
                }
                else
                {
                    Debug.LogError("LinearLight Shader not found! Please make sure the shader is compiled correctly.");
                    return;
                }
            }

            // 머티리얼 인스턴스 생성 (런타임에서 수정 가능하도록)
            materialInstance = new Material(linearLightMaterial);
            targetImage.material = materialInstance;
        }

        /// <summary>
        /// 모든 쉐이더 프로퍼티 업데이트
        /// </summary>
        private void UpdateShaderProperties()
        {
            if (materialInstance == null) return;

            materialInstance.SetColor(OverlayColorID, overlayColor);
            materialInstance.SetFloat(OpacityID, opacity);
            materialInstance.SetFloat(BlendModeID, (float)blendMode);
            materialInstance.SetFloat(GammaID, gamma);
            materialInstance.SetFloat(BrightnessID, brightness);
            materialInstance.SetFloat(ContrastID, contrast);
        }

        #region Public Methods

        /// <summary>
        /// 오버레이 색상 설정
        /// </summary>
        public void SetOverlayColor(Color color)
        {
            overlayColor = color;
            if (materialInstance != null)
                materialInstance.SetColor(OverlayColorID, overlayColor);
        }

        /// <summary>
        /// 투명도 설정
        /// </summary>
        public void SetOpacity(float value)
        {
            opacity = Mathf.Clamp01(value);
            if (materialInstance != null)
                materialInstance.SetFloat(OpacityID, opacity);
        }

        /// <summary>
        /// 블렌딩 모드 설정
        /// </summary>
        public void SetBlendMode(LinearLightBlendMode mode)
        {
            blendMode = mode;
            if (materialInstance != null)
                materialInstance.SetFloat(BlendModeID, (float)blendMode);
        }

        /// <summary>
        /// 감마 값 설정 (색상 보정)
        /// </summary>
        public void SetGamma(float value)
        {
            gamma = Mathf.Clamp(value, 0.1f, 3f);
            if (materialInstance != null)
                materialInstance.SetFloat(GammaID, gamma);
        }

        /// <summary>
        /// 명도 조정
        /// </summary>
        public void SetBrightness(float value)
        {
            brightness = Mathf.Clamp(value, -1f, 1f);
            if (materialInstance != null)
                materialInstance.SetFloat(BrightnessID, brightness);
        }

        /// <summary>
        /// 대비 조정
        /// </summary>
        public void SetContrast(float value)
        {
            contrast = Mathf.Clamp(value, 0.1f, 3f);
            if (materialInstance != null)
                materialInstance.SetFloat(ContrastID, contrast);
        }

        /// <summary>
        /// Linear Light 효과 활성화/비활성화
        /// </summary>
        public void SetLinearLightEnabled(bool enabled)
        {
            gameObject.SetActive(enabled);
        }

        /// <summary>
        /// 투명도 애니메이션 시작
        /// </summary>
        public void StartOpacityAnimation()
        {
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }
            animationCoroutine = StartCoroutine(AnimateOpacity());
        }

        /// <summary>
        /// 애니메이션 정지
        /// </summary>
        public void StopOpacityAnimation()
        {
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
                animationCoroutine = null;
            }
        }

        #endregion

        #region Animation

        private System.Collections.IEnumerator AnimateOpacity()
        {
            float startTime = Time.time;
            float initialOpacity = opacity;

            while (enableAnimation)
            {
                float elapsed = (Time.time - startTime) % animationDuration;
                float normalizedTime = elapsed / animationDuration;
                
                float animatedOpacity = opacityAnimationCurve.Evaluate(normalizedTime);
                SetOpacity(initialOpacity * animatedOpacity);
                
                yield return null;
            }
        }

        #endregion

        #region Cleanup

        private void OnDestroy()
        {
            if (materialInstance != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(materialInstance);
                }
                else
                {
                    DestroyImmediate(materialInstance);
                }
            }
        }

        #endregion

        #region Presets

        /// <summary>
        /// 클립 스튜디오 기본 설정 적용
        /// </summary>
        [ContextMenu("Apply Clip Studio Default")]
        public void ApplyClipStudioDefault()
        {
            SetBlendMode(LinearLightBlendMode.LinearLight);
            SetGamma(2.2f);
            SetBrightness(0f);
            SetContrast(1f);
            SetOpacity(0.5f);
            SetOverlayColor(new Color(1f, 1f, 1f, 0.5f));
        }

        /// <summary>
        /// 소프트 라이트 효과 설정
        /// </summary>
        [ContextMenu("Apply Soft Light Effect")]
        public void ApplySoftLightEffect()
        {
            SetBlendMode(LinearLightBlendMode.LinearLight);
            SetGamma(1.8f);
            SetBrightness(0.1f);
            SetContrast(1.2f);
            SetOpacity(0.3f);
            SetOverlayColor(new Color(1f, 0.95f, 0.8f, 0.3f));
        }

        /// <summary>
        /// 하드 라이트 효과 설정
        /// </summary>
        [ContextMenu("Apply Hard Light Effect")]
        public void ApplyHardLightEffect()
        {
            SetBlendMode(LinearLightBlendMode.LinearLight);
            SetGamma(2.4f);
            SetBrightness(-0.1f);
            SetContrast(1.8f);
            SetOpacity(0.7f);
            SetOverlayColor(new Color(1f, 1f, 1f, 0.7f));
        }

        #endregion
    }
}
