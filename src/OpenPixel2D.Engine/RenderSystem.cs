using OpenPixel2D.Abstractions;

namespace OpenPixel2D.Engine;

public abstract class RenderSystem : IRenderSystem
{
    public virtual void Initialize()
    {
    }

    public virtual void OnStart()
    {
    }

    public virtual void Render()
    {
    }

    public virtual void OnDestroy()
    {
    }

    public virtual void Dispose()
    {
    }
}