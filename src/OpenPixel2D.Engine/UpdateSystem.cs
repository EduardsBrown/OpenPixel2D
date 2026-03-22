using OpenPixel2D.Abstractions;

namespace OpenPixel2D.Engine;

public abstract class UpdateSystem : IUpdateSystem
{
    public World? World { get; private set; }
    public SystemGroup Group { get; protected set; } = SystemGroup.Default;
    internal World? RegisteredWorld { get; private set; }

    internal void SetWorld(World? world) => World = world;
    internal void SetRegisteredWorld(World? world) => RegisteredWorld = world;

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

    public virtual void Dispose()
    {
    }
}
