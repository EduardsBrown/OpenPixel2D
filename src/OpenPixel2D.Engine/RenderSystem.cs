using OpenPixel2D.Abstractions;

namespace OpenPixel2D.Engine;

public abstract class RenderSystem : IRenderSystem
{
    public World? World { get; private set; }
    internal World? RegisteredWorld { get; private set; }

    internal void SetWorld(World? world) => World = world;
    internal void SetRegisteredWorld(World? world) => RegisteredWorld = world;

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
