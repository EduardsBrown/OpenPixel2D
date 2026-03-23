namespace OpenPixel2D.Rendering.Abstractions;

public interface IRenderPassRegistry
{
    IReadOnlyList<RenderPassDescriptor> Passes { get; }

    void Register(RenderPassDescriptor descriptor);

    bool TryGetPass(string passName, out RenderPassDescriptor descriptor);
}
