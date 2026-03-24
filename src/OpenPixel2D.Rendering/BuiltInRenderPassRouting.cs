using OpenPixel2D.Rendering.Abstractions;

namespace OpenPixel2D.Rendering;

internal static class BuiltInRenderPassRouting
{
    public static BuiltInRenderPassRoute Sprite { get; } =
        new(RenderPassNames.WorldSprites, RenderSpace.World);

    public static BuiltInRenderPassRoute Text { get; } =
        new(RenderPassNames.UI, RenderSpace.Screen);
}

internal readonly record struct BuiltInRenderPassRoute(string PassName, RenderSpace Space);
