namespace OpenPixel2D.Engine.Benchmarks.Infrastructure;

public sealed class FrameBenchmarkConfig : EngineBenchmarkConfig
{
    public FrameBenchmarkConfig()
    {
        AddColumn(new FramesPerSecondColumn());
    }
}
