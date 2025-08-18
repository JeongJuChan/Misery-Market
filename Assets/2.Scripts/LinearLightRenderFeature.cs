using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

namespace MiseryMarket.Rendering
{
    /// <summary>
    /// Unity 6 URP용 Linear Light 렌더 피처 (Render Graph API 사용)
    /// 전체 화면에 Linear Light 효과를 적용할 수 있습니다
    /// </summary>
    public class LinearLightRenderFeature : ScriptableRendererFeature
    {
        [System.Serializable]
        public class LinearLightSettings
        {
            [Header("Effect Settings")]
            public Color overlayColor = new Color(1f, 1f, 1f, 0.2f);
            [Range(0f, 1f)]
            public float intensity = 0.5f;
            public LinearLightBlendMode blendMode = LinearLightBlendMode.LinearLight;
            
            [Header("Quality Settings")]
            public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
            [Range(0.1f, 3f)]
            public float gamma = 2.2f;
            [Range(-1f, 1f)]
            public float brightness = 0f;
            [Range(0.1f, 3f)]
            public float contrast = 1f;
            
            [Header("Performance")]
            public bool useHalfResolution = false;
        }

        public LinearLightSettings settings = new LinearLightSettings();
        private LinearLightRenderPass renderPass;

        public override void Create()
        {
            renderPass = new LinearLightRenderPass(settings);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.cameraType != CameraType.Game)
                return;

            renderer.EnqueuePass(renderPass);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                renderPass?.Dispose();
            }
        }
    }

    /// <summary>
    /// Linear Light 렌더 패스 구현 (Render Graph API 사용)
    /// </summary>
    public class LinearLightRenderPass : ScriptableRenderPass
    {
        private LinearLightRenderFeature.LinearLightSettings settings;
        private Material linearLightMaterial;
        
        // 쉐이더 프로퍼티 IDs
        private static readonly int OverlayColorID = Shader.PropertyToID("_OverlayColor");
        private static readonly int OpacityID = Shader.PropertyToID("_Opacity");
        private static readonly int BlendModeID = Shader.PropertyToID("_BlendMode");
        private static readonly int GammaID = Shader.PropertyToID("_Gamma");
        private static readonly int BrightnessID = Shader.PropertyToID("_Brightness");
        private static readonly int ContrastID = Shader.PropertyToID("_Contrast");

        private class PassData
        {
            public Material material;
            public LinearLightRenderFeature.LinearLightSettings settings;
            public TextureHandle cameraColorTexture;
            public TextureHandle destinationTexture;
        }

        public LinearLightRenderPass(LinearLightRenderFeature.LinearLightSettings settings)
        {
            this.settings = settings;
            this.renderPassEvent = settings.renderPassEvent;
            
            // 머티리얼 생성
            Shader shader = Shader.Find("UI/LinearLight");
            if (shader != null)
            {
                linearLightMaterial = new Material(shader);
            }
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (linearLightMaterial == null)
                return;

            string passName = "Linear Light Pass";
            using (var builder = renderGraph.AddRasterRenderPass<PassData>(passName, out var passData))
            {
                // 카메라 데이터 가져오기
                UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
                UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

                // 입력 텍스처 설정
                passData.cameraColorTexture = resourceData.activeColorTexture;
                builder.UseTexture(passData.cameraColorTexture, AccessFlags.Read);

                // 출력 텍스처 설정
                RenderTextureDescriptor descriptor = cameraData.cameraTargetDescriptor;
                descriptor.msaaSamples = 1;
                descriptor.depthBufferBits = 0;
                
                if (settings.useHalfResolution)
                {
                    descriptor.width /= 2;
                    descriptor.height /= 2;
                }

                passData.destinationTexture = UniversalRenderer.CreateRenderGraphTexture(
                    renderGraph, descriptor, "_LinearLightRT", false);
                builder.SetRenderAttachment(passData.destinationTexture, 0, AccessFlags.Write);

                // Pass 데이터 설정
                passData.material = linearLightMaterial;
                passData.settings = settings;

                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                    ExecutePass(data, context);
                });
            }
        }

        private static void ExecutePass(PassData data, RasterGraphContext context)
        {
            // 머티리얼 프로퍼티 설정
            data.material.SetColor(OverlayColorID, data.settings.overlayColor);
            data.material.SetFloat(OpacityID, data.settings.intensity);
            data.material.SetFloat(BlendModeID, (float)data.settings.blendMode);
            data.material.SetFloat(GammaID, data.settings.gamma);
            data.material.SetFloat(BrightnessID, data.settings.brightness);
            data.material.SetFloat(ContrastID, data.settings.contrast);

            // Blit 실행
            Blitter.BlitTexture(context.cmd, data.cameraColorTexture, Vector4.one, data.material, 0);
        }

        public void Dispose()
        {
            if (linearLightMaterial != null)
            {
                if (Application.isPlaying)
                    Object.Destroy(linearLightMaterial);
                else
                    Object.DestroyImmediate(linearLightMaterial);
            }
        }
    }

    /// <summary>
    /// Linear Light Volume 컴포넌트 (Unity 6 Volume 시스템 호환)
    /// </summary>
    [System.Serializable, VolumeComponentMenu("Post-processing/Linear Light")]
    public class LinearLightVolume : VolumeComponent, IPostProcessComponent
    {
        [Header("Linear Light Settings")]
        public ColorParameter overlayColor = new ColorParameter(new Color(1f, 1f, 1f, 0.2f));
        public ClampedFloatParameter intensity = new ClampedFloatParameter(0.5f, 0f, 1f);
        public IntParameter blendMode = new IntParameter(0); // 0=LinearLight, 1=LinearDodge, 2=Multiply
        
        [Header("Color Correction")]
        public ClampedFloatParameter gamma = new ClampedFloatParameter(2.2f, 0.1f, 3f);
        public ClampedFloatParameter brightness = new ClampedFloatParameter(0f, -1f, 1f);
        public ClampedFloatParameter contrast = new ClampedFloatParameter(1f, 0.1f, 3f);

        public bool IsActive() => intensity.value > 0f;

        public bool IsTileCompatible() => true;
    }

    /// <summary>
    /// Linear Light 포스트 프로세싱 렌더러 (Unity 6 호환)
    /// </summary>
    public class LinearLightRenderer : ScriptableRendererFeature
    {
        private class LinearLightPostProcessPass : ScriptableRenderPass
        {
            private Material linearLightMaterial;
            
            private class PostProcessPassData
            {
                public Material material;
                public TextureHandle cameraColorTexture;
                public TextureHandle destinationTexture;
                public LinearLightVolume volumeComponent;
            }

            public LinearLightPostProcessPass()
            {
                renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
                
                Shader shader = Shader.Find("UI/LinearLight");
                if (shader != null)
                {
                    linearLightMaterial = new Material(shader);
                }
            }

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                if (linearLightMaterial == null) return;

                var stack = VolumeManager.instance.stack;
                var linearLight = stack.GetComponent<LinearLightVolume>();
                if (linearLight == null || !linearLight.IsActive()) return;

                string passName = "Linear Light Post Process";
                using (var builder = renderGraph.AddRasterRenderPass<PostProcessPassData>(passName, out var passData))
                {
                    // 카메라 데이터 가져오기
                    UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
                    UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

                    // 입력/출력 텍스처 설정
                    passData.cameraColorTexture = resourceData.activeColorTexture;
                    builder.UseTexture(passData.cameraColorTexture, AccessFlags.Read);

                    RenderTextureDescriptor descriptor = cameraData.cameraTargetDescriptor;
                    descriptor.msaaSamples = 1;
                    descriptor.depthBufferBits = 0;

                    passData.destinationTexture = UniversalRenderer.CreateRenderGraphTexture(
                        renderGraph, descriptor, "_LinearLightPostProcessRT", false);
                    builder.SetRenderAttachment(passData.destinationTexture, 0, AccessFlags.Write);

                    // Pass 데이터 설정
                    passData.material = linearLightMaterial;
                    passData.volumeComponent = linearLight;

                    builder.SetRenderFunc((PostProcessPassData data, RasterGraphContext context) =>
                    {
                        ExecutePostProcess(data, context);
                    });
                }
            }

            private static void ExecutePostProcess(PostProcessPassData data, RasterGraphContext context)
            {
                // Volume 설정을 머티리얼에 적용
                data.material.SetColor("_LinearLightColor", data.volumeComponent.overlayColor.value);
                data.material.SetFloat("_LinearLightIntensity", data.volumeComponent.intensity.value);
                data.material.SetFloat("_Opacity", data.volumeComponent.intensity.value);

                // Blit 실행
                Blitter.BlitTexture(context.cmd, data.cameraColorTexture, Vector4.one, data.material, 0);
            }

            public void Dispose()
            {
                if (linearLightMaterial != null)
                {
                    if (Application.isPlaying)
                        Object.Destroy(linearLightMaterial);
                    else
                        Object.DestroyImmediate(linearLightMaterial);
                }
            }
        }

        private LinearLightPostProcessPass postProcessPass;

        public override void Create()
        {
            postProcessPass = new LinearLightPostProcessPass();
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(postProcessPass);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                postProcessPass?.Dispose();
            }
        }
    }
}

/// <summary>
/// Linear Light 블렌드 모드 열거형
/// </summary>
public enum LinearLightBlendMode
{
    LinearLight = 0,
    LinearDodge = 1,
    Multiply = 2
}
