using OpenPixel2D.Engine;
using OpenPixel2D.Rendering;
using OpenPixel2D.Rendering.Abstractions;

namespace OpenPixel2D.Runtime;

/// <summary>
/// Minimal backend-neutral runtime composition root. The host owns lifecycle orchestration, publishes
/// the engine-facing static <see cref="Time"/> facade from authoritative runtime timing, coordinates
/// frame construction, and delegates final execution to the backend.
/// </summary>
public sealed class EngineHost : IEngineHost
{
    private readonly RenderPipelineCoordinator _pipelineCoordinator;
    private readonly IRenderFrameExecutor _frameExecutor;
    private EngineHostState _state = EngineHostState.Created;
    private ulong _updateFrameCount;

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
        EnsureState(EngineHostState.Created, nameof(Initialize));

        ResetTimingState();
        World.Initialize();
        _state = EngineHostState.Initialized;
    }

    public void Start()
    {
        EnsureState(EngineHostState.Initialized, nameof(Start));
        PublishStartupTime();
        World.Start();
        _state = EngineHostState.Started;
    }

    public void Update(EngineTimeStep timeStep)
    {
        EnsureState(EngineHostState.Started, nameof(Update));
        LastUpdateTimeStep = timeStep;
        _updateFrameCount++;
        PublishTime(timeStep, _updateFrameCount);
        World.Update();
    }

    public void Render(EngineTimeStep timeStep, IRenderView? view = null)
    {
        EnsureState(EngineHostState.Started, nameof(Render));
        LastRenderTimeStep = timeStep;

        RenderFrame frame = view is null
            ? _pipelineCoordinator.BuildFrame(World)
            : _pipelineCoordinator.BuildFrame(World, view);

        _frameExecutor.Execute(frame, view);
    }

    public void Dispose()
    {
        if (_state == EngineHostState.Disposed)
        {
            throw new InvalidOperationException("Dispose can only be called once.");
        }

        try
        {
            World.Dispose();
        }
        finally
        {
            ResetTimingState();

            if (_frameExecutor is IDisposable disposableExecutor)
            {
                disposableExecutor.Dispose();
            }

            _state = EngineHostState.Disposed;
            GC.SuppressFinalize(this);
        }
    }

    private void EnsureState(EngineHostState expectedState, string operation)
    {
        if (_state != expectedState)
        {
            throw new InvalidOperationException(
                $"{operation} can only be called when the engine host is in the {expectedState} state. Current state: {_state}.");
        }
    }

    private void ResetTimingState()
    {
        LastUpdateTimeStep = null;
        LastRenderTimeStep = null;
        _updateFrameCount = 0UL;
        Time.Reset();
    }

    private static void PublishStartupTime()
    {
        Time.Reset();
    }

    private static void PublishTime(EngineTimeStep timeStep, ulong frameCount)
    {
        Time.Publish(
            (float)timeStep.ElapsedTime.TotalSeconds,
            timeStep.TotalTime.TotalSeconds,
            frameCount);
    }

    private enum EngineHostState
    {
        Created,
        Initialized,
        Started,
        Disposed
    }
}
