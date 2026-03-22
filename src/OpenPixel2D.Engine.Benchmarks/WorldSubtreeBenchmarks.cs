using BenchmarkDotNet.Attributes;
using OpenPixel2D.Engine.Benchmarks.Infrastructure;

namespace OpenPixel2D.Engine.Benchmarks;

[Config(typeof(EngineBenchmarkConfig))]
[BenchmarkCategory(BenchmarkCategories.World, BenchmarkCategories.Subtree)]
public class WorldSubtreeBenchmarks
{
    private World _world = null!;
    private Entity _root = null!;

    [Params(SubtreeShape.Deep, SubtreeShape.Wide, SubtreeShape.Mixed)]
    public SubtreeShape Shape { get; set; }

    [Params(WorldScale.Small, WorldScale.Medium, WorldScale.Large)]
    public WorldScale Scale { get; set; }

    [IterationSetup(Target = nameof(AttachSubtree))]
    public void SetupAttachSubtree()
    {
        _world = new World();
        _root = BenchmarkWorldFactory.CreateSubtree(Shape, Scale);
    }

    [IterationCleanup(Target = nameof(AttachSubtree))]
    public void CleanupAttachSubtree()
    {
        BenchmarkCleanup.DisposeWorld(_world);
    }

    [IterationSetup(Target = nameof(DetachSubtree))]
    public void SetupDetachSubtree()
    {
        _world = new World();
        _root = BenchmarkWorldFactory.CreateSubtree(Shape, Scale);
        _world.AddEntity(_root);
        _world.Initialize();
        _world.Start();
    }

    [IterationCleanup(Target = nameof(DetachSubtree))]
    public void CleanupDetachSubtree()
    {
        BenchmarkCleanup.DisposeWorld(_world);
    }

    [Benchmark(Baseline = true)]
    public void AttachSubtree()
    {
        _world.AddEntity(_root);
    }

    [Benchmark]
    public void DetachSubtree()
    {
        _world.RemoveEntity(_root);
    }
}
