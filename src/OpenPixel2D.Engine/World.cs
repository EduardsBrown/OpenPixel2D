using OpenPixel2D.Abstractions;

namespace OpenPixel2D.Engine;

public class World : IDisposable
{
    private List<Entity> _entities { get; set; } = [];
    private List<IUpdateSystem> _updateSystems { get; set; } = [];
    private List<IRenderSystem> _renderSystems { get; set; } = [];
    private Dictionary<SystemGroup, List<IUpdateSystem>> _updateSystemGroups { get; set; } = [];

    public IReadOnlyList<Entity> Entities => _entities;

    public void Initialize()
    {
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
    }

    public void Update()
    {
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
        entity.SetWorld(this);
        _entities.Add(entity);
    }

    public void RemoveEntity(Entity entity)
    {
        entity.Dispose();
        _entities.Remove(entity);
    }

    public void AddSystem(UpdateSystem system)
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

    public void RemoveSystem(UpdateSystem system)
    {
        system.Dispose();
        _updateSystems.Remove(system);
        _updateSystemGroups[system.Group].Remove(system);
    }

    public void AddSystem(RenderSystem system)
    {
        system.SetWorld(this);
        _renderSystems.Add(system);
    }

    public void RemoveSystem(RenderSystem system)
    {
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