using OpenPixel2D.Abstractions;
using OpenPixel2D.Rendering.Abstractions;

namespace OpenPixel2D.Engine;

public sealed class World : IDisposable
{
    private WorldState _state = WorldState.Created;

    private readonly List<Entity> _entities = [];
    private readonly AttachmentRegistry<Component> _componentRegistry = new();
    private readonly AttachmentRegistry<BehaviorComponent> _behaviorRegistry = new();
    private readonly AttachmentRegistry<UpdateSystem> _updateSysRegistry = new();
    private readonly AttachmentRegistry<RenderSystem> _renderSysRegistry = new();
    private readonly ComponentTypeEntityIndex _componentTypeEntityIndex = new();

    public IReadOnlyList<Entity> Entities => _entities;
    public IReadOnlyList<UpdateSystem> UpdateSystems => _updateSysRegistry.Items;
    public IReadOnlyList<RenderSystem> RenderSystems => _renderSysRegistry.Items;
    internal IReadOnlyList<Component> RegisteredComponents => _componentRegistry.Items;
    internal IReadOnlyList<BehaviorComponent> RegisteredBehaviorComponents => _behaviorRegistry.Items;

    #region Lifecycle

    public void Initialize()
    {
        EnsureState(WorldState.Created, nameof(Initialize));

        SynchronizePreStartRegistrations();
        InitializeActiveObjects();

        _state = WorldState.Initialized;
    }

    public void Start()
    {
        EnsureState(WorldState.Initialized, nameof(Start));

        SynchronizePreStartRegistrations();
        InitializeActiveObjects();
        StartActiveObjects();

        _state = WorldState.Started;
    }

    public void Update()
    {
        EnsureState(WorldState.Started, nameof(Update));

        List<Component> activatedComponents = [];
        List<UpdateSystem> activatedUpdateSystems = [];
        List<RenderSystem> activatedRenderSystems = [];

        FlushPendingRemovals();
        FlushPendingAdditions(activatedComponents, activatedUpdateSystems, activatedRenderSystems);
        CatchUpActivatedObjects(activatedComponents, activatedUpdateSystems, activatedRenderSystems);

        UpdateSystemsByGroup(SystemGroup.Default);
        UpdateBehaviorComponents();
        UpdateSystemsByGroup(SystemGroup.Physics);
        UpdateSystemsByGroup(SystemGroup.PostPhysics);
    }

    public void Render(IRenderContext context)
    {
        EnsureState(WorldState.Started, nameof(Render));

        for (int i = 0; i < _renderSysRegistry.Items.Count; i++)
        {
            RenderSystem system = _renderSysRegistry.Items[i];

            if (!system.HasLifecycleFlag(LifecycleFlags.Started))
            {
                continue;
            }

            system.Render(context);
        }
    }

    public void Destroy()
    {
        EnsureState(WorldState.Started, nameof(Destroy));

        DestroyActiveObjects();

        _state = WorldState.Destroyed;
    }

    public void Dispose()
    {
        if (_state == WorldState.Disposed)
        {
            throw new InvalidOperationException("Dispose can only be called once.");
        }

        if (_state == WorldState.Started)
        {
            Destroy();
        }
        else if (_state == WorldState.Initialized)
        {
            SynchronizePreStartRegistrations();
            InitializeActiveObjects();
        }

        DisposeActiveObjects();
        ReleaseWorldOwnership();

        _state = WorldState.Disposed;
        GC.SuppressFinalize(this);
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
        if (system == null || system.HasLifecycleFlag(LifecycleFlags.Disposed))
        {
            return;
        }

        if (system.World == this)
        {
            if (_updateSysRegistry.Contains(system) && !_updateSysRegistry.IsPendingRemove(system))
            {
                return;
            }

            if (_updateSysRegistry.IsPendingAdd(system))
            {
                return;
            }

            QueueRegisterUpdateSystem(system);
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

        if (_updateSysRegistry.IsPendingAdd(system) && !_updateSysRegistry.Contains(system))
        {
            QueueUnregisterUpdateSystem(system);

            if (system.RegisteredWorld == null)
            {
                system.SetWorld(null);
            }
            else if (system.RegisteredWorld != this)
            {
                system.SetWorld(system.RegisteredWorld);
            }

            return;
        }

        if (_updateSysRegistry.Contains(system) || _updateSysRegistry.IsPendingRemove(system))
        {
            QueueUnregisterUpdateSystem(system);
            return;
        }

        system.SetWorld(null);
    }

    public void AddSystem(RenderSystem? system)
    {
        if (system == null || system.HasLifecycleFlag(LifecycleFlags.Disposed))
        {
            return;
        }

        if (system.World == this)
        {
            if (_renderSysRegistry.Contains(system) && !_renderSysRegistry.IsPendingRemove(system))
            {
                return;
            }

            if (_renderSysRegistry.IsPendingAdd(system))
            {
                return;
            }

            QueueRegisterRenderSystem(system);
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

        if (_renderSysRegistry.IsPendingAdd(system) && !_renderSysRegistry.Contains(system))
        {
            QueueUnregisterRenderSystem(system);

            if (system.RegisteredWorld == null)
            {
                system.SetWorld(null);
            }
            else if (system.RegisteredWorld != this)
            {
                system.SetWorld(system.RegisteredWorld);
            }

            return;
        }

        if (_renderSysRegistry.Contains(system) || _renderSysRegistry.IsPendingRemove(system))
        {
            QueueUnregisterRenderSystem(system);
            return;
        }

        system.SetWorld(null);
    }

    public IReadOnlyCollection<Entity> GetEntitiesWithComponentType(Type componentType)
    {
        if (componentType == null)
        {
            throw new ArgumentException("Component type cannot be null.", nameof(componentType));
        }

        if (componentType.IsInterface || !typeof(Component).IsAssignableFrom(componentType))
        {
            throw new ArgumentException(
                $"Component type must be {nameof(Component)} or a non-interface type derived from it.",
                nameof(componentType));
        }

        return _componentTypeEntityIndex.GetEntities(componentType);
    }

    internal void RegisterComponent(Component component)
    {
        if (component.HasLifecycleFlag(LifecycleFlags.Disposed))
        {
            return;
        }

        _componentRegistry.QueueAdd(component);

        if (component.RegisteredWorld == this && component.Parent?.World == this)
        {
            _componentTypeEntityIndex.Add(component);
        }

        if (component is BehaviorComponent behaviorComponent)
        {
            _behaviorRegistry.QueueAdd(behaviorComponent);
        }
    }

    internal void UnregisterComponent(Component component)
    {
        if (component.RegisteredWorld == this)
        {
            _componentTypeEntityIndex.Remove(component);
        }

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
        _componentRegistry.FlushRemovals(FlushRemovedComponent);
        _updateSysRegistry.FlushRemovals(FlushRemovedUpdateSystem);
        _renderSysRegistry.FlushRemovals(FlushRemovedRenderSystem);
    }

    internal void FlushPendingAdditions(
        List<Component>? activatedComponents = null,
        List<UpdateSystem>? activatedUpdateSystems = null,
        List<RenderSystem>? activatedRenderSystems = null)
    {
        _componentRegistry.FlushAdditions(component =>
        {
            if (component.Parent?.World != this)
            {
                return AttachmentActivationResult.Discarded;
            }

            if (component.HasLifecycleFlag(LifecycleFlags.Disposed))
            {
                return AttachmentActivationResult.Discarded;
            }

            if (component.RegisteredWorld != null)
            {
                return AttachmentActivationResult.Pending;
            }

            component.SetRegisteredWorld(this);
            _componentTypeEntityIndex.Add(component);
            activatedComponents?.Add(component);
            return AttachmentActivationResult.Activated;
        });

        _behaviorRegistry.FlushAdditions(behaviorComponent =>
        {
            if (behaviorComponent.Parent?.World != this || behaviorComponent.HasLifecycleFlag(LifecycleFlags.Disposed))
            {
                return AttachmentActivationResult.Discarded;
            }

            if (behaviorComponent.RegisteredWorld != this || !_componentRegistry.Contains(behaviorComponent))
            {
                return AttachmentActivationResult.Pending;
            }

            return AttachmentActivationResult.Activated;
        });

        _updateSysRegistry.FlushAdditions(system =>
        {
            if (system.World != this)
            {
                return AttachmentActivationResult.Discarded;
            }

            if (system.HasLifecycleFlag(LifecycleFlags.Disposed))
            {
                system.SetWorld(null);
                return AttachmentActivationResult.Discarded;
            }

            if (system.RegisteredWorld != null)
            {
                return AttachmentActivationResult.Pending;
            }

            system.SetRegisteredWorld(this);
            activatedUpdateSystems?.Add(system);
            return AttachmentActivationResult.Activated;
        });

        _renderSysRegistry.FlushAdditions(system =>
        {
            if (system.World != this)
            {
                return AttachmentActivationResult.Discarded;
            }

            if (system.HasLifecycleFlag(LifecycleFlags.Disposed))
            {
                system.SetWorld(null);
                return AttachmentActivationResult.Discarded;
            }

            if (system.RegisteredWorld != null)
            {
                return AttachmentActivationResult.Pending;
            }

            system.SetRegisteredWorld(this);
            activatedRenderSystems?.Add(system);
            return AttachmentActivationResult.Activated;
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

    private void SynchronizePreStartRegistrations()
    {
        FlushPendingRemovals();
        FlushPendingAdditions();
    }

    private void CatchUpActivatedObjects(
        IReadOnlyList<Component> activatedComponents,
        IReadOnlyList<UpdateSystem> activatedUpdateSystems,
        IReadOnlyList<RenderSystem> activatedRenderSystems)
    {
        InitializeComponents(activatedComponents);
        InitializeUpdateSystems(activatedUpdateSystems);
        InitializeRenderSystems(activatedRenderSystems);
        StartActivatedBehaviorComponents(activatedComponents);
        StartUpdateSystems(activatedUpdateSystems);
        StartRenderSystems(activatedRenderSystems);
    }

    private void InitializeActiveObjects()
    {
        InitializeComponents(_componentRegistry.Items);
        InitializeUpdateSystems(_updateSysRegistry.Items);
        InitializeRenderSystems(_renderSysRegistry.Items);
    }

    private void StartActiveObjects()
    {
        StartBehaviorComponents(_behaviorRegistry.Items);
        StartUpdateSystems(_updateSysRegistry.Items);
        StartRenderSystems(_renderSysRegistry.Items);
    }

    private void DestroyActiveObjects()
    {
        DestroyBehaviorComponents(_behaviorRegistry.Items);
        DestroyUpdateSystems(_updateSysRegistry.Items);
        DestroyRenderSystems(_renderSysRegistry.Items);
    }

    private void DisposeActiveObjects()
    {
        DisposeComponents(_componentRegistry.Items);
        DisposeUpdateSystems(_updateSysRegistry.Items);
        DisposeRenderSystems(_renderSysRegistry.Items);
    }

    private void ReleaseWorldOwnership()
    {
        for (int i = 0; i < _entities.Count; i++)
        {
            _entities[i].SetWorldRecursive(null);
        }

        _entities.Clear();
        _componentTypeEntityIndex.Clear();

        for (int i = 0; i < _componentRegistry.Items.Count; i++)
        {
            _componentRegistry.Items[i].SetRegisteredWorld(null);
        }

        for (int i = 0; i < _updateSysRegistry.Items.Count; i++)
        {
            UpdateSystem system = _updateSysRegistry.Items[i];
            system.SetRegisteredWorld(null);

            if (system.World == this)
            {
                system.SetWorld(null);
            }
        }

        for (int i = 0; i < _renderSysRegistry.Items.Count; i++)
        {
            RenderSystem system = _renderSysRegistry.Items[i];
            system.SetRegisteredWorld(null);

            if (system.World == this)
            {
                system.SetWorld(null);
            }
        }

        for (int i = 0; i < _updateSysRegistry.PendingAdds.Count; i++)
        {
            UpdateSystem system = _updateSysRegistry.PendingAdds[i];

            if (system.World == this)
            {
                system.SetWorld(null);
            }
        }

        for (int i = 0; i < _renderSysRegistry.PendingAdds.Count; i++)
        {
            RenderSystem system = _renderSysRegistry.PendingAdds[i];

            if (system.World == this)
            {
                system.SetWorld(null);
            }
        }

        _behaviorRegistry.ClearAll();
        _componentRegistry.ClearAll();
        _updateSysRegistry.ClearAll();
        _renderSysRegistry.ClearAll();
    }

    private void InitializeComponents(IReadOnlyList<Component> components)
    {
        for (int i = 0; i < components.Count; i++)
        {
            InitializeComponent(components[i]);
        }
    }

    private void InitializeComponent(Component component)
    {
        if (component.RegisteredWorld != this ||
            component.HasLifecycleFlag(LifecycleFlags.Initialized) ||
            component.HasLifecycleFlag(LifecycleFlags.Disposed))
        {
            return;
        }

        component.Initialize();
        component.MarkLifecycleFlag(LifecycleFlags.Initialized);
    }

    private void InitializeUpdateSystems(IReadOnlyList<UpdateSystem> systems)
    {
        for (int i = 0; i < systems.Count; i++)
        {
            InitializeUpdateSystem(systems[i]);
        }
    }

    private void InitializeUpdateSystem(UpdateSystem system)
    {
        if (system.RegisteredWorld != this ||
            system.HasLifecycleFlag(LifecycleFlags.Initialized) ||
            system.HasLifecycleFlag(LifecycleFlags.Disposed))
        {
            return;
        }

        system.Initialize();
        system.MarkLifecycleFlag(LifecycleFlags.Initialized);
    }

    private void InitializeRenderSystems(IReadOnlyList<RenderSystem> systems)
    {
        for (int i = 0; i < systems.Count; i++)
        {
            InitializeRenderSystem(systems[i]);
        }
    }

    private void InitializeRenderSystem(RenderSystem system)
    {
        if (system.RegisteredWorld != this ||
            system.HasLifecycleFlag(LifecycleFlags.Initialized) ||
            system.HasLifecycleFlag(LifecycleFlags.Disposed))
        {
            return;
        }

        system.Initialize();
        system.MarkLifecycleFlag(LifecycleFlags.Initialized);
    }

    private void StartActivatedBehaviorComponents(IReadOnlyList<Component> components)
    {
        for (int i = 0; i < components.Count; i++)
        {
            if (components[i] is BehaviorComponent behaviorComponent)
            {
                StartBehaviorComponent(behaviorComponent);
            }
        }
    }

    private void StartBehaviorComponents(IReadOnlyList<BehaviorComponent> behaviorComponents)
    {
        for (int i = 0; i < behaviorComponents.Count; i++)
        {
            StartBehaviorComponent(behaviorComponents[i]);
        }
    }

    private void StartBehaviorComponent(BehaviorComponent behaviorComponent)
    {
        if (behaviorComponent.RegisteredWorld != this ||
            !_behaviorRegistry.Contains(behaviorComponent) ||
            !behaviorComponent.HasLifecycleFlag(LifecycleFlags.Initialized) ||
            behaviorComponent.HasLifecycleFlag(LifecycleFlags.Started) ||
            behaviorComponent.HasLifecycleFlag(LifecycleFlags.Disposed))
        {
            return;
        }

        behaviorComponent.OnStart();
        behaviorComponent.MarkLifecycleFlag(LifecycleFlags.Started);
    }

    private void StartUpdateSystems(IReadOnlyList<UpdateSystem> systems)
    {
        for (int i = 0; i < systems.Count; i++)
        {
            StartUpdateSystem(systems[i]);
        }
    }

    private void StartUpdateSystem(UpdateSystem system)
    {
        if (system.RegisteredWorld != this ||
            !system.HasLifecycleFlag(LifecycleFlags.Initialized) ||
            system.HasLifecycleFlag(LifecycleFlags.Started) ||
            system.HasLifecycleFlag(LifecycleFlags.Disposed))
        {
            return;
        }

        system.OnStart();
        system.MarkLifecycleFlag(LifecycleFlags.Started);
    }

    private void StartRenderSystems(IReadOnlyList<RenderSystem> systems)
    {
        for (int i = 0; i < systems.Count; i++)
        {
            StartRenderSystem(systems[i]);
        }
    }

    private void StartRenderSystem(RenderSystem system)
    {
        if (system.RegisteredWorld != this ||
            !system.HasLifecycleFlag(LifecycleFlags.Initialized) ||
            system.HasLifecycleFlag(LifecycleFlags.Started) ||
            system.HasLifecycleFlag(LifecycleFlags.Disposed))
        {
            return;
        }

        system.OnStart();
        system.MarkLifecycleFlag(LifecycleFlags.Started);
    }

    private void UpdateSystemsByGroup(SystemGroup group)
    {
        for (int i = 0; i < _updateSysRegistry.Items.Count; i++)
        {
            UpdateSystem system = _updateSysRegistry.Items[i];

            if (system.Group != group || !system.HasLifecycleFlag(LifecycleFlags.Started))
            {
                continue;
            }

            system.Update();
        }
    }

    private void UpdateBehaviorComponents()
    {
        for (int i = 0; i < _behaviorRegistry.Items.Count; i++)
        {
            BehaviorComponent behaviorComponent = _behaviorRegistry.Items[i];

            if (!behaviorComponent.HasLifecycleFlag(LifecycleFlags.Started))
            {
                continue;
            }

            behaviorComponent.Update();
        }
    }

    private void DestroyBehaviorComponents(IReadOnlyList<BehaviorComponent> behaviorComponents)
    {
        for (int i = 0; i < behaviorComponents.Count; i++)
        {
            DestroyBehaviorComponent(behaviorComponents[i]);
        }
    }

    private void DestroyBehaviorComponent(BehaviorComponent behaviorComponent)
    {
        if (!behaviorComponent.HasLifecycleFlag(LifecycleFlags.Started) ||
            behaviorComponent.HasLifecycleFlag(LifecycleFlags.Destroyed))
        {
            return;
        }

        behaviorComponent.OnDestroy();
        behaviorComponent.MarkLifecycleFlag(LifecycleFlags.Destroyed);
    }

    private void DestroyUpdateSystems(IReadOnlyList<UpdateSystem> systems)
    {
        for (int i = 0; i < systems.Count; i++)
        {
            DestroyUpdateSystem(systems[i]);
        }
    }

    private void DestroyUpdateSystem(UpdateSystem system)
    {
        if (!system.HasLifecycleFlag(LifecycleFlags.Started) ||
            system.HasLifecycleFlag(LifecycleFlags.Destroyed))
        {
            return;
        }

        system.OnDestroy();
        system.MarkLifecycleFlag(LifecycleFlags.Destroyed);
    }

    private void DestroyRenderSystems(IReadOnlyList<RenderSystem> systems)
    {
        for (int i = 0; i < systems.Count; i++)
        {
            DestroyRenderSystem(systems[i]);
        }
    }

    private void DestroyRenderSystem(RenderSystem system)
    {
        if (!system.HasLifecycleFlag(LifecycleFlags.Started) ||
            system.HasLifecycleFlag(LifecycleFlags.Destroyed))
        {
            return;
        }

        system.OnDestroy();
        system.MarkLifecycleFlag(LifecycleFlags.Destroyed);
    }

    private void DisposeComponents(IReadOnlyList<Component> components)
    {
        for (int i = 0; i < components.Count; i++)
        {
            DisposeComponent(components[i]);
        }
    }

    private void DisposeComponent(Component component)
    {
        if (!component.HasLifecycleFlag(LifecycleFlags.Initialized) ||
            component.HasLifecycleFlag(LifecycleFlags.Disposed))
        {
            return;
        }

        component.Dispose();
        component.MarkLifecycleFlag(LifecycleFlags.Disposed);
    }

    private void DisposeUpdateSystems(IReadOnlyList<UpdateSystem> systems)
    {
        for (int i = 0; i < systems.Count; i++)
        {
            DisposeUpdateSystem(systems[i]);
        }
    }

    private void DisposeUpdateSystem(UpdateSystem system)
    {
        if (!system.HasLifecycleFlag(LifecycleFlags.Initialized) ||
            system.HasLifecycleFlag(LifecycleFlags.Disposed))
        {
            return;
        }

        system.Dispose();
        system.MarkLifecycleFlag(LifecycleFlags.Disposed);
    }

    private void DisposeRenderSystems(IReadOnlyList<RenderSystem> systems)
    {
        for (int i = 0; i < systems.Count; i++)
        {
            DisposeRenderSystem(systems[i]);
        }
    }

    private void DisposeRenderSystem(RenderSystem system)
    {
        if (!system.HasLifecycleFlag(LifecycleFlags.Initialized) ||
            system.HasLifecycleFlag(LifecycleFlags.Disposed))
        {
            return;
        }

        system.Dispose();
        system.MarkLifecycleFlag(LifecycleFlags.Disposed);
    }

    private void FlushRemovedComponent(Component component)
    {
        if (component is BehaviorComponent behaviorComponent)
        {
            DestroyBehaviorComponent(behaviorComponent);
        }

        DisposeComponent(component);
        component.SetRegisteredWorld(null);
    }

    private void FlushRemovedUpdateSystem(UpdateSystem system)
    {
        DestroyUpdateSystem(system);
        DisposeUpdateSystem(system);
        system.SetRegisteredWorld(null);

        if (system.World == this)
        {
            system.SetWorld(null);
        }
    }

    private void FlushRemovedRenderSystem(RenderSystem system)
    {
        DestroyRenderSystem(system);
        DisposeRenderSystem(system);
        system.SetRegisteredWorld(null);

        if (system.World == this)
        {
            system.SetWorld(null);
        }
    }

    private void EnsureState(WorldState expectedState, string operation)
    {
        if (_state != expectedState)
        {
            throw new InvalidOperationException(
                $"{operation} can only be called when the world is in the {expectedState} state. Current state: {_state}.");
        }
    }
}