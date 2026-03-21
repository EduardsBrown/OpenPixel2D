using OpenPixel2D.Abstractions;

namespace OpenPixel2D.Engine;

public sealed class Entity
{
    public World World { get; private set; }

    internal void SetWorld(World world)
    {
        World = world;
    }

    public void Dispose()
    {
        World = null;
    }
}