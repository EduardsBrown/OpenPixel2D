namespace OpenPixel2D.Abstractions;

public interface IComponent : IDisposable, ICanInitialize
{
    bool Active { get; }
}