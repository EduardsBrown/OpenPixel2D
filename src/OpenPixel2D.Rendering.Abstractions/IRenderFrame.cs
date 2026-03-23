namespace OpenPixel2D.Rendering.Abstractions;

public interface IRenderFrame
{
    IRenderPassWriter GetPass(string passName);
}