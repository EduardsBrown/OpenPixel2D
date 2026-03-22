using BenchmarkDotNet.Attributes;
using OpenPixel2D.Engine.Benchmarks.Infrastructure;

namespace OpenPixel2D.Engine.Benchmarks;

[Config(typeof(EngineBenchmarkConfig))]
[BenchmarkCategory(BenchmarkCategories.World, BenchmarkCategories.Lifecycle)]
public class WorldLifecycleBenchmarks
{
    private World _world = null!;

    [Params(LifecycleWorldComposition.PassiveOnly, LifecycleWorldComposition.BehaviorOnly, LifecycleWorldComposition.UpdateSystemsOnly, LifecycleWorldComposition.RenderSystemsOnly, LifecycleWorldComposition.Mixed)]
    public LifecycleWorldComposition Composition { get; set; }

    [Params(WorldScale.Small, WorldScale.Medium, WorldScale.Large)]
    public WorldScale Scale { get; set; }

    [IterationSetup(Target = nameof(Initialize_World))]
    public void SetupInitialize()
    {
        _world = BenchmarkWorldFactory.CreateWorld(Composition, Scale);
    }

    [IterationCleanup(Target = nameof(Initialize_World))]
    public void CleanupInitialize()
    {
        BenchmarkCleanup.DisposeWorld(_world);
    }

    [IterationSetup(Target = nameof(Start_World))]
    public void SetupStart()
    {
        _world = BenchmarkWorldFactory.CreateInitializedWorld(Composition, Scale);
    }

    [IterationCleanup(Target = nameof(Start_World))]
    public void CleanupStart()
    {
        BenchmarkCleanup.DisposeWorld(_world);
    }

    [IterationSetup(Target = nameof(Destroy_World))]
    public void SetupDestroy()
    {
        _world = BenchmarkWorldFactory.CreateStartedWorld(Composition, Scale);
    }

    [IterationCleanup(Target = nameof(Destroy_World))]
    public void CleanupDestroy()
    {
        BenchmarkCleanup.DisposeWorld(_world);
    }

    [IterationSetup(Target = nameof(Dispose_InitializedWorld))]
    public void SetupDispose()
    {
        _world = BenchmarkWorldFactory.CreateInitializedWorld(Composition, Scale);
    }

    [IterationSetup(Target = nameof(InitializeAndStart_World))]
    public void SetupInitializeAndStart()
    {
        _world = BenchmarkWorldFactory.CreateWorld(Composition, Scale);
    }

    [IterationCleanup(Target = nameof(InitializeAndStart_World))]
    public void CleanupInitializeAndStart()
    {
        BenchmarkCleanup.DisposeWorld(_world);
    }

    [IterationSetup(Target = nameof(DestroyAndDispose_World))]
    public void SetupDestroyAndDispose()
    {
        _world = BenchmarkWorldFactory.CreateStartedWorld(Composition, Scale);
    }

    [Benchmark(Baseline = true)]
    public void Initialize_World()
    {
        _world.Initialize();
    }

    [Benchmark]
    public void Start_World()
    {
        _world.Start();
    }

    [Benchmark]
    public void Destroy_World()
    {
        _world.Destroy();
    }

    [Benchmark]
    public void Dispose_InitializedWorld()
    {
        _world.Dispose();
    }

    [Benchmark]
    public void InitializeAndStart_World()
    {
        _world.Initialize();
        _world.Start();
    }

    [Benchmark]
    public void DestroyAndDispose_World()
    {
        _world.Destroy();
        _world.Dispose();
    }
}
