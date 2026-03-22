namespace OpenPixel2D.Engine;

internal sealed class ComponentTypeEntityIndex
{
    private readonly Dictionary<Type, EntityBucket> _entitiesByQueryType = [];
    private readonly Dictionary<Type, Type[]> _queryTypesByComponentType = [];

    public IReadOnlyCollection<Entity> GetEntities(Type componentType) => GetOrCreateBucket(componentType);

    public void Add(Component component)
    {
        Entity? entity = component.Parent;

        if (entity == null)
        {
            return;
        }

        foreach (Type queryType in GetQueryTypes(component.GetType()))
        {
            GetOrCreateBucket(queryType).Add(entity);
        }
    }

    public void Remove(Component component)
    {
        Entity? entity = component.Parent;

        if (entity == null)
        {
            return;
        }

        foreach (Type queryType in GetQueryTypes(component.GetType()))
        {
            GetOrCreateBucket(queryType).Remove(entity);
        }
    }

    public void Clear()
    {
        foreach (EntityBucket bucket in _entitiesByQueryType.Values)
        {
            bucket.Clear();
        }

        _queryTypesByComponentType.Clear();
    }

    private EntityBucket GetOrCreateBucket(Type componentType)
    {
        if (_entitiesByQueryType.TryGetValue(componentType, out EntityBucket? bucket))
        {
            return bucket;
        }

        bucket = new EntityBucket();
        _entitiesByQueryType.Add(componentType, bucket);
        return bucket;
    }

    private Type[] GetQueryTypes(Type componentType)
    {
        if (_queryTypesByComponentType.TryGetValue(componentType, out Type[]? queryTypes))
        {
            return queryTypes;
        }

        List<Type> types = [];

        for (Type? current = componentType; current != null && typeof(Component).IsAssignableFrom(current); current = current.BaseType)
        {
            types.Add(current);

            if (current == typeof(Component))
            {
                break;
            }
        }

        queryTypes = [.. types];
        _queryTypesByComponentType.Add(componentType, queryTypes);
        return queryTypes;
    }

    private sealed class EntityBucket : IReadOnlyCollection<Entity>
    {
        private readonly Dictionary<Entity, int> _entityCounts = [];

        public int Count => _entityCounts.Count;

        public void Add(Entity entity)
        {
            if (_entityCounts.TryGetValue(entity, out int count))
            {
                _entityCounts[entity] = count + 1;
                return;
            }

            _entityCounts.Add(entity, 1);
        }

        public void Remove(Entity entity)
        {
            if (!_entityCounts.TryGetValue(entity, out int count))
            {
                return;
            }

            if (count == 1)
            {
                _entityCounts.Remove(entity);
                return;
            }

            _entityCounts[entity] = count - 1;
        }

        public void Clear() => _entityCounts.Clear();

        public IEnumerator<Entity> GetEnumerator() => _entityCounts.Keys.GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
