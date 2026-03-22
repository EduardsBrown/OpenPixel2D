namespace OpenPixel2D.Engine;

public sealed class World : IDisposable
{
    private WorldState _state = WorldState.Created;

    private readonly List<Entity> _entities = [];
    private readonly AttachmentRegistry<Component> _componentRegistry = new();
    private readonly AttachmentRegistry<BehaviorComponent> _behaviorRegistry = new();
    private readonly AttachmentRegistry<UpdateSystem> _updateSysRegistry = new();
    private readonly AttachmentRegistry<RenderSystem> _renderSysRegistry = new();

    public IReadOnlyList<Entity> Entities => _entities;
    public IReadOnlyList<UpdateSystem> UpdateSystems => _updateSysRegistry.Items;
    public IReadOnlyList<RenderSystem> RenderSystems => _renderSysRegistry.Items;
    internal IReadOnlyList<Component> RegisteredComponents => _componentRegistry.Items;
    internal IReadOnlyList<BehaviorComponent> RegisteredBehaviorComponents => _behaviorRegistry.Items;

    #region Lifecycle

    public void Initialize()
    {
        // TODO: Init

        _state = WorldState.Initialized;
    }

    public void OnStart()
    {
        // TODO: Start

        _state = WorldState.Started;
    }

    public void Update()
    {
    }

    public void Render()
    {
    }

    public void OnDestroy()
    {
        // TODO: Destroy

        _state = WorldState.Destroyed;
    }

    public void Dispose()
    {
        // TODO: Dispose

        _state = WorldState.Disposed;
    }

    #endregion

    public void AddEntity(Entity? entity)
    {
        if (entity == null)
        {
            return;
        }

        if (entity.World == this && entity.Parent == null && _entities.Contains(entity))
        {
            return;
        }

        if (entity.World == this)
        {
            entity.DetachFromOwnerPreservingWorld();
        }
        else
        {
            entity.DetachFromOwner();
        }

        AddRootDirect(entity);

        if (entity.World == this)
        {
            return;
        }

        entity.SetWorldRecursive(this);
        RegisterSubtree(entity);
    }

    public void RemoveEntity(Entity? entity)
    {
        if (entity == null || entity.Parent != null || entity.World != this || !_entities.Contains(entity))
        {
            return;
        }

        entity.DetachFromOwner();
    }

    public void AddSystem(UpdateSystem? system)
    {
        if (system == null)
        {
            return;
        }

        if (system.World == this && _updateSysRegistry.Items.Contains(system))
        {
            return;
        }

        system.World?.DetachUpdateSystem(system);
        AttachUpdateSystem(system);
    }

    public void RemoveSystem(UpdateSystem? system)
    {
        if (system == null || system.World != this)
        {
            return;
        }

        DetachUpdateSystem(system);
    }

    public void AddSystem(RenderSystem? system)
    {
        if (system == null)
        {
            return;
        }

        if (system.World == this && _renderSysRegistry.Items.Contains(system))
        {
            return;
        }

        system.World?.DetachRenderSystem(system);
        AttachRenderSystem(system);
    }

    public void RemoveSystem(RenderSystem? system)
    {
        if (system == null || system.World != this)
        {
            return;
        }

        DetachRenderSystem(system);
    }

    internal void RegisterComponent(Component component)
    {
        _componentRegistry.Add(component);

        if (component is BehaviorComponent behaviorComponent)
        {
            _behaviorRegistry.Add(behaviorComponent);
        }
    }

    internal void UnregisterComponent(Component component)
    {
        _componentRegistry.Remove(component);

        if (component is BehaviorComponent behaviorComponent)
        {
            _behaviorRegistry.Remove(behaviorComponent);
        }
    }

    internal void RegisterSubtree(Entity entity)
    {
        foreach (Component component in entity.EnumerateSubtreeComponents())
        {
            RegisterComponent(component);
        }
    }

    internal void UnregisterSubtree(Entity entity)
    {
        foreach (Component component in entity.EnumerateSubtreeComponents())
        {
            UnregisterComponent(component);
        }
    }

    internal void AddRootDirect(Entity entity)
    {
        entity.SetParent(null);

        if (_entities.Contains(entity))
        {
            return;
        }

        _entities.Add(entity);
    }

    internal void RemoveRootDirect(Entity entity)
    {
        _entities.Remove(entity);
        entity.SetParent(null);
    }

    private void AttachUpdateSystem(UpdateSystem system)
    {
        _updateSysRegistry.Add(system);
        system.SetWorld(this);
    }

    private void DetachUpdateSystem(UpdateSystem system)
    {
        if (!_updateSysRegistry.Items.Contains(system))
        {
            return;
        }

        _updateSysRegistry.Remove(system);

        if (system.World == this)
        {
            system.SetWorld(null);
        }
    }

    private void AttachRenderSystem(RenderSystem system)
    {
        _renderSysRegistry.Add(system);
        system.SetWorld(this);
    }

    private void DetachRenderSystem(RenderSystem system)
    {
        if (!_renderSysRegistry.Items.Contains(system))
        {
            return;
        }

        _renderSysRegistry.Remove(system);

        if (system.World == this)
        {
            system.SetWorld(null);
        }
    }
}
