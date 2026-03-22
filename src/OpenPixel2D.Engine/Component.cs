using System.Text.Json.Serialization;
using OpenPixel2D.Abstractions;

namespace OpenPixel2D.Engine;

public abstract class Component : IComponent
{
    public bool Active { get; set; } = true;

    [JsonIgnore]
    public Entity? Parent { get; private set; }

    internal void SetParent(Entity? parent) => Parent = parent;

    public virtual void Initialize()
    {
    }

    public virtual void OnStart()
    {
    }

    public virtual void Dispose()
    {
    }
}
