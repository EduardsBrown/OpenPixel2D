using System.Drawing;
using OpenPixel2D.Engine;
using OpenPixel2D.Rendering.Abstractions;

namespace OpenPixel2D.Runtime.Tests;

public sealed class EngineHostTimeTests
{
    [Fact]
    public void Start_ExposesZeroTimeDuringOnStart()
    {
        World world = new();
        TimeProbeBehavior behavior = new();
        TimeProbeUpdateSystem updateSystem = new();
        Entity entity = new();
        entity.AddComponent(behavior);
        world.AddEntity(entity);
        world.AddSystem(updateSystem);
        EngineHost host = new(world, new NullFrameExecutor());

        host.Initialize();
        host.Start();

        TimeSnapshot expected = new(0f, 0d, 0UL);
        Assert.Equal(expected, behavior.OnStartSnapshot);
        Assert.Equal(expected, updateSystem.OnStartSnapshot);
        Assert.Equal(expected.DeltaTime, Time.DeltaTime);
        Assert.Equal(expected.TotalTime, Time.TotalTime);
        Assert.Equal(expected.FrameCount, Time.FrameCount);
    }

    [Fact]
    public void Update_PublishesStaticTimeBeforeGameplayCallbacks()
    {
        World world = new();
        TimeProbeBehavior behavior = new();
        TimeProbeUpdateSystem updateSystem = new();
        Entity entity = new();
        entity.AddComponent(behavior);
        world.AddEntity(entity);
        world.AddSystem(updateSystem);
        EngineHost host = new(world, new NullFrameExecutor());
        EngineTimeStep timeStep = new(TimeSpan.FromMilliseconds(16), TimeSpan.FromSeconds(2));

        host.Initialize();
        host.Start();

        host.Update(timeStep);

        TimeSnapshot expected = CreateSnapshot(timeStep, 1UL);
        Assert.Equal(expected, behavior.UpdateSnapshot);
        Assert.Equal(expected, updateSystem.UpdateSnapshot);
        Assert.Equal(expected.DeltaTime, Time.DeltaTime, 6);
        Assert.Equal(expected.TotalTime, Time.TotalTime, 6);
        Assert.Equal(expected.FrameCount, Time.FrameCount);
    }

    [Fact]
    public void LaterUpdates_OverwriteStaticTimeAndRenderDoesNotAdvanceIt()
    {
        World world = new();
        EngineHost host = new(world, new NullFrameExecutor());
        EngineTimeStep firstUpdate = new(TimeSpan.FromMilliseconds(16), TimeSpan.FromSeconds(1));
        EngineTimeStep renderStep = new(TimeSpan.FromMilliseconds(33), TimeSpan.FromSeconds(99));
        EngineTimeStep secondUpdate = new(TimeSpan.FromMilliseconds(20), TimeSpan.FromSeconds(3));

        host.Initialize();
        host.Start();

        host.Update(firstUpdate);

        Assert.Equal(CreateSnapshot(firstUpdate, 1UL), SnapshotCurrentTime());

        host.Render(renderStep);

        Assert.Equal(CreateSnapshot(firstUpdate, 1UL), SnapshotCurrentTime());

        host.Update(secondUpdate);

        Assert.Equal(CreateSnapshot(secondUpdate, 2UL), SnapshotCurrentTime());
    }

    [Fact]
    public void Initialize_ResetsStaticTimeAfterPriorRuntimeUse()
    {
        EngineHost firstHost = new(new World(), new NullFrameExecutor());
        EngineHost secondHost = new(new World(), new NullFrameExecutor());

        firstHost.Initialize();
        firstHost.Start();
        firstHost.Update(new EngineTimeStep(TimeSpan.FromMilliseconds(16), TimeSpan.FromSeconds(5)));

        Assert.NotEqual(0f, Time.DeltaTime);
        Assert.NotEqual(0d, Time.TotalTime);
        Assert.NotEqual(0UL, Time.FrameCount);

        secondHost.Initialize();

        Assert.Equal(0f, Time.DeltaTime);
        Assert.Equal(0d, Time.TotalTime);
        Assert.Equal(0UL, Time.FrameCount);
    }

    [Fact]
    public void Dispose_ResetsStaticTimeToDefaults()
    {
        EngineHost host = new(new World(), new NullFrameExecutor());

        host.Initialize();
        host.Start();
        host.Update(new EngineTimeStep(TimeSpan.FromMilliseconds(16), TimeSpan.FromSeconds(5)));

        host.Dispose();

        Assert.Equal(0f, Time.DeltaTime);
        Assert.Equal(0d, Time.TotalTime);
        Assert.Equal(0UL, Time.FrameCount);
    }

    private static TimeSnapshot CreateSnapshot(EngineTimeStep timeStep, ulong frameCount)
    {
        return new TimeSnapshot(
            (float)timeStep.ElapsedTime.TotalSeconds,
            timeStep.TotalTime.TotalSeconds,
            frameCount);
    }

    private static TimeSnapshot SnapshotCurrentTime()
    {
        return new TimeSnapshot(Time.DeltaTime, Time.TotalTime, Time.FrameCount);
    }

    private sealed class NullFrameExecutor : IRenderFrameExecutor
    {
        public void Execute(IRenderCompletedFrame frame, IRenderView? view)
        {
        }
    }

    private sealed class TimeProbeBehavior : BehaviorComponent
    {
        public TimeSnapshot? OnStartSnapshot { get; private set; }

        public TimeSnapshot? UpdateSnapshot { get; private set; }

        public override void OnStart()
        {
            OnStartSnapshot = SnapshotCurrentTime();
        }

        public override void Update()
        {
            UpdateSnapshot = SnapshotCurrentTime();
        }
    }

    private sealed class TimeProbeUpdateSystem : UpdateSystem
    {
        public TimeSnapshot? OnStartSnapshot { get; private set; }

        public TimeSnapshot? UpdateSnapshot { get; private set; }

        public override void OnStart()
        {
            OnStartSnapshot = SnapshotCurrentTime();
        }

        public override void Update()
        {
            UpdateSnapshot = SnapshotCurrentTime();
        }
    }

    private readonly record struct TimeSnapshot(float DeltaTime, double TotalTime, ulong FrameCount);
}
