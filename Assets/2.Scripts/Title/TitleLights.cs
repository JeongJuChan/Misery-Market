using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightsBlink : MonoBehaviour
{
    [Header("Targets")]
    [SerializeField] private Light2D[] lights;                 // 비워두면 자식에서 자동 수집

    [Header("Warm Color")]
    [SerializeField] private Color warmColor = new Color(1.0f, 0.86f, 0.62f); // 따뜻한 전구색
    [Range(0f, 1f)] [SerializeField] private float colorJitter = 0.05f;       // 각 전구마다 색 살짝 랜덤

    [Header("Base Intensity")]
    [SerializeField] private float minIntensity = 0.6f;
    [SerializeField] private float maxIntensity = 1.2f;

    [Header("Perlin Flicker")]
    [SerializeField] private Vector2 noiseSpeedRange = new Vector2(1.2f, 2.4f); // 전구별 속도 범위
    [SerializeField] private float desyncSeconds = 0.7f;                        // 전구별 시차(시드)

    [Header("Glitch (momentary off)")]
    [SerializeField] private float glitchChancePerSecond = 0.08f;               // 초당 확률(전구별)
    [SerializeField] private Vector2 glitchDurationRange = new Vector2(0.05f, 0.2f);

    [Header("Optional: Ripple (무대조명 느낌 추가 파형)")]
    [SerializeField] private bool addRipple = false;
    [SerializeField] private float rippleAmplitude = 0.15f;   // 강도(0~1 비율)
    [SerializeField] private float rippleSpeed = 1.5f;        // 속도
    [SerializeField] private float rippleSpacing = 0.6f;      // 전구 간 위상 간격

    [SerializeField] private float minSoundInterval = 0.12f;  // 소리 남발 방지(전역 쿨다운)

    struct State
    {
        public float seed;      // 전구별 시드(시간 오프셋)
        public float speed;     // 전구별 노이즈 속도
        public float glitchT;   // 남은 글리치 시간
        public Color baseColor; // 약간 다른 따뜻한 색
        public bool wasGlitching;  // 이전 프레임 glitch 상태
    }

    private State[] states;
    private float soundCooldown;   // 전역 쿨다운

    void OnEnable() {
        if (lights == null || lights.Length == 0)
            lights = GetComponentsInChildren<Light2D>(includeInactive: true);

        if (lights == null) return;

        states = new State[lights.Length];
        var rng = new System.Random(transform.GetInstanceID());

        for (int i = 0; i < lights.Length; i++) {
            if (lights[i] == null) continue;

            // 따뜻한 색 + 약간의 채도/명도 랜덤
            float j = (float)rng.NextDouble() * colorJitter;
            var tinted = warmColor;
            tinted.r = Mathf.Clamp01(tinted.r * (1f - j * 0.2f) + j * 0.05f);
            tinted.g = Mathf.Clamp01(tinted.g * (1f - j * 0.4f) + j * 0.02f);
            tinted.b = Mathf.Clamp01(tinted.b * (1f - j * 0.6f));

            states[i] = new State
            {
                seed = (float)rng.NextDouble() * desyncSeconds,
                speed = Mathf.Lerp(noiseSpeedRange.x, noiseSpeedRange.y, (float)rng.NextDouble()),
                glitchT = 0f,
                baseColor = tinted,
                wasGlitching = false
            };

            lights[i].color = tinted;
            lights[i].intensity = minIntensity;
            lights[i].enabled = true;
        }
    }

    void Update() {
        if (lights == null || states == null) return;

        float dt = Application.isPlaying ? Time.deltaTime : 1f/60f;
        if (soundCooldown > 0f) soundCooldown -= dt;

        for (int i = 0; i < lights.Length; i++)
        {
            var L = lights[i];
            if (L == null) continue;

            var st = states[i];

            // 글리치 타이머
            bool isGlitching = st.glitchT > 0f;
            if (isGlitching)
            {
                st.glitchT -= dt;
                L.enabled = false;
            }
            else
            {
                L.enabled = true;
            }

            // Perlin 기반 부드러운 깜빡임
            float t = Application.isPlaying ? Time.time : (float)UnityEditor.EditorApplication.timeSinceStartup;
            float p = Mathf.PerlinNoise((t + st.seed) * st.speed, i * 0.1234f);
            float baseIntensity = Mathf.Lerp(minIntensity, maxIntensity, p);

            // Ripple 옵션(행렬로 달린 전구가 물결치듯)
            if (addRipple)
            {
                float phase = i * rippleSpacing + t * rippleSpeed;
                float wave = Mathf.Sin(phase) * 0.5f + 0.5f;           // 0~1
                baseIntensity *= Mathf.Lerp(1f - rippleAmplitude, 1f, wave);
            }

            L.intensity = baseIntensity;

            // 확률적으로 순간 꺼짐(접촉불량 느낌)
            if (Application.isPlaying && glitchChancePerSecond > 0f)
            {
                float pGlitch = glitchChancePerSecond * dt;
                if (Random.value < pGlitch)
                {
                    st.glitchT = Random.Range(glitchDurationRange.x, glitchDurationRange.y);
                }
            }

            if (Application.isPlaying && soundCooldown <= 0f)
            {
                if (!st.wasGlitching && isGlitching)
                {
                    // glitch 시작 = OFF 소리
                    // SoundManager.Instance.PlaySFX(offClip, L.transform.position);
                    soundCooldown = minSoundInterval;
                }
                else if (st.wasGlitching && !isGlitching)
                {
                    // glitch 종료 = ON 소리
                    // SoundManager.Instance.PlaySFX(onClip, L.transform.position);
                    soundCooldown = minSoundInterval;
                }
            }

            st.wasGlitching = isGlitching;
            states[i] = st;
        }
    }

#if UNITY_EDITOR
    void OnValidate() {
        if (glitchDurationRange.x < 0f) glitchDurationRange.x = 0f;
        if (glitchDurationRange.y < glitchDurationRange.x) glitchDurationRange.y = glitchDurationRange.x;
        if (noiseSpeedRange.x < 0f) noiseSpeedRange.x = 0f;
        if (noiseSpeedRange.y < noiseSpeedRange.x) noiseSpeedRange.y = noiseSpeedRange.x;
        if (minIntensity < 0f) minIntensity = 0f;
        if (maxIntensity < minIntensity) maxIntensity = minIntensity;
    }
#endif
}
