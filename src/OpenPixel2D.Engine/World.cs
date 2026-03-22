namespace OpenPixel2D.Engine;

public sealed class World : IDisposable
{
    private WorldState _state = WorldState.Created;

    private List<Entity> _entities = [];
    private AttachmentRegistry<Component> _componentRegistry = new();
    private AttachmentRegistry<BehaviorComponent> _behaviorRegistry = new();
    private AttachmentRegistry<UpdateSystem> _updateSysRegistry = new();
    private AttachmentRegistry<RenderSystem> _renderSysRegistry = new();

    public IReadOnlyList<Entity> Entities => _entities;
    public IReadOnlyList<UpdateSystem> UpdateSystems => _updateSysRegistry.Items;
    public IReadOnlyList<RenderSystem> RenderSystems => _renderSysRegistry.Items;

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
}