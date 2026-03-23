namespace OpenPixel2D.Rendering.Abstractions;

public readonly record struct RenderState(
    RenderSortMode SortMode = RenderSortMode.Deferred,
    BlendMode BlendMode = BlendMode.AlphaBlend,
    SamplerMode SamplerMode = SamplerMode.LinearClamp,
    DepthMode DepthMode = DepthMode.None,
    RasterizerOptions Rasterizer = default);

public enum RenderSortMode
{
    Deferred = 0,
    Texture = 1,
    FrontToBack = 2,
    BackToFront = 3,
    Immediate = 4
}

public enum BlendMode
{
    Opaque = 0,
    AlphaBlend = 1,
    Additive = 2,
    NonPremultiplied = 3
}

public enum SamplerMode
{
    PointClamp = 0,
    PointWrap = 1,
    LinearClamp = 2,
    LinearWrap = 3
}

public enum DepthMode
{
    None = 0,
    Read = 1,
    ReadWrite = 2
}

public readonly record struct RasterizerOptions(
    CullMode CullMode = CullMode.None,
    bool ScissorTestEnabled = false);

public enum CullMode
{
    None = 0,
    Clockwise = 1,
    CounterClockwise = 2
}