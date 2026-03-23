namespace OpenPixel2D.Rendering.Abstractions;

public readonly record struct RenderPassDescriptor(
    string Name,
    int Order,
    RenderState State,
    RenderTargetId? Target = null,
    ClearOptions? Clear = null);