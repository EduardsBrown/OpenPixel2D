using OpenPixel2D.Abstractions;
using OpenPixel2D.Engine;
using OpenPixel2D.Rendering.Abstractions;

namespace OpenPixel2D.Engine.Tests;

internal sealed class LifecycleRecorder
{
    private readonly List<string> _events = [];

    public IReadOnlyList<string> Events => _events;

    public void Record(string owner, string eventName)
    {
        _events.Add($"{owner}.{eventName}");
    }

    public int Count(string entry)
    {
        return _events.Count(item => item == entry);
    }

    public int IndexOf(string entry)
    {
        return _events.IndexOf(entry);
    }

    public void Clear()
    {
        _events.Clear();
    }
}

internal sealed class RecordingComponent : Component
{
    private readonly string _name;
    private readonly LifecycleRecorder _recorder;
    private readonly Action? _onInitialize;
    private readonly Action? _onDispose;

    public RecordingComponent(
        string name,
        LifecycleRecorder recorder,
        Action? onInitialize = null,
        Action? onDispose = null)
    {
        _name = name;
        _recorder = recorder;
        _onInitialize = onInitialize;
        _onDispose = onDispose;
    }

    public override void Initialize()
    {
        _recorder.Record(_name, nameof(Initialize));
        _onInitialize?.Invoke();
    }

    public override void Dispose()
    {
        _recorder.Record(_name, nameof(Dispose));
        _onDispose?.Invoke();
    }
}

internal sealed class RecordingBehaviorComponent : BehaviorComponent
{
    private readonly string _name;
    private readonly LifecycleRecorder _recorder;
    private readonly Action? _onInitialize;
    private readonly Action? _onStart;
    private readonly Action? _onUpdate;
    private readonly Action? _onDestroy;
    private readonly Action? _onDispose;

    public RecordingBehaviorComponent(
        string name,
        LifecycleRecorder recorder,
        Action? onInitialize = null,
        Action? onStart = null,
        Action? onUpdate = null,
        Action? onDestroy = null,
        Action? onDispose = null)
    {
        _name = name;
        _recorder = recorder;
        _onInitialize = onInitialize;
        _onStart = onStart;
        _onUpdate = onUpdate;
        _onDestroy = onDestroy;
        _onDispose = onDispose;
    }

    public override void Initialize()
    {
        _recorder.Record(_name, nameof(Initialize));
        _onInitialize?.Invoke();
    }

    public override void OnStart()
    {
        _recorder.Record(_name, nameof(OnStart));
        _onStart?.Invoke();
    }

    public override void Update()
    {
        _recorder.Record(_name, nameof(Update));
        _onUpdate?.Invoke();
    }

    public override void OnDestroy()
    {
        _recorder.Record(_name, nameof(OnDestroy));
        _onDestroy?.Invoke();
    }

    public override void Dispose()
    {
        _recorder.Record(_name, nameof(Dispose));
        _onDispose?.Invoke();
    }
}

internal sealed class RecordingUpdateSystem : UpdateSystem
{
    private readonly string _name;
    private readonly LifecycleRecorder _recorder;
    private readonly Action? _onInitialize;
    private readonly Action? _onStart;
    private readonly Action? _onUpdate;
    private readonly Action? _onDestroy;
    private readonly Action? _onDispose;

    public RecordingUpdateSystem(
        string name,
        LifecycleRecorder recorder,
        SystemGroup group = SystemGroup.Default,
        Action? onInitialize = null,
        Action? onStart = null,
        Action? onUpdate = null,
        Action? onDestroy = null,
        Action? onDispose = null)
    {
        _name = name;
        _recorder = recorder;
        _onInitialize = onInitialize;
        _onStart = onStart;
        _onUpdate = onUpdate;
        _onDestroy = onDestroy;
        _onDispose = onDispose;
        Group = group;
    }

    public override void Initialize()
    {
        _recorder.Record(_name, nameof(Initialize));
        _onInitialize?.Invoke();
    }

    public override void OnStart()
    {
        _recorder.Record(_name, nameof(OnStart));
        _onStart?.Invoke();
    }

    public override void Update()
    {
        _recorder.Record(_name, nameof(Update));
        _onUpdate?.Invoke();
    }

    public override void OnDestroy()
    {
        _recorder.Record(_name, nameof(OnDestroy));
        _onDestroy?.Invoke();
    }

    public override void Dispose()
    {
        _recorder.Record(_name, nameof(Dispose));
        _onDispose?.Invoke();
    }
}

internal sealed class RecordingRenderSystem : RenderSystem
{
    private readonly string _name;
    private readonly LifecycleRecorder _recorder;
    private readonly Action? _onInitialize;
    private readonly Action? _onStart;
    private readonly Action? _onRender;
    private readonly Action? _onDestroy;
    private readonly Action? _onDispose;

    public RecordingRenderSystem(
        string name,
        LifecycleRecorder recorder,
        Action? onInitialize = null,
        Action? onStart = null,
        Action? onRender = null,
        Action? onDestroy = null,
        Action? onDispose = null)
    {
        _name = name;
        _recorder = recorder;
        _onInitialize = onInitialize;
        _onStart = onStart;
        _onRender = onRender;
        _onDestroy = onDestroy;
        _onDispose = onDispose;
    }

    public override void Initialize()
    {
        _recorder.Record(_name, nameof(Initialize));
        _onInitialize?.Invoke();
    }

    public override void OnStart()
    {
        _recorder.Record(_name, nameof(OnStart));
        _onStart?.Invoke();
    }

    public override void Render(IRenderContext context)
    {
        _recorder.Record(_name, nameof(Render));
        _onRender?.Invoke();
    }

    public override void OnDestroy()
    {
        _recorder.Record(_name, nameof(OnDestroy));
        _onDestroy?.Invoke();
    }

    public override void Dispose()
    {
        _recorder.Record(_name, nameof(Dispose));
        _onDispose?.Invoke();
    }
}