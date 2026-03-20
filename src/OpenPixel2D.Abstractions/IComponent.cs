namespace OpenPixel2D.Abstractions;

public interface IComponent : IDisposable
{
    bool Active { get; }
}