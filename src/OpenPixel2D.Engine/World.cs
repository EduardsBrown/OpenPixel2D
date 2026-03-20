using OpenPixel2D.Abstractions;

namespace OpenPixel2D.Engine;

public class World : IDisposable
{
    private ResourceQueue<Entity> _entityQueue = new();
    private ResourceQueue<UpdateSystem> _updateSystemQueue = new();
    private ResourceQueue<RenderSystem> _renderSystemQueue = new();

    private WorldState _state = WorldState.Bootstrap;

    private List<Entity> _entities { get; set; } = [];
    private List<IUpdateSystem> _updateSystems { get; set; } = [];
    private List<IRenderSystem> _renderSystems { get; set; } = [];
    private Dictionary<SystemGroup, List<IUpdateSystem>> _updateSystemGroups { get; set; } = [];

    public IReadOnlyList<Entity> Entities => _entities;

    public void Initialize()
    {
        _state = WorldState.Initialised;

        foreach (var entity in _entities)
        {
            entity.Initialize();
        }

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
        foreach (var entity in _entities)
        {
            entity.OnStart();
        }

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

        _entityQueue.Flush();
        _updateSystemQueue.Flush();
        _renderSystemQueue.Flush();
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
        if (_state == WorldState.Bootstrap)
        {
            Add(entity);
            return;
        }

        if (_entityQueue.HasPendingAdd(entity))
        {
            return;
        }

        if (IsEntityActive(entity) && !_entityQueue.HasPendingRemove(entity))
        {
            return;
        }

        _entityQueue.QueueAdd(entity, Add);
    }

    public void RemoveEntity(Entity entity)
    {
        if (_state == WorldState.Bootstrap)
        {
            Remove(entity);
            return;
        }

        if (_entityQueue.HasPendingRemove(entity))
        {
            return;
        }

        if (!IsEntityActive(entity) && !_entityQueue.HasPendingAdd(entity))
        {
            return;
        }

        _entityQueue.QueueRemove(entity, Remove);
    }

    public void AddSystem(UpdateSystem system)
    {
        if (_state == WorldState.Bootstrap)
        {
            Add(system);
            return;
        }

        if (_updateSystemQueue.HasPendingAdd(system))
        {
            return;
        }

        if (IsUpdateSystemActive(system) && !_updateSystemQueue.HasPendingRemove(system))
        {
            return;
        }

        _updateSystemQueue.QueueAdd(system, Add);
    }

    public void RemoveSystem(UpdateSystem system)
    {
        if (_state == WorldState.Bootstrap)
        {
            Remove(system);
            return;
        }

        if (_updateSystemQueue.HasPendingRemove(system))
        {
            return;
        }

        if (!IsUpdateSystemActive(system) && !_updateSystemQueue.HasPendingAdd(system))
        {
            return;
        }

        _updateSystemQueue.QueueRemove(system, Remove);
    }

    public void AddSystem(RenderSystem system)
    {
        if (_state == WorldState.Bootstrap)
        {
            Add(system);
            return;
        }

        if (_renderSystemQueue.HasPendingAdd(system))
        {
            return;
        }

        if (IsRenderSystemActive(system) && !_renderSystemQueue.HasPendingRemove(system))
        {
            return;
        }

        _renderSystemQueue.QueueAdd(system, Add);
    }

    public void RemoveSystem(RenderSystem system)
    {
        if (_state == WorldState.Bootstrap)
        {
            Remove(system);
            return;
        }

        if (_renderSystemQueue.HasPendingRemove(system))
        {
            return;
        }

        if (!IsRenderSystemActive(system) && !_renderSystemQueue.HasPendingAdd(system))
        {
            return;
        }

        _renderSystemQueue.QueueRemove(system, Remove);
    }

    private void Add(Entity entity)
    {
        if (IsEntityActive(entity))
        {
            return;
        }

        entity.SetWorld(this);
        _entities.Add(entity);
    }

    private void Remove(Entity entity)
    {
        if (!_entities.Remove(entity))
        {
            return;
        }

        entity.OnDestroy();
        entity.Dispose();
    }

    private void Add(UpdateSystem system)
    {
        if (IsUpdateSystemActive(system))
        {
            return;
        }

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
        if (IsRenderSystemActive(system))
        {
            return;
        }

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
            system.Dispose();
        }

        foreach (var system in _renderSystems)
        {
            system.Dispose();
        }

        foreach (var entity in _entities)
        {
            entity.Dispose();
        }

        foreach (var entity in _entityQueue.DrainPendingAdds())
        {
            entity.Dispose();
        }

        foreach (var system in _updateSystemQueue.DrainPendingAdds())
        {
            system.Dispose();
        }

        foreach (var system in _renderSystemQueue.DrainPendingAdds())
        {
            system.Dispose();
        }

        _entityQueue.Clear();
        _updateSystemQueue.Clear();
        _renderSystemQueue.Clear();

        _updateSystems.Clear();
        _updateSystemGroups.Clear();
        _renderSystems.Clear();
        _entities.Clear();
    }

    private bool IsEntityActive(Entity entity)
    {
        return _entities.Contains(entity);
    }

    private bool IsUpdateSystemActive(UpdateSystem system)
    {
        return _updateSystems.Contains(system);
    }

    private bool IsRenderSystemActive(RenderSystem system)
    {
        return _renderSystems.Contains(system);
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
}
