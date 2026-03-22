using OpenPixel2D.Abstractions;

namespace OpenPixel2D.Engine;

public abstract class UpdateSystem : IUpdateSystem
{
    public SystemGroup Group { get; protected set; } = SystemGroup.Default;

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