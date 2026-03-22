using BenchmarkDotNet.Attributes;
using OpenPixel2D.Engine.Benchmarks.Infrastructure;

namespace OpenPixel2D.Engine.Benchmarks;

[Config(typeof(EngineBenchmarkConfig))]
[BenchmarkCategory(BenchmarkCategories.World, BenchmarkCategories.Mutation)]
public class WorldComponentMutationBenchmarks
{
    private World? _world;
    private Entity _entity = null!;
    private NoOpComponent _passiveComponent = null!;
    private NoOpBehaviorComponent _behaviorComponent = null!;

    [IterationSetup(Target = nameof(AddPassiveComponent_DetachedEntity))]
    public void SetupAddPassiveComponentDetachedEntity()
    {
        _entity = new Entity();
        _passiveComponent = new NoOpComponent();
    }

    [IterationSetup(Target = nameof(AddPassiveComponent_StartedWorldEntity))]
    public void SetupAddPassiveComponentStartedWorldEntity()
    {
        _world = new World();
        _entity = new Entity();
        _passiveComponent = new NoOpComponent();
        _world.AddEntity(_entity);
        _world.Initialize();
        _world.Start();
    }

    [IterationCleanup(Target = nameof(AddPassiveComponent_StartedWorldEntity))]
    public void CleanupAddPassiveComponentStartedWorldEntity()
    {
        BenchmarkCleanup.DisposeWorld(_world);
    }

    [IterationSetup(Target = nameof(RemovePassiveComponent_DetachedEntity))]
    public void SetupRemovePassiveComponentDetachedEntity()
    {
        _entity = new Entity();
        _passiveComponent = new NoOpComponent();
        _entity.AddComponent(_passiveComponent);
    }

    [IterationSetup(Target = nameof(RemovePassiveComponent_StartedWorldEntity))]
    public void SetupRemovePassiveComponentStartedWorldEntity()
    {
        _world = new World();
        _entity = new Entity();
        _passiveComponent = new NoOpComponent();
        _entity.AddComponent(_passiveComponent);
        _world.AddEntity(_entity);
        _world.Initialize();
        _world.Start();
    }

    [IterationCleanup(Target = nameof(RemovePassiveComponent_StartedWorldEntity))]
    public void CleanupRemovePassiveComponentStartedWorldEntity()
    {
        BenchmarkCleanup.DisposeWorld(_world);
    }

    [IterationSetup(Target = nameof(AddBehaviorComponent_DetachedEntity))]
    public void SetupAddBehaviorComponentDetachedEntity()
    {
        _entity = new Entity();
        _behaviorComponent = new NoOpBehaviorComponent();
    }

    [IterationSetup(Target = nameof(AddBehaviorComponent_StartedWorldEntity))]
    public void SetupAddBehaviorComponentStartedWorldEntity()
    {
        _world = new World();
        _entity = new Entity();
        _behaviorComponent = new NoOpBehaviorComponent();
        _world.AddEntity(_entity);
        _world.Initialize();
        _world.Start();
    }

    [IterationCleanup(Target = nameof(AddBehaviorComponent_StartedWorldEntity))]
    public void CleanupAddBehaviorComponentStartedWorldEntity()
    {
        BenchmarkCleanup.DisposeWorld(_world);
    }

    [IterationSetup(Target = nameof(RemoveBehaviorComponent_DetachedEntity))]
    public void SetupRemoveBehaviorComponentDetachedEntity()
    {
        _entity = new Entity();
        _behaviorComponent = new NoOpBehaviorComponent();
        _entity.AddComponent(_behaviorComponent);
    }

    [IterationSetup(Target = nameof(RemoveBehaviorComponent_StartedWorldEntity))]
    public void SetupRemoveBehaviorComponentStartedWorldEntity()
    {
        _world = new World();
        _entity = new Entity();
        _behaviorComponent = new NoOpBehaviorComponent();
        _entity.AddComponent(_behaviorComponent);
        _world.AddEntity(_entity);
        _world.Initialize();
        _world.Start();
    }

    [IterationCleanup(Target = nameof(RemoveBehaviorComponent_StartedWorldEntity))]
    public void CleanupRemoveBehaviorComponentStartedWorldEntity()
    {
        BenchmarkCleanup.DisposeWorld(_world);
    }

    [Benchmark(Baseline = true)]
    public void AddPassiveComponent_DetachedEntity()
    {
        _entity.AddComponent(_passiveComponent);
    }

    [Benchmark]
    public void AddPassiveComponent_StartedWorldEntity()
    {
        _entity.AddComponent(_passiveComponent);
    }

    [Benchmark]
    public void RemovePassiveComponent_DetachedEntity()
    {
        _entity.RemoveComponent(_passiveComponent);
    }

    [Benchmark]
    public void RemovePassiveComponent_StartedWorldEntity()
    {
        _entity.RemoveComponent(_passiveComponent);
    }

    [Benchmark]
    public void AddBehaviorComponent_DetachedEntity()
    {
        _entity.AddComponent(_behaviorComponent);
    }

    [Benchmark]
    public void AddBehaviorComponent_StartedWorldEntity()
    {
        _entity.AddComponent(_behaviorComponent);
    }

    [Benchmark]
    public void RemoveBehaviorComponent_DetachedEntity()
    {
        _entity.RemoveComponent(_behaviorComponent);
    }

    [Benchmark]
    public void RemoveBehaviorComponent_StartedWorldEntity()
    {
        _entity.RemoveComponent(_behaviorComponent);
    }
}
