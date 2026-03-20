namespace OpenPixel2D.Abstractions;

public interface IUpdateSystem : IDisposable
{
    SystemGroup Group { get; set; }

    void Initialize();
    void OnStart();
    void Update();
}