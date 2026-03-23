using OpenPixel2D.Rendering.Abstractions;

namespace OpenPixel2D.Rendering;

public static class RenderPassRegistryExtensions
{
    public static IRenderPassRegistry RegisterDefaultPasses(this IRenderPassRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);

        registry.Register(new RenderPassDescriptor(RenderPassNames.WorldSprites, 0, CreateState(SamplerMode.PointClamp)));
        registry.Register(new RenderPassDescriptor(RenderPassNames.UI, 100, CreateState(SamplerMode.LinearClamp)));
        registry.Register(new RenderPassDescriptor(RenderPassNames.Debug, 200, CreateState(SamplerMode.PointClamp)));

        return registry;
    }

    private static RenderState CreateState(SamplerMode samplerMode)
    {
        return new RenderState(
            BlendMode: BlendMode.AlphaBlend,
            SamplerMode: samplerMode,
            DepthMode: DepthMode.None);
    }
}
