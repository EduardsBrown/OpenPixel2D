namespace OpenPixel2D.Abstractions;

public interface IUpdateSystem : IDisposable, IInitializable, IStartable, IUpdatable, IDestroyable
{
    SystemGroup Group { get; }
}