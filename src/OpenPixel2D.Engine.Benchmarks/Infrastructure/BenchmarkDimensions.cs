namespace OpenPixel2D.Engine.Benchmarks.Infrastructure;

internal static class BenchmarkDimensions
{
    public static int GetEntityCount(WorldScale scale) => scale switch
    {
        WorldScale.Small => 100,
        WorldScale.Medium => 1_000,
        WorldScale.Large => 10_000,
        _ => throw new ArgumentOutOfRangeException(nameof(scale), scale, null)
    };

    public static int GetSystemCount(WorldScale scale) => scale switch
    {
        WorldScale.Small => 10,
        WorldScale.Medium => 100,
        WorldScale.Large => 1_000,
        _ => throw new ArgumentOutOfRangeException(nameof(scale), scale, null)
    };

    public static int GetMixedSystemCount(WorldScale scale)
    {
        return Math.Max(1, GetEntityCount(scale) / 100);
    }
}
