using BenchmarkDotNet.Attributes;
using OpenPixel2D.Engine.Benchmarks.Infrastructure;

namespace OpenPixel2D.Engine.Benchmarks;

[Config(typeof(EngineBenchmarkConfig))]
[BenchmarkCategory(BenchmarkCategories.World, BenchmarkCategories.Mutation)]
public class WorldEntityMutationBenchmarks
{
    private World? _world;
    private Entity _root = null!;
    private Entity _parent = null!;
    private Entity _child = null!;

    [Params(EntityPayload.Empty, EntityPayload.PassivePayload, EntityPayload.BehaviorPayload, EntityPayload.NestedSubtree)]
    public EntityPayload Payload { get; set; }

    [Params(WorldScale.Small, WorldScale.Medium, WorldScale.Large)]
    public WorldScale Scale { get; set; }

    [IterationSetup(Target = nameof(AddRootEntity))]
    public void SetupAddRootEntity()
    {
        _world = new World();
        _root = BenchmarkWorldFactory.CreatePayloadEntity(Payload, Scale);
    }

    [IterationCleanup(Target = nameof(AddRootEntity))]
    public void CleanupAddRootEntity()
    {
        BenchmarkCleanup.DisposeWorld(_world);
    }

    [IterationSetup(Target = nameof(RemoveRootEntity))]
    public void SetupRemoveRootEntity()
    {
        _world = new World();
        _root = BenchmarkWorldFactory.CreatePayloadEntity(Payload, Scale);
        _world.AddEntity(_root);
        _world.Initialize();
        _world.Start();
    }

    [IterationCleanup(Target = nameof(RemoveRootEntity))]
    public void CleanupRemoveRootEntity()
    {
        BenchmarkCleanup.DisposeWorld(_world);
    }

    [IterationSetup(Target = nameof(AddChildEntity_DetachedParent))]
    public void SetupAddChildEntityDetachedParent()
    {
        _parent = new Entity();
        _child = BenchmarkWorldFactory.CreatePayloadEntity(Payload, Scale);
    }

    [IterationSetup(Target = nameof(AddChildEntity_StartedWorldParent))]
    public void SetupAddChildEntityStartedWorldParent()
    {
        _world = new World();
        _parent = new Entity();
        _child = BenchmarkWorldFactory.CreatePayloadEntity(Payload, Scale);
        _world.AddEntity(_parent);
        _world.Initialize();
        _world.Start();
    }

    [IterationCleanup(Target = nameof(AddChildEntity_StartedWorldParent))]
    public void CleanupAddChildEntityStartedWorldParent()
    {
        BenchmarkCleanup.DisposeWorld(_world);
    }

    [IterationSetup(Target = nameof(RemoveChildEntity_DetachedParent))]
    public void SetupRemoveChildEntityDetachedParent()
    {
        _parent = new Entity();
        _child = BenchmarkWorldFactory.CreatePayloadEntity(Payload, Scale);
        _parent.AddEntity(_child);
    }

    [IterationSetup(Target = nameof(RemoveChildEntity_StartedWorldParent))]
    public void SetupRemoveChildEntityStartedWorldParent()
    {
        _world = new World();
        _parent = new Entity();
        _child = BenchmarkWorldFactory.CreatePayloadEntity(Payload, Scale);
        _parent.AddEntity(_child);
        _world.AddEntity(_parent);
        _world.Initialize();
        _world.Start();
    }

    [IterationCleanup(Target = nameof(RemoveChildEntity_StartedWorldParent))]
    public void CleanupRemoveChildEntityStartedWorldParent()
    {
        BenchmarkCleanup.DisposeWorld(_world);
    }

    [Benchmark(Baseline = true)]
    public void AddRootEntity()
    {
        _world!.AddEntity(_root);
    }

    [Benchmark]
    public void RemoveRootEntity()
    {
        _world!.RemoveEntity(_root);
    }

    [Benchmark]
    public void AddChildEntity_DetachedParent()
    {
        _parent.AddEntity(_child);
    }

    [Benchmark]
    public void AddChildEntity_StartedWorldParent()
    {
        _parent.AddEntity(_child);
    }

    [Benchmark]
    public void RemoveChildEntity_DetachedParent()
    {
        _parent.RemoveEntity(_child);
    }

    [Benchmark]
    public void RemoveChildEntity_StartedWorldParent()
    {
        _parent.RemoveEntity(_child);
    }
}
