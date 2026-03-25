namespace OpenPixel2D.Runtime;

public readonly record struct EngineTimeStep
{
    public EngineTimeStep(TimeSpan elapsedTime, TimeSpan totalTime)
    {
        if (elapsedTime < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(elapsedTime), "Elapsed time cannot be negative.");
        }

        if (totalTime < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(totalTime), "Total time cannot be negative.");
        }

        ElapsedTime = elapsedTime;
        TotalTime = totalTime;
    }

    public TimeSpan ElapsedTime { get; }

    public TimeSpan TotalTime { get; }
}
