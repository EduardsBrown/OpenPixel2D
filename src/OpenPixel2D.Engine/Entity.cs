namespace OpenPixel2D.Engine;

public sealed class Entity
{
    private readonly List<Component> _components = [];
    private readonly List<Entity> _children = [];

    public Guid Id { get; set; }
    public World? World { get; private set; }
    public Entity? Parent { get; private set; }
    public IReadOnlyList<Component> Components => _components;
    public IReadOnlyList<Entity> Children => _children;

    public void AddChild(Entity? child)
    {
        if (child == null || child == this || child.Parent == this || child.ContainsInSubtree(this))
        {
            return;
        }

        if (child.World == World)
        {
            child.DetachFromOwnerPreservingWorld();
            AddChildDirect(child);
            return;
        }

        child.DetachFromOwner();
        AddChildDirect(child);

        if (World == null)
        {
            return;
        }

        child.SetWorldRecursive(World);
        World.RegisterSubtree(child);
    }

    public void RemoveChild(Entity? child)
    {
        if (child == null || child.Parent != this)
        {
            return;
        }

        child.DetachFromOwner();
    }

    public void AddComponent(Component? component)
    {
        if (component == null || component.Parent == this)
        {
            return;
        }

        component.Parent?.RemoveComponent(component);

        _components.Add(component);
        component.SetParent(this);

        World?.RegisterComponent(component);
    }

    public void RemoveComponent(Component? component)
    {
        if (component == null || component.Parent != this)
        {
            return;
        }

        if (!_components.Remove(component))
        {
            return;
        }

        World?.UnregisterComponent(component);
        component.SetParent(null);
    }

    internal void DetachFromOwner()
    {
        World? currentWorld = World;

        if (Parent != null)
        {
            Parent.RemoveChildDirect(this);
        }
        else if (currentWorld != null)
        {
            currentWorld.RemoveRootDirect(this);
        }

        if (currentWorld == null)
        {
            return;
        }

        currentWorld.UnregisterSubtree(this);
        SetWorldRecursive(null);
    }

    internal void DetachFromOwnerPreservingWorld()
    {
        if (Parent != null)
        {
            Parent.RemoveChildDirect(this);
            return;
        }

        World?.RemoveRootDirect(this);
    }

    internal IEnumerable<Entity> EnumerateSubtree()
    {
        yield return this;

        for (int i = 0; i < _children.Count; i++)
        {
            foreach (Entity entity in _children[i].EnumerateSubtree())
            {
                yield return entity;
            }
        }
    }

    internal IEnumerable<Component> EnumerateSubtreeComponents()
    {
        foreach (Entity entity in EnumerateSubtree())
        {
            for (int i = 0; i < entity._components.Count; i++)
            {
                yield return entity._components[i];
            }
        }
    }

    internal bool ContainsInSubtree(Entity entity)
    {
        foreach (Entity descendant in EnumerateSubtree())
        {
            if (descendant == entity)
            {
                return true;
            }
        }

        return false;
    }

    internal void SetWorldRecursive(World? world)
    {
        foreach (Entity entity in EnumerateSubtree())
        {
            entity.SetWorld(world);
        }
    }

    internal void SetWorld(World? world) => World = world;
    internal void SetParent(Entity? parent) => Parent = parent;

    internal void AddChildDirect(Entity child)
    {
        _children.Add(child);
        child.SetParent(this);
    }

    internal void RemoveChildDirect(Entity child)
    {
        if (!_children.Remove(child))
        {
            return;
        }

        child.SetParent(null);
    }
}
