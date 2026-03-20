namespace OpenPixel2D.Abstractions;

public interface IBehaviorComponent : IComponent
{
    void Initialize();
    void OnStart();
    void Update();
}