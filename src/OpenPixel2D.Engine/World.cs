using OpenPixel2D.Abstractions;

namespace OpenPixel2D.Engine;

public class World : IDisposable
{
    private readonly ResourceQueue<UpdateSystem> _updateSystemQueue;
    private readonly ResourceQueue<RenderSystem> _renderSystemQueue;

    private WorldState _state = WorldState.Bootstrap;

    private List<Entity> _entities { get; set; } = [];
    private List<IUpdateSystem> _updateSystems { get; set; } = [];
    private List<IRenderSystem> _renderSystems { get; set; } = [];
    private List<IBehaviorComponent> _behaviors { get; set; } = [];
    private Dictionary<SystemGroup, List<IUpdateSystem>> _updateSystemGroups { get; set; } = [];
    private readonly Dictionary<Type, HashSet<Entity>> _entityCache = new();

    public IReadOnlyList<Entity> Entities => _entities;

    public World()
    {
        _updateSystemQueue = new ResourceQueue<UpdateSystem>(Add, Remove);
        _renderSystemQueue = new ResourceQueue<RenderSystem>(Add, Remove);
    }

    public void Initialize()
    {
        _state = WorldState.Initialised;

        foreach (var system in _renderSystems)
        {
            system.Initialize();
        }

        foreach (var system in _updateSystems)
        {
            system.Initialize();
        }
    }

    public void Start()
    {
        foreach (var system in _renderSystems)
        {
            system.OnStart();
        }

        foreach (var system in _updateSystems)
        {
            system.OnStart();
        }

        _state = WorldState.Running;
    }

    public void Update()
    {
        if (_state != WorldState.Running)
        {
            return;
        }

        _updateSystemQueue.Flush();
        _renderSystemQueue.Flush();

        UpdateGroup(SystemGroup.Default);

        // TODO: Update BehaviorComponents

        UpdateGroup(SystemGroup.Physics);
        UpdateGroup(SystemGroup.PostPhysics);
    }

    public void Render()
    {
        foreach (var system in _renderSystems)
        {
            system.Render();
        }
    }

    public void AddEntity(Entity entity)
    {
        entity.DetachFromCurrentContainer();
        entity.SetWorld(this);
        _entities.Add(entity);
    }

    public void RemoveEntity(Entity entity)
    {
        if (entity.Parent is not null)
        {
            if (!ReferenceEquals(entity.AttachedWorld, this))
            {
                return;
            }

            entity.Parent.RemoveEntity(entity);
            return;
        }

        if (!_entities.Remove(entity))
        {
            return;
        }

        entity.Dispose();
    }

    internal void RegisterEntityComponent(Entity entity, IComponent component)
    {
        var type = component.GetType();

        if (!_entityCache.ContainsKey(type))
        {
            _entityCache[type] = [];
        }

        _entityCache[type].Add(entity);
    }

    internal void UnregisterEntityComponent(Entity entity, IComponent component)
    {
        var type = component.GetType();

        if (_entityCache.TryGetValue(type, out var entities))
        {
            entities.Remove(entity);
        }
    }

    public IEnumerable<Entity> GetEntitiesWith<T>() where T : IComponent
    {
        if (_entityCache.TryGetValue(typeof(T), out var entities))
        {
            return entities;
        }

        return Enumerable.Empty<Entity>();
    }

    public void AddSystem(UpdateSystem system)
    {
        if (_state == WorldState.Bootstrap)
        {
            Add(system);
            return;
        }

        _updateSystemQueue.QueueAdd(system);
    }

    public void RemoveSystem(UpdateSystem system)
    {
        if (_state == WorldState.Bootstrap)
        {
            Remove(system);
            return;
        }

        _updateSystemQueue.QueueRemove(system);
    }

    public void AddSystem(RenderSystem system)
    {
        if (_state == WorldState.Bootstrap)
        {
            Add(system);
            return;
        }

        _renderSystemQueue.QueueAdd(system);
    }

    public void RemoveSystem(RenderSystem system)
    {
        if (_state == WorldState.Bootstrap)
        {
            Remove(system);
            return;
        }

        _renderSystemQueue.QueueRemove(system);
    }

    private void Add(UpdateSystem system)
    {
        system.SetWorld(this);
        _updateSystems.Add(system);
        GetOrCreateUpdateSystemGroup(system.Group).Add(system);
    }

    private void Remove(UpdateSystem system)
    {
        if (!_updateSystems.Remove(system))
        {
            return;
        }

        if (_updateSystemGroups.TryGetValue(system.Group, out var systems))
        {
            systems.Remove(system);

            if (systems.Count == 0)
            {
                _updateSystemGroups.Remove(system.Group);
            }
        }

        system.OnDestroy();
        system.Dispose();
    }

    private void Add(RenderSystem system)
    {
        system.SetWorld(this);
        _renderSystems.Add(system);
    }

    private void Remove(RenderSystem system)
    {
        if (!_renderSystems.Remove(system))
        {
            return;
        }

        system.OnDestroy();
        system.Dispose();
    }

    public void Dispose()
    {
        foreach (var system in _updateSystems)
        {
            system.OnDestroy();
            system.Dispose();
        }

        foreach (var system in _renderSystems)
        {
            system.OnDestroy();
            system.Dispose();
        }

        foreach (var entity in _entities.ToArray())
        {
            entity.Dispose();
        }

        _updateSystemQueue.DrainPendingAdds(static system => system.Dispose());
        _renderSystemQueue.DrainPendingAdds(static system => system.Dispose());

        _updateSystemQueue.Clear();
        _renderSystemQueue.Clear();

        _updateSystems.Clear();
        _updateSystemGroups.Clear();
        _renderSystems.Clear();
        _entities.Clear();
        _entityCache.Clear();
    }

    internal bool DetachRootEntity(Entity entity)
    {
        return _entities.Remove(entity);
    }

    private List<IUpdateSystem> GetOrCreateUpdateSystemGroup(SystemGroup group)
    {
        if (!_updateSystemGroups.TryGetValue(group, out var systems))
        {
            systems = [];
            _updateSystemGroups.Add(group, systems);
        }

        return systems;
    }

    private void UpdateGroup(SystemGroup group)
    {
        if (!_updateSystemGroups.TryGetValue(group, out var systems))
        {
            return;
        }

        for (var i = 0; i < systems.Count; i++)
        {
            systems[i].Update();
        }
    }
}