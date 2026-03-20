namespace OpenPixel2D.Abstractions;

public interface IUpdateSystem : IDisposable, IAttachable
{
    SystemGroup Group { get; set; }

    void Update();
}