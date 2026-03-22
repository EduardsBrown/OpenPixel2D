namespace OpenPixel2D.Engine.Benchmarks.Infrastructure;

internal static class BenchmarkCleanup
{
    public static void DisposeWorld(World? world)
    {
        if (world is null)
        {
            return;
        }

        world.Dispose();
    }
}
