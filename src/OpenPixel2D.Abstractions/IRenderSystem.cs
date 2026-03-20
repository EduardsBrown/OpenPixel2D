namespace OpenPixel2D.Abstractions;

public interface IRenderSystem : IDisposable
{
    void Initialize();
    void OnStart();
    void Render();
}