using OpenPixel2D.Engine;
using OpenPixel2D.Rendering;
using OpenPixel2D.Rendering.Abstractions;

namespace OpenPixel2D.Runtime;

public sealed class EngineHost : IEngineHost
{
    private readonly RenderPipelineCoordinator _pipelineCoordinator;
    private readonly IRenderFrameExecutor _frameExecutor;
    private bool _disposed;

    public EngineHost(World world, IRenderFrameExecutor frameExecutor)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(frameExecutor);

        World = world;
        _frameExecutor = frameExecutor;
        _pipelineCoordinator = new RenderPipelineCoordinator();
    }

    public World World { get; }

    internal EngineTimeStep? LastUpdateTimeStep { get; private set; }

    internal EngineTimeStep? LastRenderTimeStep { get; private set; }

    public void Initialize()
    {
        EnsureNotDisposed(nameof(Initialize));
        World.Initialize();
    }

    public void Start()
    {
        EnsureNotDisposed(nameof(Start));
        World.Start();
    }

    public void Update(EngineTimeStep timeStep)
    {
        EnsureNotDisposed(nameof(Update));
        LastUpdateTimeStep = timeStep;
        World.Update();
    }

    public void Render(EngineTimeStep timeStep, IRenderView? view = null)
    {
        EnsureNotDisposed(nameof(Render));
        LastRenderTimeStep = timeStep;

        RenderFrame frame = view is null
            ? _pipelineCoordinator.BuildFrame(World)
            : _pipelineCoordinator.BuildFrame(World, view);

        _frameExecutor.Execute(frame, view);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            throw new InvalidOperationException("Dispose can only be called once.");
        }

        try
        {
            World.Dispose();
        }
        finally
        {
            if (_frameExecutor is IDisposable disposableExecutor)
            {
                disposableExecutor.Dispose();
            }

            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }

    private void EnsureNotDisposed(string operation)
    {
        if (_disposed)
        {
            throw new InvalidOperationException($"{operation} cannot be called after the engine host has been disposed.");
        }
    }
}
