using BenchmarkDotNet.Attributes;
using OpenPixel2D.Engine.Benchmarks.Infrastructure;

namespace OpenPixel2D.Engine.Benchmarks;

[Config(typeof(FrameBenchmarkConfig))]
[BenchmarkCategory(BenchmarkCategories.World, BenchmarkCategories.Update)]
public class WorldUpdateBenchmarks
{
    private World _world = null!;

    [Params(WorldScale.Small, WorldScale.Medium, WorldScale.Large)]
    public WorldScale Scale { get; set; }

    [GlobalSetup(Target = nameof(Update_BehaviorsOnly))]
    public void SetupBehaviorsOnly()
    {
        _world = BenchmarkWorldFactory.CreateStartedWorld(LifecycleWorldComposition.BehaviorOnly, Scale);
    }

    [GlobalCleanup(Target = nameof(Update_BehaviorsOnly))]
    public void CleanupBehaviorsOnly()
    {
        BenchmarkCleanup.DisposeWorld(_world);
    }

    [GlobalSetup(Target = nameof(Update_UpdateSystemsOnly))]
    public void SetupUpdateSystemsOnly()
    {
        _world = BenchmarkWorldFactory.CreateStartedWorld(LifecycleWorldComposition.UpdateSystemsOnly, Scale);
    }

    [GlobalCleanup(Target = nameof(Update_UpdateSystemsOnly))]
    public void CleanupUpdateSystemsOnly()
    {
        BenchmarkCleanup.DisposeWorld(_world);
    }

    [GlobalSetup(Target = nameof(Update_MixedSteadyState))]
    public void SetupMixedSteadyState()
    {
        _world = BenchmarkWorldFactory.CreateStartedWorld(LifecycleWorldComposition.Mixed, Scale);
    }

    [GlobalCleanup(Target = nameof(Update_MixedSteadyState))]
    public void CleanupMixedSteadyState()
    {
        BenchmarkCleanup.DisposeWorld(_world);
    }

    [IterationSetup(Target = nameof(Update_PendingAdds))]
    public void SetupPendingAdds()
    {
        _world = BenchmarkWorldFactory.CreateStartedWorld(LifecycleWorldComposition.Mixed, Scale);
        BenchmarkWorldFactory.QueuePendingAdds(_world, Scale);
    }

    [IterationCleanup(Target = nameof(Update_PendingAdds))]
    public void CleanupPendingAdds()
    {
        BenchmarkCleanup.DisposeWorld(_world);
    }

    [IterationSetup(Target = nameof(Update_PendingRemoves))]
    public void SetupPendingRemoves()
    {
        _world = BenchmarkWorldFactory.CreateStartedWorld(LifecycleWorldComposition.Mixed, Scale);
        BenchmarkWorldFactory.QueuePendingRemoves(_world, Scale);
    }

    [IterationCleanup(Target = nameof(Update_PendingRemoves))]
    public void CleanupPendingRemoves()
    {
        BenchmarkCleanup.DisposeWorld(_world);
    }

    [IterationSetup(Target = nameof(Update_PendingMixedAddsAndRemoves))]
    public void SetupPendingMixedAddsAndRemoves()
    {
        _world = BenchmarkWorldFactory.CreateStartedWorld(LifecycleWorldComposition.Mixed, Scale);
        BenchmarkWorldFactory.QueuePendingMixedChanges(_world, Scale);
    }

    [IterationCleanup(Target = nameof(Update_PendingMixedAddsAndRemoves))]
    public void CleanupPendingMixedAddsAndRemoves()
    {
        BenchmarkCleanup.DisposeWorld(_world);
    }

    [Benchmark]
    public void Update_BehaviorsOnly()
    {
        _world.Update();
    }

    [Benchmark]
    public void Update_UpdateSystemsOnly()
    {
        _world.Update();
    }

    [Benchmark(Baseline = true)]
    public void Update_MixedSteadyState()
    {
        _world.Update();
    }

    [Benchmark]
    public void Update_PendingAdds()
    {
        _world.Update();
    }

    [Benchmark]
    public void Update_PendingRemoves()
    {
        _world.Update();
    }

    [Benchmark]
    public void Update_PendingMixedAddsAndRemoves()
    {
        _world.Update();
    }
}
