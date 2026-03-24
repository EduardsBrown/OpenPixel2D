namespace OpenPixel2D.Rendering.Abstractions;

public interface IRenderFrameExecutor
{
    void Execute(IRenderCompletedFrame frame, IRenderView? view);
}
