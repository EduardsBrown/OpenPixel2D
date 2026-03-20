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
        }
        else
        {
            _entityQueue.QueueAdd(entity, Add);
        }
    }

    public void RemoveEntity(Entity entity)
    {
        if (_state == WorldState.Bootstrap)
        {
            Remove(entity);
        }
        else
        {
            _entityQueue.QueueRemove(entity, Remove);
        }
    }

    public void AddSystem(UpdateSystem system)
    {
        if (_state == WorldState.Bootstrap)
        {
            Add(system);
        }
        else
        {
            _updateSystemQueue.QueueAdd(system, Add);
        }
    }

    public void RemoveSystem(UpdateSystem system)
    {
        if (_state == WorldState.Bootstrap)
        {
            Remove(system);
        }
        else
        {
            _updateSystemQueue.QueueRemove(system, Remove);
        }
    }

    public void AddSystem(RenderSystem system)
    {
        if (_state == WorldState.Bootstrap)
        {
            Add(system);
        }
        else
        {
            _renderSystemQueue.QueueAdd(system, Add);
        }
    }

    public void RemoveSystem(RenderSystem system)
    {
        if (_state == WorldState.Bootstrap)
        {
            Remove(system);
        }
        else
        {
            _renderSystemQueue.QueueRemove(system, Remove);
        }
    }

    private void Add(Entity entity)
    {
        entity.SetWorld(this);
        _entities.Add(entity);
    }

    private void Remove(Entity entity)
    {
        entity.OnDestroy();
        entity.Dispose();
        _entities.Remove(entity);
    }

    private void Add(UpdateSystem system)
    {
        system.SetWorld(this);
        _updateSystems.Add(system);

        if (!_updateSystemGroups.TryGetValue(system.Group, out var systems))
        {
            systems = new List<IUpdateSystem>();
            _updateSystemGroups.Add(system.Group, systems);
        }

        systems.Add(system);
    }

    private void Remove(UpdateSystem system)
    {
        system.OnDestroy();
        system.Dispose();
        _updateSystems.Remove(system);
        _updateSystemGroups[system.Group].Remove(system);
    }

    private void Add(RenderSystem system)
    {
        system.SetWorld(this);
        _renderSystems.Add(system);
    }

    private void Remove(RenderSystem system)
    {
        system.OnDestroy();
        system.Dispose();
        _renderSystems.Remove(system);
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

        _updateSystems.Clear();
        _updateSystemGroups.Clear();
        _renderSystems.Clear();
        _entities.Clear();
    }
}