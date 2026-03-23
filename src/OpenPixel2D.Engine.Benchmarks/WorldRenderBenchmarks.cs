using BenchmarkDotNet.Attributes;
using OpenPixel2D.Engine.Benchmarks.Infrastructure;

namespace OpenPixel2D.Engine.Benchmarks;

[Config(typeof(FrameBenchmarkConfig))]
[BenchmarkCategory(BenchmarkCategories.World, BenchmarkCategories.Render)]
public class WorldRenderBenchmarks
{
    private World _world = null!;

    [Params(WorldScale.Small, WorldScale.Medium, WorldScale.Large)]
    public WorldScale Scale { get; set; }

    [GlobalSetup(Target = nameof(Render_RenderSystemsOnly))]
    public void SetupRenderSystemsOnly()
    {
        _world = BenchmarkWorldFactory.CreateStartedWorld(LifecycleWorldComposition.RenderSystemsOnly, Scale);
    }

    [GlobalCleanup(Target = nameof(Render_RenderSystemsOnly))]
    public void CleanupRenderSystemsOnly()
    {
        BenchmarkCleanup.DisposeWorld(_world);
    }

    [GlobalSetup(Target = nameof(Render_RuntimeActivatedSystems))]
    public void SetupRuntimeActivatedSystems()
    {
        _world = BenchmarkWorldFactory.CreateStartedWorld();
        BenchmarkWorldFactory.ActivateRuntimeRenderSystems(_world, Scale);
    }

    [GlobalCleanup(Target = nameof(Render_RuntimeActivatedSystems))]
    public void CleanupRuntimeActivatedSystems()
    {
        BenchmarkCleanup.DisposeWorld(_world);
    }

    [GlobalSetup(Target = nameof(Render_MixedSteadyState))]
    public void SetupMixedSteadyState()
    {
        _world = BenchmarkWorldFactory.CreateStartedWorld(LifecycleWorldComposition.Mixed, Scale);
    }

    [GlobalCleanup(Target = nameof(Render_MixedSteadyState))]
    public void CleanupMixedSteadyState()
    {
        BenchmarkCleanup.DisposeWorld(_world);
    }

    [Benchmark(Baseline = true)]
    public void Render_RenderSystemsOnly()
    {
        _world.Render(null);
    }

    [Benchmark]
    public void Render_RuntimeActivatedSystems()
    {
        _world.Render(null);
    }

    [Benchmark]
    public void Render_MixedSteadyState()
    {
        _world.Render(null);
    }
}
