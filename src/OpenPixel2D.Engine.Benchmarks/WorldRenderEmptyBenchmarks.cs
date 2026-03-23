using BenchmarkDotNet.Attributes;
using OpenPixel2D.Engine.Benchmarks.Infrastructure;

namespace OpenPixel2D.Engine.Benchmarks;

[Config(typeof(FrameBenchmarkConfig))]
[BenchmarkCategory(BenchmarkCategories.World, BenchmarkCategories.Render)]
public class WorldRenderEmptyBenchmarks
{
    private World _world = null!;

    [GlobalSetup]
    public void Setup()
    {
        _world = BenchmarkWorldFactory.CreateStartedWorld();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        BenchmarkCleanup.DisposeWorld(_world);
    }

    [Benchmark(Baseline = true)]
    public void Render_EmptyWorld()
    {
        _world.Render(null);
    }
}
