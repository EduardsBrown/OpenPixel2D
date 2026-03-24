namespace OpenPixel2D.Rendering.Abstractions;

public interface IRenderCompletedFrame
{
    IEnumerable<IRenderCompletedPass> GetPopulatedPasses();
}
