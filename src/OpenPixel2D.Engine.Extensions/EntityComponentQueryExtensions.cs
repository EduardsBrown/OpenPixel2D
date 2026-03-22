using OpenPixel2D.Engine;

namespace OpenPixel2D.Engine.Extensions;

public static class EntityComponentQueryExtensions
{
    public static bool HasComponent<T>(this Entity entity) where T : Component
    {
        ArgumentNullException.ThrowIfNull(entity);

        for (int i = 0; i < entity.Components.Count; i++)
        {
            if (entity.Components[i] is T)
            {
                return true;
            }
        }

        return false;
    }

    public static bool TryGetComponent<T>(this Entity entity, out T? component) where T : Component
    {
        ArgumentNullException.ThrowIfNull(entity);

        for (int i = 0; i < entity.Components.Count; i++)
        {
            if (entity.Components[i] is T match)
            {
                component = match;
                return true;
            }
        }

        component = null;
        return false;
    }

    public static T? GetComponent<T>(this Entity entity) where T : Component
    {
        ArgumentNullException.ThrowIfNull(entity);
        TryGetComponent(entity, out T? component);
        return component;
    }

    public static IEnumerable<T> GetComponents<T>(this Entity entity) where T : Component
    {
        ArgumentNullException.ThrowIfNull(entity);
        return GetComponentsIterator<T>(entity);
    }

    private static IEnumerable<T> GetComponentsIterator<T>(Entity entity) where T : Component
    {
        for (int i = 0; i < entity.Components.Count; i++)
        {
            if (entity.Components[i] is T component)
            {
                yield return component;
            }
        }
    }
}
