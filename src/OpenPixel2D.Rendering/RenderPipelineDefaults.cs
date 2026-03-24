using OpenPixel2D.Rendering.Abstractions;

namespace OpenPixel2D.Rendering;

/// <summary>
/// Internal composition root for the current built-in rendering pipeline. Default processors remain
/// internal for now, but later public registration can layer on top of the same registry mechanics.
/// </summary>
internal static class RenderPipelineDefaults
{
    public static IRenderPassRegistry CreatePassRegistry()
    {
        RenderPassRegistry registry = new();
        registry.RegisterDefaultPasses();
        return registry;
    }

    public static IRenderAssetResolver CreateAssetResolver()
    {
        return new PassthroughRenderAssetResolver();
    }

    public static RenderProcessorRegistry CreateProcessorRegistry()
    {
        return CreateProcessorRegistry(CreateAssetResolver());
    }

    public static RenderProcessorRegistry CreateProcessorRegistry(IRenderAssetResolver assetResolver)
    {
        ArgumentNullException.ThrowIfNull(assetResolver);

        RenderProcessorRegistry registry = new();
        RegisterBuiltInProcessors(registry, assetResolver);
        return registry;
    }

    public static void RegisterBuiltInProcessors(RenderProcessorRegistry registry, IRenderAssetResolver assetResolver)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(assetResolver);

        registry.Register(new SpriteRenderItemProcessor(assetResolver));
        registry.Register(new TextRenderItemProcessor(assetResolver));
    }
}
