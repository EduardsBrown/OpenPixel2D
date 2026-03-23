using OpenPixel2D.Abstractions;
using OpenPixel2D.Rendering.Abstractions;

namespace OpenPixel2D.Engine;

public abstract class RenderSystem : IRenderSystem
{
    public World? World { get; private set; }
    internal World? RegisteredWorld { get; private set; }
    internal LifecycleFlags LifecycleFlags { get; private set; }

    internal void SetWorld(World? world) => World = world;
    internal void SetRegisteredWorld(World? world) => RegisteredWorld = world;
    internal bool HasLifecycleFlag(LifecycleFlags flag) => (LifecycleFlags & flag) == flag;
    internal void MarkLifecycleFlag(LifecycleFlags flag) => LifecycleFlags |= flag;

    public virtual void Initialize()
    {
    }

    public virtual void OnStart()
    {
    }

    public virtual void Render(IRenderContext context)
    {
    }

    public virtual void OnDestroy()
    {
    }

    public virtual void Dispose()
    {
    }
}