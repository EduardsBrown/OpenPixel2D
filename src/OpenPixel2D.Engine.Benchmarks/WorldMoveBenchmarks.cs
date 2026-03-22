using BenchmarkDotNet.Attributes;
using OpenPixel2D.Engine.Benchmarks.Infrastructure;

namespace OpenPixel2D.Engine.Benchmarks;

[Config(typeof(EngineBenchmarkConfig))]
[BenchmarkCategory(BenchmarkCategories.World, BenchmarkCategories.Move)]
public class WorldMoveBenchmarks
{
    private World _sourceWorld = null!;
    private World _destinationWorld = null!;
    private Entity _subtree = null!;
    private Entity _sourceParent = null!;
    private Entity _destinationParent = null!;

    [Params(SubtreeShape.Deep, SubtreeShape.Wide, SubtreeShape.Mixed)]
    public SubtreeShape Shape { get; set; }

    [Params(WorldScale.Small, WorldScale.Medium, WorldScale.Large)]
    public WorldScale Scale { get; set; }

    [IterationSetup(Target = nameof(MoveSubtree_BetweenCreatedWorlds))]
    public void SetupMoveSubtreeBetweenCreatedWorlds()
    {
        _sourceWorld = new World();
        _destinationWorld = new World();
        _subtree = BenchmarkWorldFactory.CreateSubtree(Shape, Scale);
        _sourceWorld.AddEntity(_subtree);
    }

    [IterationCleanup(Target = nameof(MoveSubtree_BetweenCreatedWorlds))]
    public void CleanupMoveSubtreeBetweenCreatedWorlds()
    {
        BenchmarkCleanup.DisposeWorld(_sourceWorld);
        BenchmarkCleanup.DisposeWorld(_destinationWorld);
    }

    [IterationSetup(Target = nameof(MoveSubtree_BetweenStartedWorlds))]
    public void SetupMoveSubtreeBetweenStartedWorlds()
    {
        _sourceWorld = new World();
        _destinationWorld = BenchmarkWorldFactory.CreateStartedWorld();
        _subtree = BenchmarkWorldFactory.CreateSubtree(Shape, Scale);
        _sourceWorld.AddEntity(_subtree);
        _sourceWorld.Initialize();
        _sourceWorld.Start();
    }

    [IterationCleanup(Target = nameof(MoveSubtree_BetweenStartedWorlds))]
    public void CleanupMoveSubtreeBetweenStartedWorlds()
    {
        BenchmarkCleanup.DisposeWorld(_sourceWorld);
        BenchmarkCleanup.DisposeWorld(_destinationWorld);
    }

    [IterationSetup(Target = nameof(ReparentChild_SameWorld))]
    public void SetupReparentChildSameWorld()
    {
        _sourceWorld = new World();
        _sourceParent = new Entity();
        _destinationParent = new Entity();
        _subtree = BenchmarkWorldFactory.CreateSubtree(Shape, Scale);
        _sourceParent.AddEntity(_subtree);
        _sourceWorld.AddEntity(_sourceParent);
        _sourceWorld.AddEntity(_destinationParent);
        _sourceWorld.Initialize();
        _sourceWorld.Start();
    }

    [IterationCleanup(Target = nameof(ReparentChild_SameWorld))]
    public void CleanupReparentChildSameWorld()
    {
        BenchmarkCleanup.DisposeWorld(_sourceWorld);
    }

    [Benchmark(Baseline = true)]
    public void MoveSubtree_BetweenCreatedWorlds()
    {
        _destinationWorld.AddEntity(_subtree);
    }

    [Benchmark]
    public void MoveSubtree_BetweenStartedWorlds()
    {
        _destinationWorld.AddEntity(_subtree);
        _sourceWorld.Update();
        _destinationWorld.Update();
    }

    [Benchmark]
    public void ReparentChild_SameWorld()
    {
        _destinationParent.AddEntity(_subtree);
    }
}
