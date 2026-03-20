namespace OpenPixel2D.Abstractions;

public interface IRenderSystem : IDisposable, IAttachable
{
    void Render();
}