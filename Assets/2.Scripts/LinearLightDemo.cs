using UnityEngine;
using UnityEngine.UI;
using MiseryMarket.Rendering;

namespace MiseryMarket.Demo
{
    /// <summary>
    /// Linear Light 데모 스크립트
    /// Unity 6에서 클립 스튜디오 Linear Light 기능의 사용법을 보여줍니다
    /// </summary>
    public class LinearLightDemo : MonoBehaviour
    {
        [Header("Demo Components")]
        [SerializeField] private LinearLightController linearLightController;
        [SerializeField] private Slider opacitySlider;
        [SerializeField] private Slider brightnessSlider;
        [SerializeField] private Slider contrastSlider;
        [SerializeField] private Dropdown blendModeDropdown;
        [SerializeField] private Button[] presetButtons;
        [SerializeField] private Text infoText;

        [Header("Color Presets")]
        [SerializeField] private Color[] colorPresets = {
            new Color(1f, 0.8f, 0.6f, 0.5f), // 따뜻한 색
            new Color(0.6f, 0.8f, 1f, 0.5f), // 차가운 색
            new Color(1f, 1f, 1f, 0.5f),     // 중성색
            new Color(1f, 0.6f, 0.6f, 0.5f), // 빨강
            new Color(0.6f, 1f, 0.6f, 0.5f), // 초록
            new Color(0.6f, 0.6f, 1f, 0.5f)  // 파랑
        };

        private void Start()
        {
            InitializeUI();
            SetupEventListeners();
            UpdateInfoText();
        }

        private void InitializeUI()
        {
            if (linearLightController == null)
            {
                linearLightController = FindFirstObjectByType<LinearLightController>();
            }

            // 슬라이더 초기값 설정
            if (opacitySlider != null)
            {
                opacitySlider.value = 0.5f;
            }

            if (brightnessSlider != null)
            {
                brightnessSlider.minValue = -1f;
                brightnessSlider.maxValue = 1f;
                brightnessSlider.value = 0f;
            }

            if (contrastSlider != null)
            {
                contrastSlider.minValue = 0.1f;
                contrastSlider.maxValue = 3f;
                contrastSlider.value = 1f;
            }

            // 드롭다운 설정
            if (blendModeDropdown != null)
            {
                blendModeDropdown.options.Clear();
                blendModeDropdown.options.Add(new Dropdown.OptionData("Linear Light"));
                blendModeDropdown.options.Add(new Dropdown.OptionData("Linear Dodge"));
                blendModeDropdown.options.Add(new Dropdown.OptionData("Multiply"));
                blendModeDropdown.value = 0;
            }
        }

        private void SetupEventListeners()
        {
            // 슬라이더 이벤트
            if (opacitySlider != null)
            {
                opacitySlider.onValueChanged.AddListener(OnOpacityChanged);
            }

            if (brightnessSlider != null)
            {
                brightnessSlider.onValueChanged.AddListener(OnBrightnessChanged);
            }

            if (contrastSlider != null)
            {
                contrastSlider.onValueChanged.AddListener(OnContrastChanged);
            }

            if (blendModeDropdown != null)
            {
                blendModeDropdown.onValueChanged.AddListener(OnBlendModeChanged);
            }

            // 프리셋 버튼 이벤트
            if (presetButtons != null && presetButtons.Length >= 4)
            {
                presetButtons[0].onClick.AddListener(() => ApplyPreset("ClipStudioDefault"));
                presetButtons[1].onClick.AddListener(() => ApplyPreset("SoftLight"));
                presetButtons[2].onClick.AddListener(() => ApplyPreset("HardLight"));
                presetButtons[3].onClick.AddListener(() => ApplyPreset("Reset"));
            }
        }

        #region UI Event Handlers

        private void OnOpacityChanged(float value)
        {
            if (linearLightController != null)
            {
                linearLightController.SetOpacity(value);
                UpdateInfoText();
            }
        }

        private void OnBrightnessChanged(float value)
        {
            if (linearLightController != null)
            {
                linearLightController.SetBrightness(value);
                UpdateInfoText();
            }
        }

        private void OnContrastChanged(float value)
        {
            if (linearLightController != null)
            {
                linearLightController.SetContrast(value);
                UpdateInfoText();
            }
        }

        private void OnBlendModeChanged(int index)
        {
            if (linearLightController != null)
            {
                linearLightController.SetBlendMode((MiseryMarket.Rendering.LinearLightBlendMode)index);
                UpdateInfoText();
            }
        }

        private void ApplyPreset(string presetName)
        {
            if (linearLightController == null) return;

            switch (presetName)
            {
                case "ClipStudioDefault":
                    linearLightController.ApplyClipStudioDefault();
                    break;
                case "SoftLight":
                    linearLightController.ApplySoftLightEffect();
                    break;
                case "HardLight":
                    linearLightController.ApplyHardLightEffect();
                    break;
                case "Reset":
                    ResetToDefaults();
                    break;
            }

            SyncUIWithController();
            UpdateInfoText();
        }

        #endregion

        #region Utility Methods

        private void ResetToDefaults()
        {
            if (linearLightController != null)
            {
                linearLightController.SetOverlayColor(new Color(1f, 1f, 1f, 0.2f));
                linearLightController.SetOpacity(1f);
                linearLightController.SetBlendMode(MiseryMarket.Rendering.LinearLightBlendMode.LinearLight);
                linearLightController.SetGamma(2.2f);
                linearLightController.SetBrightness(0f);
                linearLightController.SetContrast(1f);
            }
        }

        private void SyncUIWithController()
        {
            // UI 요소들을 컨트롤러의 현재 값으로 동기화
            // 실제 구현에서는 LinearLightController에 getter 메소드를 추가해야 함
        }

        private void UpdateInfoText()
        {
            if (infoText == null) return;

            string info = "Linear Light Effect Status:\n";
            info += $"Opacity: {(opacitySlider?.value ?? 0.5f):F2}\n";
            info += $"Brightness: {(brightnessSlider?.value ?? 0f):F2}\n";
            info += $"Contrast: {(contrastSlider?.value ?? 1f):F2}\n";
            info += $"Blend Mode: {(MiseryMarket.Rendering.LinearLightBlendMode)(blendModeDropdown?.value ?? 0)}";

            infoText.text = info;
        }

        #endregion

        #region Color Cycle Demo

        [Header("Auto Color Cycling")]
        [SerializeField] private bool enableColorCycling = false;
        [SerializeField] private float colorCycleSpeed = 2f;
        private int currentColorIndex = 0;
        private float colorTimer = 0f;

        private void Update()
        {
            if (enableColorCycling && linearLightController != null)
            {
                colorTimer += Time.deltaTime * colorCycleSpeed;
                
                if (colorTimer >= 1f)
                {
                    colorTimer = 0f;
                    currentColorIndex = (currentColorIndex + 1) % colorPresets.Length;
                    linearLightController.SetOverlayColor(colorPresets[currentColorIndex]);
                }
            }
        }

        #endregion

        #region Public Methods (외부에서 호출 가능)

        /// <summary>
        /// 특정 색상으로 즉시 변경
        /// </summary>
        public void SetColor(int colorIndex)
        {
            if (colorIndex >= 0 && colorIndex < colorPresets.Length && linearLightController != null)
            {
                linearLightController.SetOverlayColor(colorPresets[colorIndex]);
                currentColorIndex = colorIndex;
            }
        }

        /// <summary>
        /// 자동 색상 순환 토글
        /// </summary>
        public void ToggleColorCycling()
        {
            enableColorCycling = !enableColorCycling;
        }

        /// <summary>
        /// Linear Light 효과 토글
        /// </summary>
        public void ToggleLinearLightEffect()
        {
            if (linearLightController != null)
            {
                linearLightController.SetLinearLightEnabled(!linearLightController.gameObject.activeInHierarchy);
            }
        }

        #endregion

        #region Debug Methods

        [ContextMenu("Test All Presets")]
        public void TestAllPresets()
        {
            StartCoroutine(TestPresetsCoroutine());
        }

        private System.Collections.IEnumerator TestPresetsCoroutine()
        {
            string[] presets = { "ClipStudioDefault", "SoftLight", "HardLight", "Reset" };
            
            foreach (string preset in presets)
            {
                Debug.Log($"Testing preset: {preset}");
                ApplyPreset(preset);
                yield return new WaitForSeconds(2f);
            }
            
            Debug.Log("Preset test completed!");
        }

        #endregion
    }
}
