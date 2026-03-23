namespace OpenPixel2D.Rendering.Abstractions;

public interface IRenderPassWriter
{
    void Submit<TCommand>(in TCommand command)
        where TCommand : struct, IRenderCommand;
}