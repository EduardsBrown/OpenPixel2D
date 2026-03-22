namespace OpenPixel2D.Abstractions;

public interface IComponent : IDisposable, IInitializable
{
    bool Active { get; }
}
