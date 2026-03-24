namespace OpenPixel2D.Rendering.Abstractions;

public interface IRenderCompletedPass
{
    RenderPassDescriptor Descriptor { get; }

    IReadOnlyList<IRenderCommand> Commands { get; }
}
