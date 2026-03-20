namespace OpenPixel2D.Abstractions;

public interface IBehaviorComponent : IComponent, IAttachable
{
    void Update();
}