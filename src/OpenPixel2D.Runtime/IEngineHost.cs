using OpenPixel2D.Engine;
using OpenPixel2D.Rendering.Abstractions;

namespace OpenPixel2D.Runtime;

public interface IEngineHost : IDisposable
{
    World World { get; }

    void Initialize();

    void Start();

    void Update(EngineTimeStep timeStep);

    void Render(EngineTimeStep timeStep, IRenderView? view = null);
}
