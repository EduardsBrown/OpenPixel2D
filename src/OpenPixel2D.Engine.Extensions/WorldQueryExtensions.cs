using OpenPixel2D.Engine;

namespace OpenPixel2D.Engine.Extensions;

public static class WorldQueryExtensions
{
    public static IEnumerable<Entity> GetEntitiesWith<T>(this World world) where T : Component
    {
        ArgumentNullException.ThrowIfNull(world);
        return world.GetEntitiesWithComponentType(typeof(T));
    }
}
