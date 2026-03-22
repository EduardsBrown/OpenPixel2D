using BenchmarkDotNet.Attributes;
using OpenPixel2D.Engine.Benchmarks.Infrastructure;

namespace OpenPixel2D.Engine.Benchmarks;

[Config(typeof(EngineBenchmarkConfig))]
[BenchmarkCategory(BenchmarkCategories.World, BenchmarkCategories.Mutation)]
public class WorldSystemMutationBenchmarks
{
    private World _world = null!;
    private NoOpUpdateSystem _updateSystem = null!;
    private NoOpRenderSystem _renderSystem = null!;

    [IterationSetup(Target = nameof(AddUpdateSystem_CreatedWorld))]
    public void SetupAddUpdateSystemCreatedWorld()
    {
        _world = new World();
        _updateSystem = new NoOpUpdateSystem();
    }

    [IterationCleanup(Target = nameof(AddUpdateSystem_CreatedWorld))]
    public void CleanupAddUpdateSystemCreatedWorld()
    {
        BenchmarkCleanup.DisposeWorld(_world);
    }

    [IterationSetup(Target = nameof(AddUpdateSystem_StartedWorld))]
    public void SetupAddUpdateSystemStartedWorld()
    {
        _world = BenchmarkWorldFactory.CreateStartedWorld();
        _updateSystem = new NoOpUpdateSystem();
    }

    [IterationCleanup(Target = nameof(AddUpdateSystem_StartedWorld))]
    public void CleanupAddUpdateSystemStartedWorld()
    {
        BenchmarkCleanup.DisposeWorld(_world);
    }

    [IterationSetup(Target = nameof(RemoveUpdateSystem_CreatedWorld))]
    public void SetupRemoveUpdateSystemCreatedWorld()
    {
        _world = new World();
        _updateSystem = new NoOpUpdateSystem();
        _world.AddSystem(_updateSystem);
    }

    [IterationCleanup(Target = nameof(RemoveUpdateSystem_CreatedWorld))]
    public void CleanupRemoveUpdateSystemCreatedWorld()
    {
        BenchmarkCleanup.DisposeWorld(_world);
    }

    [IterationSetup(Target = nameof(RemoveUpdateSystem_StartedWorld))]
    public void SetupRemoveUpdateSystemStartedWorld()
    {
        _world = new World();
        _updateSystem = new NoOpUpdateSystem();
        _world.AddSystem(_updateSystem);
        _world.Initialize();
        _world.Start();
    }

    [IterationCleanup(Target = nameof(RemoveUpdateSystem_StartedWorld))]
    public void CleanupRemoveUpdateSystemStartedWorld()
    {
        BenchmarkCleanup.DisposeWorld(_world);
    }

    [IterationSetup(Target = nameof(AddRenderSystem_CreatedWorld))]
    public void SetupAddRenderSystemCreatedWorld()
    {
        _world = new World();
        _renderSystem = new NoOpRenderSystem();
    }

    [IterationCleanup(Target = nameof(AddRenderSystem_CreatedWorld))]
    public void CleanupAddRenderSystemCreatedWorld()
    {
        BenchmarkCleanup.DisposeWorld(_world);
    }

    [IterationSetup(Target = nameof(AddRenderSystem_StartedWorld))]
    public void SetupAddRenderSystemStartedWorld()
    {
        _world = BenchmarkWorldFactory.CreateStartedWorld();
        _renderSystem = new NoOpRenderSystem();
    }

    [IterationCleanup(Target = nameof(AddRenderSystem_StartedWorld))]
    public void CleanupAddRenderSystemStartedWorld()
    {
        BenchmarkCleanup.DisposeWorld(_world);
    }

    [IterationSetup(Target = nameof(RemoveRenderSystem_CreatedWorld))]
    public void SetupRemoveRenderSystemCreatedWorld()
    {
        _world = new World();
        _renderSystem = new NoOpRenderSystem();
        _world.AddSystem(_renderSystem);
    }

    [IterationCleanup(Target = nameof(RemoveRenderSystem_CreatedWorld))]
    public void CleanupRemoveRenderSystemCreatedWorld()
    {
        BenchmarkCleanup.DisposeWorld(_world);
    }

    [IterationSetup(Target = nameof(RemoveRenderSystem_StartedWorld))]
    public void SetupRemoveRenderSystemStartedWorld()
    {
        _world = new World();
        _renderSystem = new NoOpRenderSystem();
        _world.AddSystem(_renderSystem);
        _world.Initialize();
        _world.Start();
    }

    [IterationCleanup(Target = nameof(RemoveRenderSystem_StartedWorld))]
    public void CleanupRemoveRenderSystemStartedWorld()
    {
        BenchmarkCleanup.DisposeWorld(_world);
    }

    [Benchmark(Baseline = true)]
    public void AddUpdateSystem_CreatedWorld()
    {
        _world.AddSystem(_updateSystem);
    }

    [Benchmark]
    public void AddUpdateSystem_StartedWorld()
    {
        _world.AddSystem(_updateSystem);
    }

    [Benchmark]
    public void RemoveUpdateSystem_CreatedWorld()
    {
        _world.RemoveSystem(_updateSystem);
    }

    [Benchmark]
    public void RemoveUpdateSystem_StartedWorld()
    {
        _world.RemoveSystem(_updateSystem);
    }

    [Benchmark]
    public void AddRenderSystem_CreatedWorld()
    {
        _world.AddSystem(_renderSystem);
    }

    [Benchmark]
    public void AddRenderSystem_StartedWorld()
    {
        _world.AddSystem(_renderSystem);
    }

    [Benchmark]
    public void RemoveRenderSystem_CreatedWorld()
    {
        _world.RemoveSystem(_renderSystem);
    }

    [Benchmark]
    public void RemoveRenderSystem_StartedWorld()
    {
        _world.RemoveSystem(_renderSystem);
    }
}
