namespace OpenPixel2D.Abstractions;

public interface IComponent : IDisposable, IInitializable, IStartable
{
    bool Active { get; }
}