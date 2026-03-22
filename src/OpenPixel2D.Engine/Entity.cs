namespace OpenPixel2D.Engine;

public sealed class Entity
{
    public Guid Id { get; set; }
    public World? World { get; private set; }
    public Entity? Parent { get; private set; }

    private List<Component> _components = [];
    private List<Entity> _children = [];

    internal void SetWorld(World world) => World = world;
    internal void SetParent(Entity parent) => Parent = parent;
}