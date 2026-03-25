namespace OpenPixel2D.Engine;

/// <summary>
/// Public runtime-driven timing facade for gameplay code. The runtime host updates this state before
/// <see cref="BehaviorComponent.OnStart"/> and each update frame, while runtime orchestration keeps its
/// own authoritative timestep model internally.
/// </summary>
public static class Time
{
    private static float _deltaTime;
    private static double _totalTime;
    private static ulong _frameCount;

    public static float DeltaTime => _deltaTime;

    public static double TotalTime => _totalTime;

    public static ulong FrameCount => _frameCount;

    internal static void Publish(float deltaTime, double totalTime, ulong frameCount)
    {
        if (deltaTime < 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(deltaTime), "Delta time cannot be negative.");
        }

        if (totalTime < 0d)
        {
            throw new ArgumentOutOfRangeException(nameof(totalTime), "Total time cannot be negative.");
        }

        _deltaTime = deltaTime;
        _totalTime = totalTime;
        _frameCount = frameCount;
    }

    internal static void Reset()
    {
        _deltaTime = 0f;
        _totalTime = 0d;
        _frameCount = 0UL;
    }
}
