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
        if (system == null || system.World == this)
        {
            return;
        }

        system.World?.RemoveSystem(system);
        system.SetWorld(this);
        QueueRegisterUpdateSystem(system);
    }

    public void RemoveSystem(UpdateSystem? system)
    {
        if (system == null || system.World != this)
        {
            return;
        }

        system.SetWorld(null);
        QueueUnregisterUpdateSystem(system);
    }

    public void AddSystem(RenderSystem? system)
    {
        if (system == null || system.World == this)
        {
            return;
        }

        system.World?.RemoveSystem(system);
        system.SetWorld(this);
        QueueRegisterRenderSystem(system);
    }

    public void RemoveSystem(RenderSystem? system)
    {
        if (system == null || system.World != this)
        {
            return;
        }

        system.SetWorld(null);
        QueueUnregisterRenderSystem(system);
    }

    internal void RegisterComponent(Component component)
    {
        _componentRegistry.QueueAdd(component);

        if (component is BehaviorComponent behaviorComponent)
        {
            _behaviorRegistry.QueueAdd(behaviorComponent);
        }
    }

    internal void UnregisterComponent(Component component)
    {
        if (component is BehaviorComponent behaviorComponent)
        {
            _behaviorRegistry.QueueRemove(behaviorComponent);
        }

        _componentRegistry.QueueRemove(component);
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

    internal void FlushPendingRemovals()
    {
        _behaviorRegistry.FlushRemovals(static _ => { });
        _componentRegistry.FlushRemovals(component => component.SetRegisteredWorld(null));
        _updateSysRegistry.FlushRemovals(system => system.SetRegisteredWorld(null));
        _renderSysRegistry.FlushRemovals(system => system.SetRegisteredWorld(null));
    }

    internal void FlushPendingAdditions()
    {
        _componentRegistry.FlushAdditions(component =>
        {
            if (component.Parent?.World != this || component.RegisteredWorld != null)
            {
                return false;
            }

            component.SetRegisteredWorld(this);
            return true;
        });

        _behaviorRegistry.FlushAdditions(behaviorComponent =>
        {
            if (behaviorComponent.Parent?.World != this ||
                behaviorComponent.RegisteredWorld != this ||
                !_componentRegistry.Items.Contains(behaviorComponent))
            {
                return false;
            }

            return true;
        });

        _updateSysRegistry.FlushAdditions(system =>
        {
            if (system.World != this || system.RegisteredWorld != null)
            {
                return false;
            }

            system.SetRegisteredWorld(this);
            return true;
        });

        _renderSysRegistry.FlushAdditions(system =>
        {
            if (system.World != this || system.RegisteredWorld != null)
            {
                return false;
            }

            system.SetRegisteredWorld(this);
            return true;
        });
    }

    private void QueueRegisterUpdateSystem(UpdateSystem system)
    {
        _updateSysRegistry.QueueAdd(system);
    }

    private void QueueUnregisterUpdateSystem(UpdateSystem system)
    {
        _updateSysRegistry.QueueRemove(system);
    }

    private void QueueRegisterRenderSystem(RenderSystem system)
    {
        _renderSysRegistry.QueueAdd(system);
    }

    private void QueueUnregisterRenderSystem(RenderSystem system)
    {
        _renderSysRegistry.QueueRemove(system);
    }
}
