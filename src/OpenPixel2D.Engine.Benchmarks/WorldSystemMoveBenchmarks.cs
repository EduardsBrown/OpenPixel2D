using BenchmarkDotNet.Attributes;
using OpenPixel2D.Engine.Benchmarks.Infrastructure;

namespace OpenPixel2D.Engine.Benchmarks;

[Config(typeof(EngineBenchmarkConfig))]
[BenchmarkCategory(BenchmarkCategories.World, BenchmarkCategories.Move)]
public class WorldSystemMoveBenchmarks
{
    private World _sourceWorld = null!;
    private World _destinationWorld = null!;
    private NoOpUpdateSystem _updateSystem = null!;
    private NoOpRenderSystem _renderSystem = null!;

    [IterationSetup(Target = nameof(MoveUpdateSystem_BetweenStartedWorlds))]
    public void SetupMoveUpdateSystemBetweenStartedWorlds()
    {
        _sourceWorld = new World();
        _destinationWorld = BenchmarkWorldFactory.CreateStartedWorld();
        _updateSystem = new NoOpUpdateSystem();
        _sourceWorld.AddSystem(_updateSystem);
        _sourceWorld.Initialize();
        _sourceWorld.Start();
    }

    [IterationCleanup(Target = nameof(MoveUpdateSystem_BetweenStartedWorlds))]
    public void CleanupMoveUpdateSystemBetweenStartedWorlds()
    {
        BenchmarkCleanup.DisposeWorld(_sourceWorld);
        BenchmarkCleanup.DisposeWorld(_destinationWorld);
    }

    [IterationSetup(Target = nameof(MoveRenderSystem_BetweenStartedWorlds))]
    public void SetupMoveRenderSystemBetweenStartedWorlds()
    {
        _sourceWorld = new World();
        _destinationWorld = BenchmarkWorldFactory.CreateStartedWorld();
        _renderSystem = new NoOpRenderSystem();
        _sourceWorld.AddSystem(_renderSystem);
        _sourceWorld.Initialize();
        _sourceWorld.Start();
    }

    [IterationCleanup(Target = nameof(MoveRenderSystem_BetweenStartedWorlds))]
    public void CleanupMoveRenderSystemBetweenStartedWorlds()
    {
        BenchmarkCleanup.DisposeWorld(_sourceWorld);
        BenchmarkCleanup.DisposeWorld(_destinationWorld);
    }

    [Benchmark(Baseline = true)]
    public void MoveUpdateSystem_BetweenStartedWorlds()
    {
        _destinationWorld.AddSystem(_updateSystem);
        _sourceWorld.Update();
        _destinationWorld.Update();
    }

    [Benchmark]
    public void MoveRenderSystem_BetweenStartedWorlds()
    {
        _destinationWorld.AddSystem(_renderSystem);
        _sourceWorld.Update();
        _destinationWorld.Update();
    }
}
