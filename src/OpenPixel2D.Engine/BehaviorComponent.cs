using OpenPixel2D.Abstractions;

namespace OpenPixel2D.Engine;

public abstract class BehaviorComponent : Component, IBehaviorComponent
{
    public virtual void Update()
    {
    }

    public virtual void OnDestroy()
    {
    }
}