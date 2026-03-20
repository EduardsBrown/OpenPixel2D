using OpenPixel2D.Abstractions;

namespace OpenPixel2D.Engine;

public abstract class UpdateSystem : IUpdateSystem
{
    public SystemGroup Group { get; set; }
    public World World { get; private set; }

    internal void SetWorld(World world)
    {
        World = world;
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

    public void Dispose()
    {
        World = null;
    }
}