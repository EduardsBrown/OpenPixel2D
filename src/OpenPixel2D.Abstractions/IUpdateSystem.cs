namespace OpenPixel2D.Abstractions;

public interface IUpdateSystem : IDisposable, IAttachable, ICanUpdate, ICanInitialize
{
    SystemGroup Group { get; set; }
}