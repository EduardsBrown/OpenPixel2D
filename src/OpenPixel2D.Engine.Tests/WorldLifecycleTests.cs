using OpenPixel2D.Abstractions;
using OpenPixel2D.Engine;

namespace OpenPixel2D.Engine.Tests;

public sealed class WorldLifecycleTests
{
    [Fact]
    public void Initialize_ActivatesStartupObjectsWithoutImmediateLifecycle()
    {
        World world = new();
        List<string> log = [];
        Entity entity = new();
        SpyComponent component = new("component", log);
        SpyBehaviorComponent behavior = new("behavior", log);
        SpyUpdateSystem updateSystem = new("update", log);
        SpyRenderSystem renderSystem = new("render", log);

        entity.AddComponent(component);
        entity.AddComponent(behavior);
        world.AddEntity(entity);
        world.AddSystem(updateSystem);
        world.AddSystem(renderSystem);

        Assert.Empty(log);

        world.Initialize();

        Assert.Equal(
            [
                "component.Initialize",
                "behavior.Initialize",
                "update.Initialize",
                "render.Initialize"
            ],
            log);
    }

    [Fact]
    public void Start_InitializesLatePreStartObjectsThenStartsThem()
    {
        World world = new();
        List<string> log = [];
        Entity entity = new();
        SpyComponent component = new("component", log);
        SpyBehaviorComponent behavior = new("behavior", log);
        SpyUpdateSystem updateSystem = new("update", log);
        SpyRenderSystem renderSystem = new("render", log);

        world.Initialize();

        entity.AddComponent(component);
        entity.AddComponent(behavior);
        world.AddEntity(entity);
        world.AddSystem(updateSystem);
        world.AddSystem(renderSystem);

        Assert.Empty(log);

        world.Start();

        Assert.Equal(
            [
                "component.Initialize",
                "behavior.Initialize",
                "update.Initialize",
                "render.Initialize",
                "behavior.OnStart",
                "update.OnStart",
                "render.OnStart"
            ],
            log);
    }

    [Fact]
    public void Update_ActivatesRuntimeAdditionsAtStartOfNextFrame()
    {
        List<string> log = [];
        Entity entity = new();
        World world = CreateStartedWorld(entity);
        SpyBehaviorComponent behavior = new("behavior", log);
        SpyUpdateSystem updateSystem = new("update", log);
        SpyRenderSystem renderSystem = new("render", log);

        entity.AddComponent(behavior);
        world.AddSystem(updateSystem);
        world.AddSystem(renderSystem);

        Assert.Empty(log);
        Assert.Empty(world.RegisteredBehaviorComponents);
        Assert.Empty(world.UpdateSystems);
        Assert.Empty(world.RenderSystems);

        world.Render();

        Assert.Empty(log);

        world.Update();

        Assert.Equal(
            [
                "behavior.Initialize",
                "update.Initialize",
                "render.Initialize",
                "behavior.OnStart",
                "update.OnStart",
                "render.OnStart",
                "update.Update",
                "behavior.Update"
            ],
            log);

        world.Render();

        Assert.Equal(
            [
                "behavior.Initialize",
                "update.Initialize",
                "render.Initialize",
                "behavior.OnStart",
                "update.OnStart",
                "render.OnStart",
                "update.Update",
                "behavior.Update",
                "render.Render"
            ],
            log);
    }

    [Fact]
    public void Update_DoesNotActivateObjectsAddedDuringCurrentFrameUntilNextFrame()
    {
        List<string> spawnedLog = [];
        Entity entity = new();
        SpyBehaviorComponent spawnedBehavior = new("behavior", spawnedLog);
        SpyUpdateSystem spawnedUpdateSystem = new("update", spawnedLog);
        SpyRenderSystem spawnedRenderSystem = new("render", spawnedLog);
        SpyUpdateSystem? spawner = null;
        spawner = new SpyUpdateSystem(
            "spawner",
            [],
            onUpdate: () =>
            {
                entity.AddComponent(spawnedBehavior);
                spawner!.World!.AddSystem(spawnedUpdateSystem);
                spawner.World.AddSystem(spawnedRenderSystem);
            });

        World world = new();
        world.AddEntity(entity);
        world.AddSystem(spawner);
        world.Initialize();
        world.Start();

        world.Update();
        world.Render();

        Assert.Empty(spawnedLog);

        world.Update();

        Assert.Equal(
            [
                "behavior.Initialize",
                "update.Initialize",
                "render.Initialize",
                "behavior.OnStart",
                "update.OnStart",
                "render.OnStart",
                "update.Update",
                "behavior.Update"
            ],
            spawnedLog);

        world.Render();

        Assert.Equal(
            [
                "behavior.Initialize",
                "update.Initialize",
                "render.Initialize",
                "behavior.OnStart",
                "update.OnStart",
                "render.OnStart",
                "update.Update",
                "behavior.Update",
                "render.Render"
            ],
            spawnedLog);
    }

    [Fact]
    public void UpdateFlush_RemovesStartedObjectsWithDestroyThenDispose()
    {
        List<string> log = [];
        Entity entity = new();
        SpyBehaviorComponent behavior = new("behavior", log);
        SpyUpdateSystem updateSystem = new("update", log);
        SpyRenderSystem renderSystem = new("render", log);
        entity.AddComponent(behavior);

        World world = new();
        world.AddEntity(entity);
        world.AddSystem(updateSystem);
        world.AddSystem(renderSystem);
        world.Initialize();
        world.Start();
        log.Clear();

        entity.RemoveComponent(behavior);
        world.RemoveSystem(updateSystem);
        world.RemoveSystem(renderSystem);

        world.Render();

        Assert.Equal(["render.Render"], log);

        world.Update();
        world.Render();

        Assert.Equal(
            [
                "render.Render",
                "behavior.OnDestroy",
                "behavior.Dispose",
                "update.OnDestroy",
                "update.Dispose",
                "render.OnDestroy",
                "render.Dispose"
            ],
            log);
    }

    [Fact]
    public void Start_DisposesInitializedButNeverStartedObjectsRemovedBeforeStart()
    {
        World world = new();
        List<string> log = [];
        Entity entity = new();
        SpyBehaviorComponent behavior = new("behavior", log);
        SpyUpdateSystem updateSystem = new("update", log);
        SpyRenderSystem renderSystem = new("render", log);

        entity.AddComponent(behavior);
        world.AddEntity(entity);
        world.AddSystem(updateSystem);
        world.AddSystem(renderSystem);
        world.Initialize();
        log.Clear();

        entity.RemoveComponent(behavior);
        world.RemoveSystem(updateSystem);
        world.RemoveSystem(renderSystem);

        world.Start();

        Assert.Equal(
            [
                "behavior.Dispose",
                "update.Dispose",
                "render.Dispose"
            ],
            log);
    }

    [Fact]
    public void AddThenRemoveBeforeActivation_ReceivesNoLifecycle()
    {
        List<string> log = [];
        Entity entity = new();
        World world = CreateStartedWorld(entity);
        SpyBehaviorComponent behavior = new("behavior", log);
        SpyUpdateSystem updateSystem = new("update", log);
        SpyRenderSystem renderSystem = new("render", log);

        entity.AddComponent(behavior);
        world.AddSystem(updateSystem);
        world.AddSystem(renderSystem);

        entity.RemoveComponent(behavior);
        world.RemoveSystem(updateSystem);
        world.RemoveSystem(renderSystem);

        world.Update();
        world.Render();

        Assert.Empty(log);
    }

    [Fact]
    public void Update_UsesDefaultBehaviorPhysicsThenPostPhysicsOrder()
    {
        List<string> log = [];
        Entity entity = new();
        SpyBehaviorComponent behavior = new("behavior", log);
        SpyUpdateSystem defaultSystem = new("default", log);
        SpyUpdateSystem physicsSystem = new("physics", log, SystemGroup.Physics);
        SpyUpdateSystem postPhysicsSystem = new("post", log, SystemGroup.PostPhysics);
        entity.AddComponent(behavior);

        World world = new();
        world.AddEntity(entity);
        world.AddSystem(defaultSystem);
        world.AddSystem(physicsSystem);
        world.AddSystem(postPhysicsSystem);
        world.Initialize();
        world.Start();
        log.Clear();

        world.Update();

        Assert.Equal(
            [
                "default.Update",
                "behavior.Update",
                "physics.Update",
                "post.Update"
            ],
            log);
    }

    [Fact]
    public void InvalidWorldLifecycleCalls_AreGuarded()
    {
        World world = new();

        Assert.Throws<InvalidOperationException>(() => world.Start());
        Assert.Throws<InvalidOperationException>(() => world.Update());
        Assert.Throws<InvalidOperationException>(() => world.Render());
        Assert.Throws<InvalidOperationException>(() => world.Destroy());

        world.Initialize();

        Assert.Throws<InvalidOperationException>(() => world.Initialize());

        world.Dispose();

        Assert.Throws<InvalidOperationException>(() => world.Dispose());
    }

    [Fact]
    public void Dispose_FromStarted_AutoDestroysThenDisposes()
    {
        List<string> log = [];
        Entity entity = new();
        SpyBehaviorComponent behavior = new("behavior", log);
        SpyUpdateSystem updateSystem = new("update", log);
        SpyRenderSystem renderSystem = new("render", log);
        entity.AddComponent(behavior);

        World world = new();
        world.AddEntity(entity);
        world.AddSystem(updateSystem);
        world.AddSystem(renderSystem);
        world.Initialize();
        world.Start();
        log.Clear();

        world.Dispose();

        Assert.Equal(
            [
                "behavior.OnDestroy",
                "update.OnDestroy",
                "render.OnDestroy",
                "behavior.Dispose",
                "update.Dispose",
                "render.Dispose"
            ],
            log);

        Assert.Throws<InvalidOperationException>(() => world.Dispose());
    }

    [Fact]
    public void TornDownInstances_DoNotReactivateInAnotherWorld()
    {
        List<string> teardownLog = [];
        Entity entity = new();
        SpyBehaviorComponent behavior = new("behavior", teardownLog);
        SpyUpdateSystem updateSystem = new("update", teardownLog);
        SpyRenderSystem renderSystem = new("render", teardownLog);
        entity.AddComponent(behavior);

        World firstWorld = new();
        firstWorld.AddEntity(entity);
        firstWorld.AddSystem(updateSystem);
        firstWorld.AddSystem(renderSystem);
        firstWorld.Initialize();
        firstWorld.Start();
        teardownLog.Clear();

        firstWorld.RemoveEntity(entity);
        firstWorld.RemoveSystem(updateSystem);
        firstWorld.RemoveSystem(renderSystem);
        firstWorld.Update();

        Assert.Equal(
            [
                "behavior.OnDestroy",
                "behavior.Dispose",
                "update.OnDestroy",
                "update.Dispose",
                "render.OnDestroy",
                "render.Dispose"
            ],
            teardownLog);

        teardownLog.Clear();

        World secondWorld = new();
        secondWorld.AddEntity(entity);
        secondWorld.AddSystem(updateSystem);
        secondWorld.AddSystem(renderSystem);
        secondWorld.Initialize();
        secondWorld.Start();
        secondWorld.Update();
        secondWorld.Render();

        Assert.Empty(teardownLog);
        Assert.Empty(secondWorld.RegisteredComponents);
        Assert.Empty(secondWorld.RegisteredBehaviorComponents);
        Assert.Empty(secondWorld.UpdateSystems);
        Assert.Empty(secondWorld.RenderSystems);
    }

    private static World CreateStartedWorld(Entity? entity = null)
    {
        World world = new();

        if (entity != null)
        {
            world.AddEntity(entity);
        }

        world.Initialize();
        world.Start();
        return world;
    }

    private sealed class SpyComponent : Component
    {
        private readonly string _name;
        private readonly List<string> _log;

        public SpyComponent(string name, List<string> log)
        {
            _name = name;
            _log = log;
        }

        public override void Initialize()
        {
            _log.Add($"{_name}.Initialize");
        }

        public override void Dispose()
        {
            _log.Add($"{_name}.Dispose");
        }
    }

    private sealed class SpyBehaviorComponent : BehaviorComponent
    {
        private readonly string _name;
        private readonly List<string> _log;
        private readonly Action? _onUpdate;

        public SpyBehaviorComponent(string name, List<string> log, Action? onUpdate = null)
        {
            _name = name;
            _log = log;
            _onUpdate = onUpdate;
        }

        public override void Initialize()
        {
            _log.Add($"{_name}.Initialize");
        }

        public override void OnStart()
        {
            _log.Add($"{_name}.OnStart");
        }

        public override void Update()
        {
            _log.Add($"{_name}.Update");
            _onUpdate?.Invoke();
        }

        public override void OnDestroy()
        {
            _log.Add($"{_name}.OnDestroy");
        }

        public override void Dispose()
        {
            _log.Add($"{_name}.Dispose");
        }
    }

    private sealed class SpyUpdateSystem : UpdateSystem
    {
        private readonly string _name;
        private readonly List<string> _log;
        private readonly Action? _onUpdate;

        public SpyUpdateSystem(string name, List<string> log, SystemGroup group = SystemGroup.Default, Action? onUpdate = null)
        {
            _name = name;
            _log = log;
            _onUpdate = onUpdate;
            Group = group;
        }

        public override void Initialize()
        {
            _log.Add($"{_name}.Initialize");
        }

        public override void OnStart()
        {
            _log.Add($"{_name}.OnStart");
        }

        public override void Update()
        {
            _log.Add($"{_name}.Update");
            _onUpdate?.Invoke();
        }

        public override void OnDestroy()
        {
            _log.Add($"{_name}.OnDestroy");
        }

        public override void Dispose()
        {
            _log.Add($"{_name}.Dispose");
        }
    }

    private sealed class SpyRenderSystem : RenderSystem
    {
        private readonly string _name;
        private readonly List<string> _log;

        public SpyRenderSystem(string name, List<string> log)
        {
            _name = name;
            _log = log;
        }

        public override void Initialize()
        {
            _log.Add($"{_name}.Initialize");
        }

        public override void OnStart()
        {
            _log.Add($"{_name}.OnStart");
        }

        public override void Render()
        {
            _log.Add($"{_name}.Render");
        }

        public override void OnDestroy()
        {
            _log.Add($"{_name}.OnDestroy");
        }

        public override void Dispose()
        {
            _log.Add($"{_name}.Dispose");
        }
    }
}
