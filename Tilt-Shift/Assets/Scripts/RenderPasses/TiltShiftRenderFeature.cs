using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class TiltShiftRenderFeature : ScriptableRendererFeature
{

    [System.Serializable]
    public class HLSettings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingSkybox;
        public Material mMat;
        public string textureId = "_ScreenTexture";

        public bool Preview = false;

        [Range(-1f, 1f)]
        public float Offset = 0f;

        [Range(0f, 20f)]
        public float Area = 1f;

        [Range(0f, 20f)]
        public float Spread = 1f;

        [Range(8, 64)]
        public int Samples = 32;

        [Range(0f, 2f)]
        public float Radius = 1f;

        public bool UseDistortion = true;

        [Range(0f, 20f)]
        public float CubicDistortion = 5f;
        [Range(0.01f, 2f)]
        public float DistortionScale = 1f;
    }

    public HLSettings settings = new HLSettings();

    RenderTargetHandle m_renderTargetHandle;

    TiltShiftRenderPass m_ScriptablePass;

    public override void Create()
    {
        m_ScriptablePass = new TiltShiftRenderPass("HLPostEffectRender", settings.renderPassEvent, settings.mMat, settings.Offset, settings.Area,
        settings.Spread, settings.Samples, settings.Radius, settings.UseDistortion, settings.CubicDistortion, settings.DistortionScale);
        m_renderTargetHandle.Init(settings.textureId);
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        var src = renderer.cameraColorTarget;
        var dest = RenderTargetHandle.CameraTarget;
        if (settings.mMat == null)
        {
            Debug.LogWarningFormat("丢失blit材质");
            return;
        }
        m_ScriptablePass.Setup(src, dest);
        renderer.EnqueuePass(m_ScriptablePass);
    }
}
