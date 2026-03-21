using OpenPixel2D.Abstractions;

namespace OpenPixel2D.Engine;

public abstract class UpdateSystem : IUpdateSystem
{
    private World? _world;

    public SystemGroup Group { get; set; }
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

    public virtual void Update()
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