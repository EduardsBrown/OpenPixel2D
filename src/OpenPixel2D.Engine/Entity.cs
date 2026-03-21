using OpenPixel2D.Abstractions;

namespace OpenPixel2D.Engine;

public sealed class Entity
{
    public World World { get; private set; }
    public Entity? Parent { get; private set; } = null;

    private List<Entity> _children = [];
    private List<IComponent> _components = [];

    public void AddComponent(IComponent component)
    {
        _components.Add(component);
        World.RegisterEntityComponent(this, component);
    }

    public void RemoveComponent(IComponent component)
    {
        _components.Remove(component);
        World.UnregisterEntityComponent(this, component);
    }

    public void AddEntity(Entity entity)
    {
        if (entity.Parent != null)
        {
            entity.Parent.RemoveEntity(entity);
        }

        entity.Parent = this;
        entity.World = World;
        _children.Add(entity);
    }

    public void RemoveEntity(Entity entity)
    {
        _children.Remove(entity);
    }

    internal void SetWorld(World world)
    {
        World = world;
    }

    public void Dispose()
    {
        World = null;
    }
}