namespace OpenPixel2D.Engine.Benchmarks.Infrastructure;

public enum WorldScale
{
    Small,
    Medium,
    Large
}

public enum EntityPayload
{
    Empty,
    PassivePayload,
    BehaviorPayload,
    NestedSubtree
}

public enum LifecycleWorldComposition
{
    PassiveOnly,
    BehaviorOnly,
    UpdateSystemsOnly,
    RenderSystemsOnly,
    Mixed
}

public enum SubtreeShape
{
    Deep,
    Wide,
    Mixed
}
