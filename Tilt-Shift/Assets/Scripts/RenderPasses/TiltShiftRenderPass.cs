using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
public class TiltShiftRenderPass : ScriptableRenderPass
{
    public Material mMat;
    string m_ProfilerTag;
    private RenderTargetIdentifier source { get; set; }
    private RenderTargetHandle destination { get; set; }


    public bool Preview = false;
    public float Offset = 0f;

    public float Area = 1f;

    public float Spread = 1f;

    public int Samples = 32;

    public float Radius = 1f;
    public bool UseDistortion = true;

    public float CubicDistortion = 5f;
    public float DistortionScale = 1f;

    protected Vector4 m_GoldenRot = new Vector4();
    public FilterMode filterMode { get; set; }

    public TiltShiftRenderPass(string passname, RenderPassEvent _event, Material _mat, float offset, float area, float spread, int samples, float radius, bool usedistorition, float cubic, float dissacle)
    {
        m_ProfilerTag = passname;
        this.renderPassEvent = _event;
        mMat = _mat;

        // Precompute rotations
        float c = Mathf.Cos(2.39996323f);
        float s = Mathf.Sin(2.39996323f);
        m_GoldenRot.Set(c, s, -s, c);
        m_temporaryColorTexture.Init("temporaryColorTexture");

        Offset = offset;
        Area = area;
        Spread = spread;
        Samples = samples;
        Radius = radius;
        UseDistortion = usedistorition;
        CubicDistortion = cubic;
        DistortionScale = dissacle;
    }


    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);
        RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
        opaqueDesc.depthBufferBits = 0;
        
        int width = renderingData.cameraData.cameraTargetDescriptor.width;

        int height = renderingData.cameraData.cameraTargetDescriptor.height;

        if (UseDistortion)
			mMat.EnableKeyword("USE_DISTORTION");
		else
			mMat.DisableKeyword("USE_DISTORTION");

		mMat.SetVector("_GoldenRot", m_GoldenRot);
		mMat.SetVector("_Gradient", new Vector3(Offset, Area, Spread));
	    mMat.SetVector("_Distortion", new Vector2(CubicDistortion, DistortionScale));
		mMat.SetVector("_Params", new Vector4(Samples, Radius, 1f / width, 1f / height));

        cmd.GetTemporaryRT(m_temporaryColorTexture.id, opaqueDesc, filterMode);
        Blit(cmd, source, m_temporaryColorTexture.Identifier(), mMat, 1);
        Blit(cmd, m_temporaryColorTexture.Identifier(), source);
        
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    RenderTargetHandle m_temporaryColorTexture;

    public void Setup(RenderTargetIdentifier src, RenderTargetHandle dest)
    {
        this.source = src;
        this.destination = dest;
    }


    public override void FrameCleanup(CommandBuffer cmd)
    {
        if (destination == RenderTargetHandle.CameraTarget)
            cmd.ReleaseTemporaryRT(m_temporaryColorTexture.id);
    }
}
