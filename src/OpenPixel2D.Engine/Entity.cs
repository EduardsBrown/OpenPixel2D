using OpenPixel2D.Abstractions;

namespace OpenPixel2D.Engine;

public sealed class Entity : IAttachable
{
    public World World { get; private set; }

    public void Initialize()
    {
    }

    public void OnStart()
    {
    }

    internal void SetWorld(World world)
    {
        World = world;
    }

    public void Dispose()
    {
        World = null;
    }
}