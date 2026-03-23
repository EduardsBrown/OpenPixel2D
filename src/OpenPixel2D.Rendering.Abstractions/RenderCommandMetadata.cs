namespace OpenPixel2D.Rendering.Abstractions;

public readonly record struct RenderCommandMetadata(
    int Layer = 0,
    long SortKey = 0,
    RenderSpace Space = RenderSpace.World,
    RenderState? StateOverride = null);