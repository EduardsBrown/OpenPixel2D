using OpenPixel2D.Abstractions;

namespace OpenPixel2D.Engine;

public abstract class RenderSystem : IRenderSystem
{
    private World? _world;

    public World World => _world ?? throw new InvalidOperationException("System is not attached to a world.");

    internal void SetWorld(World world)
    {
        _world = world;
    }

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

    public void Dispose()
    {
        _world = null;
    }
}