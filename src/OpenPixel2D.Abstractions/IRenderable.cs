using OpenPixel2D.Rendering.Abstractions;

namespace OpenPixel2D.Abstractions;

public interface IRenderable
{
    void Render(IRenderContext context);
}