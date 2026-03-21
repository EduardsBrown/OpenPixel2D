using OpenPixel2D.Abstractions;

namespace OpenPixel2D.Engine;

public sealed class Entity
{
    private World? _world;

    public World World => _world ?? throw new InvalidOperationException("Entity is not attached to a world.");
    public Entity? Parent { get; private set; }

    private readonly List<Entity> _children = [];
    private readonly List<IComponent> _components = [];

    internal World? AttachedWorld => _world;

    public void AddComponent(IComponent component)
    {
        _components.Add(component);
        _world?.RegisterEntityComponent(this, component);
    }

    public void RemoveComponent(IComponent component)
    {
        if (!_components.Remove(component))
        {
            return;
        }

        _world?.UnregisterEntityComponent(this, component);
    }

    public void AddEntity(Entity entity)
    {
        if (ReferenceEquals(entity, this) || IsDescendantOf(entity))
        {
            throw new InvalidOperationException("An entity cannot be added to itself or one of its descendants.");
        }

        entity.DetachFromCurrentContainer();
        entity.Parent = this;
        _children.Add(entity);

        if (_world is not null)
        {
            entity.AttachToWorld(_world);
        }
    }

    public void RemoveEntity(Entity entity)
    {
        if (!DetachChild(entity))
        {
            return;
        }

        entity.Dispose();
    }

    internal void SetWorld(World world)
    {
        AttachToWorld(world);
    }

    public void Dispose()
    {
        DetachFromCurrentContainer();

        var children = _children.ToArray();
        _children.Clear();

        foreach (var child in children)
        {
            child.Parent = null;
            child.Dispose();
        }

        foreach (var component in _components)
        {
            component.Dispose();
        }

        _components.Clear();
        Parent = null;
    }

    internal void DetachFromCurrentContainer()
    {
        if (Parent is not null)
        {
            Parent.DetachChild(this);
            return;
        }

        _world?.DetachRootEntity(this);
        DetachFromWorld();
    }

    internal void AttachToWorld(World world)
    {
        var alreadyAttachedToTarget = ReferenceEquals(_world, world);

        if (_world is not null && !alreadyAttachedToTarget)
        {
            DetachFromWorld();
        }

        _world = world;

        if (!alreadyAttachedToTarget)
        {
            foreach (var component in _components)
            {
                world.RegisterEntityComponent(this, component);
            }
        }

        foreach (var child in _children)
        {
            child.AttachToWorld(world);
        }
    }

    internal void DetachFromWorld()
    {
        if (_world is null)
        {
            return;
        }

        var world = _world;

        foreach (var child in _children)
        {
            child.DetachFromWorld();
        }

        foreach (var component in _components)
        {
            world.UnregisterEntityComponent(this, component);
        }

        _world = null;
    }

    private bool DetachChild(Entity entity)
    {
        if (!_children.Remove(entity))
        {
            return false;
        }

        entity.Parent = null;
        entity.DetachFromWorld();
        return true;
    }

    private bool IsDescendantOf(Entity entity)
    {
        for (var current = Parent; current is not null; current = current.Parent)
        {
            if (ReferenceEquals(current, entity))
            {
                return true;
            }
        }

        return false;
    }
}
