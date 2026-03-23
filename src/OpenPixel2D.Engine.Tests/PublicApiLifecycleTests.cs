using OpenPixel2D.Engine;

namespace OpenPixel2D.Engine.Tests;

public sealed class PublicApiLifecycleTests
{
    [Fact]
    public void PassiveComponent_AddedBeforeInitialize_FollowsPassiveLifecycle()
    {
        LifecycleRecorder recorder = new();
        World world = new();
        Entity entity = new();
        RecordingComponent passive = new("Passive", recorder);

        entity.AddComponent(passive);
        world.AddEntity(entity);

        Assert.Empty(recorder.Events);

        world.Initialize();
        Assert.Equal(["Passive.Initialize"], recorder.Events);

        world.Start();
        world.Destroy();
        world.Dispose();

        Assert.Equal(
            [
                "Passive.Initialize",
                "Passive.Dispose"
            ],
            recorder.Events);
        Assert.Equal(1, recorder.Count("Passive.Initialize"));
        Assert.Equal(1, recorder.Count("Passive.Dispose"));
        Assert.Equal(0, recorder.Count("Passive.OnStart"));
        Assert.Equal(0, recorder.Count("Passive.OnDestroy"));
    }

    [Fact]
    public void BehaviorComponent_AddedBeforeInitialize_FollowsFullLifecycle()
    {
        LifecycleRecorder recorder = new();
        World world = new();
        Entity entity = new();
        RecordingBehaviorComponent behavior = new("Behavior", recorder);

        entity.AddComponent(behavior);
        world.AddEntity(entity);

        Assert.Empty(recorder.Events);

        world.Initialize();
        world.Start();
        world.Update();
        world.Destroy();
        world.Dispose();

        Assert.Equal(
            [
                "Behavior.Initialize",
                "Behavior.OnStart",
                "Behavior.Update",
                "Behavior.OnDestroy",
                "Behavior.Dispose"
            ],
            recorder.Events);
    }

    [Fact]
    public void UpdateSystem_AddedBeforeInitialize_FollowsFullLifecycle()
    {
        LifecycleRecorder recorder = new();
        World world = new();
        RecordingUpdateSystem updateSystem = new("UpdateSystem", recorder);

        world.AddSystem(updateSystem);

        Assert.Empty(recorder.Events);

        world.Initialize();
        world.Start();
        world.Update();
        world.Destroy();
        world.Dispose();

        Assert.Equal(
            [
                "UpdateSystem.Initialize",
                "UpdateSystem.OnStart",
                "UpdateSystem.Update",
                "UpdateSystem.OnDestroy",
                "UpdateSystem.Dispose"
            ],
            recorder.Events);
    }

    [Fact]
    public void RenderSystem_AddedBeforeInitialize_FollowsFullLifecycle()
    {
        LifecycleRecorder recorder = new();
        World world = new();
        RecordingRenderSystem renderSystem = new("RenderSystem", recorder);

        world.AddSystem(renderSystem);

        Assert.Empty(recorder.Events);

        world.Initialize();
        world.Start();
        world.Render(null);
        world.Destroy();
        world.Dispose();

        Assert.Equal(
            [
                "RenderSystem.Initialize",
                "RenderSystem.OnStart",
                "RenderSystem.Render",
                "RenderSystem.OnDestroy",
                "RenderSystem.Dispose"
            ],
            recorder.Events);
    }

    [Fact]
    public void PassiveComponent_AddedAfterInitialize_WaitsForStartAndNeverStarts()
    {
        LifecycleRecorder recorder = new();
        World world = new();
        Entity entity = new();
        RecordingComponent passive = new("Passive", recorder);

        world.Initialize();
        entity.AddComponent(passive);
        world.AddEntity(entity);

        Assert.Empty(recorder.Events);

        world.Start();
        world.Update();
        world.Destroy();
        world.Dispose();

        Assert.Equal(
            [
                "Passive.Initialize",
                "Passive.Dispose"
            ],
            recorder.Events);
        Assert.Equal(0, recorder.Count("Passive.OnStart"));
    }

    [Fact]
    public void BehaviorComponent_AddedAfterInitialize_CatchesUpAtStart()
    {
        LifecycleRecorder recorder = new();
        World world = new();
        Entity entity = new();
        RecordingBehaviorComponent behavior = new("Behavior", recorder);

        world.Initialize();
        entity.AddComponent(behavior);
        world.AddEntity(entity);

        Assert.Empty(recorder.Events);

        world.Start();
        world.Update();
        world.Destroy();
        world.Dispose();

        Assert.Equal(
            [
                "Behavior.Initialize",
                "Behavior.OnStart",
                "Behavior.Update",
                "Behavior.OnDestroy",
                "Behavior.Dispose"
            ],
            recorder.Events);
    }

    [Fact]
    public void UpdateSystem_AddedAfterInitialize_CatchesUpAtStart()
    {
        LifecycleRecorder recorder = new();
        World world = new();
        RecordingUpdateSystem updateSystem = new("UpdateSystem", recorder);

        world.Initialize();
        world.AddSystem(updateSystem);

        Assert.Empty(recorder.Events);

        world.Start();
        world.Update();
        world.Destroy();
        world.Dispose();

        Assert.Equal(
            [
                "UpdateSystem.Initialize",
                "UpdateSystem.OnStart",
                "UpdateSystem.Update",
                "UpdateSystem.OnDestroy",
                "UpdateSystem.Dispose"
            ],
            recorder.Events);
    }

    [Fact]
    public void RenderSystem_AddedAfterInitialize_CatchesUpAtStart()
    {
        LifecycleRecorder recorder = new();
        World world = new();
        RecordingRenderSystem renderSystem = new("RenderSystem", recorder);

        world.Initialize();
        world.AddSystem(renderSystem);

        Assert.Empty(recorder.Events);

        world.Start();
        world.Render(null);
        world.Destroy();
        world.Dispose();

        Assert.Equal(
            [
                "RenderSystem.Initialize",
                "RenderSystem.OnStart",
                "RenderSystem.Render",
                "RenderSystem.OnDestroy",
                "RenderSystem.Dispose"
            ],
            recorder.Events);
    }

    [Fact]
    public void BehaviorComponent_AddedAfterStart_ActivatesOnNextUpdateOnly()
    {
        LifecycleRecorder recorder = new();
        World world = CreateStartedWorld();
        Entity entity = new();
        RecordingBehaviorComponent behavior = new("Behavior", recorder);

        entity.AddComponent(behavior);
        world.AddEntity(entity);

        Assert.Empty(recorder.Events);

        world.Render(null);
        Assert.Empty(recorder.Events);

        world.Update();

        Assert.Equal(
            [
                "Behavior.Initialize",
                "Behavior.OnStart",
                "Behavior.Update"
            ],
            recorder.Events);

        world.Update();
        world.Destroy();
        world.Dispose();

        Assert.Equal(
            [
                "Behavior.Initialize",
                "Behavior.OnStart",
                "Behavior.Update",
                "Behavior.Update",
                "Behavior.OnDestroy",
                "Behavior.Dispose"
            ],
            recorder.Events);
        AssertLoggedOnce(recorder, "Behavior.Initialize", "Behavior.OnStart", "Behavior.OnDestroy", "Behavior.Dispose");
    }

    [Fact]
    public void PassiveComponent_AddedAfterStart_InitializesOnNextUpdateWithoutRuntimeCallbacks()
    {
        LifecycleRecorder recorder = new();
        World world = CreateStartedWorld();
        Entity entity = new();
        RecordingComponent passive = new("Passive", recorder);

        entity.AddComponent(passive);
        world.AddEntity(entity);

        Assert.Empty(recorder.Events);

        world.Render(null);
        Assert.Empty(recorder.Events);

        world.Update();
        world.Update();
        world.Destroy();
        world.Dispose();

        Assert.Equal(
            [
                "Passive.Initialize",
                "Passive.Dispose"
            ],
            recorder.Events);
    }

    [Fact]
    public void UpdateSystem_AddedAfterStart_ActivatesOnNextUpdateOnly()
    {
        LifecycleRecorder recorder = new();
        World world = CreateStartedWorld();
        RecordingUpdateSystem updateSystem = new("UpdateSystem", recorder);

        world.AddSystem(updateSystem);

        Assert.Empty(recorder.Events);

        world.Render(null);
        Assert.Empty(recorder.Events);

        world.Update();

        Assert.Equal(
            [
                "UpdateSystem.Initialize",
                "UpdateSystem.OnStart",
                "UpdateSystem.Update"
            ],
            recorder.Events);

        world.Update();
        world.Destroy();
        world.Dispose();

        Assert.Equal(
            [
                "UpdateSystem.Initialize",
                "UpdateSystem.OnStart",
                "UpdateSystem.Update",
                "UpdateSystem.Update",
                "UpdateSystem.OnDestroy",
                "UpdateSystem.Dispose"
            ],
            recorder.Events);
    }

    [Fact]
    public void RenderSystem_AddedAfterStart_ActivatesOnNextUpdateThenRenders()
    {
        LifecycleRecorder recorder = new();
        World world = CreateStartedWorld();
        RecordingRenderSystem renderSystem = new("RenderSystem", recorder);

        world.AddSystem(renderSystem);

        Assert.Empty(recorder.Events);

        world.Render(null);
        Assert.Empty(recorder.Events);

        world.Update();
        Assert.Equal(
            [
                "RenderSystem.Initialize",
                "RenderSystem.OnStart"
            ],
            recorder.Events);

        world.Render(null);
        world.Render(null);
        world.Destroy();
        world.Dispose();

        Assert.Equal(
            [
                "RenderSystem.Initialize",
                "RenderSystem.OnStart",
                "RenderSystem.Render",
                "RenderSystem.Render",
                "RenderSystem.OnDestroy",
                "RenderSystem.Dispose"
            ],
            recorder.Events);
    }

    [Fact]
    public void DeveloperSpawnsGameplayObjectsDuringRuntime_AndTheyActivateOnTheFollowingFrame()
    {
        LifecycleRecorder spawnedRecorder = new();
        World world = CreateStartedWorld();
        Entity enemy = new();
        RecordingBehaviorComponent enemyBrain = new("EnemyBrain", spawnedRecorder);
        RecordingUpdateSystem enemyLogic = new("EnemyLogic", spawnedRecorder);
        RecordingRenderSystem impactEffect = new("ImpactEffect", spawnedRecorder);
        enemy.AddComponent(enemyBrain);

        RecordingUpdateSystem? spawner = null;
        spawner = new RecordingUpdateSystem(
            "Spawner",
            new LifecycleRecorder(),
            onUpdate: () =>
            {
                world.AddEntity(enemy);
                world.AddSystem(enemyLogic);
                world.AddSystem(impactEffect);
                world.RemoveSystem(spawner!);
            });

        world.AddSystem(spawner);
        world.Update();
        world.Render(null);

        Assert.Empty(spawnedRecorder.Events);

        world.Update();

        Assert.Equal(
            [
                "EnemyBrain.Initialize",
                "EnemyLogic.Initialize",
                "ImpactEffect.Initialize",
                "EnemyBrain.OnStart",
                "EnemyLogic.OnStart",
                "ImpactEffect.OnStart",
                "EnemyLogic.Update",
                "EnemyBrain.Update"
            ],
            spawnedRecorder.Events);

        world.Render(null);
        world.Destroy();
        world.Dispose();

        Assert.Equal(
            [
                "EnemyBrain.Initialize",
                "EnemyLogic.Initialize",
                "ImpactEffect.Initialize",
                "EnemyBrain.OnStart",
                "EnemyLogic.OnStart",
                "ImpactEffect.OnStart",
                "EnemyLogic.Update",
                "EnemyBrain.Update",
                "ImpactEffect.Render",
                "EnemyBrain.OnDestroy",
                "EnemyLogic.OnDestroy",
                "ImpactEffect.OnDestroy",
                "EnemyBrain.Dispose",
                "EnemyLogic.Dispose",
                "ImpactEffect.Dispose"
            ],
            spawnedRecorder.Events);
    }

    [Fact]
    public void BehaviorComponent_RemovedAfterStart_IsDestroyedAndDisposedOnNextUpdate()
    {
        LifecycleRecorder recorder = new();
        World world = new();
        Entity entity = new();
        RecordingBehaviorComponent behavior = new("Behavior", recorder);

        entity.AddComponent(behavior);
        world.AddEntity(entity);
        world.Initialize();
        world.Start();
        recorder.Clear();

        entity.RemoveComponent(behavior);

        Assert.Empty(recorder.Events);

        world.Update();
        world.Update();
        world.Destroy();
        world.Dispose();

        Assert.Equal(
            [
                "Behavior.OnDestroy",
                "Behavior.Dispose"
            ],
            recorder.Events);
    }

    [Fact]
    public void UpdateSystem_RemovedAfterStart_IsDestroyedAndDisposedOnNextUpdate()
    {
        LifecycleRecorder recorder = new();
        World world = new();
        RecordingUpdateSystem updateSystem = new("UpdateSystem", recorder);

        world.AddSystem(updateSystem);
        world.Initialize();
        world.Start();
        recorder.Clear();

        world.RemoveSystem(updateSystem);

        Assert.Empty(recorder.Events);

        world.Update();
        world.Update();
        world.Destroy();
        world.Dispose();

        Assert.Equal(
            [
                "UpdateSystem.OnDestroy",
                "UpdateSystem.Dispose"
            ],
            recorder.Events);
    }

    [Fact]
    public void RenderSystem_RemovedAfterStart_RendersUntilFlushThenStops()
    {
        LifecycleRecorder recorder = new();
        World world = new();
        RecordingRenderSystem renderSystem = new("RenderSystem", recorder);

        world.AddSystem(renderSystem);
        world.Initialize();
        world.Start();
        recorder.Clear();

        world.RemoveSystem(renderSystem);

        Assert.Empty(recorder.Events);

        world.Render(null);
        world.Update();
        world.Render(null);
        world.Destroy();
        world.Dispose();

        Assert.Equal(
            [
                "RenderSystem.Render",
                "RenderSystem.OnDestroy",
                "RenderSystem.Dispose"
            ],
            recorder.Events);
    }

    [Fact]
    public void PassiveComponent_RemovedAfterStart_DisposesWithoutDestroy()
    {
        LifecycleRecorder recorder = new();
        World world = new();
        Entity entity = new();
        RecordingComponent passive = new("Passive", recorder);

        entity.AddComponent(passive);
        world.AddEntity(entity);
        world.Initialize();
        world.Start();
        recorder.Clear();

        entity.RemoveComponent(passive);

        Assert.Empty(recorder.Events);

        world.Update();
        world.Update();
        world.Destroy();
        world.Dispose();

        Assert.Equal(
            [
                "Passive.Dispose"
            ],
            recorder.Events);
        Assert.Equal(0, recorder.Count("Passive.OnDestroy"));
    }

    [Fact]
    public void AddedThenRemovedBeforeActivation_ReceivesNoLifecycleAcrossAllObjectTypes()
    {
        LifecycleRecorder recorder = new();
        World world = CreateStartedWorld();
        Entity entity = new();
        RecordingComponent passive = new("Passive", recorder);
        RecordingBehaviorComponent behavior = new("Behavior", recorder);
        RecordingUpdateSystem updateSystem = new("UpdateSystem", recorder);
        RecordingRenderSystem renderSystem = new("RenderSystem", recorder);

        entity.AddComponent(passive);
        entity.AddComponent(behavior);
        world.AddEntity(entity);
        world.AddSystem(updateSystem);
        world.AddSystem(renderSystem);

        world.RemoveEntity(entity);
        world.RemoveSystem(updateSystem);
        world.RemoveSystem(renderSystem);

        world.Update();
        world.Render(null);
        world.Destroy();
        world.Dispose();

        Assert.Empty(recorder.Events);
    }

    [Fact]
    public void LifecycleOrdering_AlwaysKeepsInitializeBeforeStartAndDestroyBeforeDispose()
    {
        LifecycleRecorder recorder = new();
        World world = CreateStartedWorld();
        Entity entity = new();
        RecordingBehaviorComponent behavior = new("Behavior", recorder);
        RecordingUpdateSystem updateSystem = new("UpdateSystem", recorder);
        RecordingRenderSystem renderSystem = new("RenderSystem", recorder);

        entity.AddComponent(behavior);
        world.AddEntity(entity);
        world.AddSystem(updateSystem);
        world.AddSystem(renderSystem);

        world.Update();
        world.Render(null);
        world.Destroy();
        world.Dispose();

        AssertEventOrder(recorder, "Behavior.Initialize", "Behavior.OnStart");
        AssertEventOrder(recorder, "Behavior.OnStart", "Behavior.Update");
        AssertEventOrder(recorder, "Behavior.OnDestroy", "Behavior.Dispose");
        AssertEventOrder(recorder, "UpdateSystem.Initialize", "UpdateSystem.OnStart");
        AssertEventOrder(recorder, "UpdateSystem.OnStart", "UpdateSystem.Update");
        AssertEventOrder(recorder, "UpdateSystem.OnDestroy", "UpdateSystem.Dispose");
        AssertEventOrder(recorder, "RenderSystem.Initialize", "RenderSystem.OnStart");
        AssertEventOrder(recorder, "RenderSystem.OnStart", "RenderSystem.Render");
        AssertEventOrder(recorder, "RenderSystem.OnDestroy", "RenderSystem.Dispose");
        AssertLoggedOnce(
            recorder,
            "Behavior.Initialize",
            "Behavior.OnStart",
            "Behavior.OnDestroy",
            "Behavior.Dispose",
            "UpdateSystem.Initialize",
            "UpdateSystem.OnStart",
            "UpdateSystem.OnDestroy",
            "UpdateSystem.Dispose",
            "RenderSystem.Initialize",
            "RenderSystem.OnStart",
            "RenderSystem.OnDestroy",
            "RenderSystem.Dispose");
    }

    [Fact]
    public void DeveloperBuildsSmallSceneBeforeStartup_AndSeesPredictableLifecycle()
    {
        LifecycleRecorder recorder = new();
        World world = new();
        Entity player = new();
        Entity weapon = new();
        RecordingComponent sprite = new("PlayerSprite", recorder);
        RecordingBehaviorComponent weaponBehavior = new("WeaponBehavior", recorder);
        RecordingUpdateSystem gameplayLoop = new("GameplayLoop", recorder);
        RecordingRenderSystem sceneRender = new("SceneRender", recorder);

        player.AddComponent(sprite);
        weapon.AddComponent(weaponBehavior);
        player.AddEntity(weapon);
        world.AddEntity(player);
        world.AddSystem(gameplayLoop);
        world.AddSystem(sceneRender);

        Assert.Empty(recorder.Events);

        world.Initialize();
        world.Start();
        world.Update();
        world.Render(null);
        world.Destroy();
        world.Dispose();

        Assert.Equal(
            [
                "PlayerSprite.Initialize",
                "WeaponBehavior.Initialize",
                "GameplayLoop.Initialize",
                "SceneRender.Initialize",
                "WeaponBehavior.OnStart",
                "GameplayLoop.OnStart",
                "SceneRender.OnStart",
                "GameplayLoop.Update",
                "WeaponBehavior.Update",
                "SceneRender.Render",
                "WeaponBehavior.OnDestroy",
                "GameplayLoop.OnDestroy",
                "SceneRender.OnDestroy",
                "PlayerSprite.Dispose",
                "WeaponBehavior.Dispose",
                "GameplayLoop.Dispose",
                "SceneRender.Dispose"
            ],
            recorder.Events);
    }

    [Fact]
    public void DeveloperRemovesGameplayObjectsDuringRuntime_AndTeardownIsDeferred()
    {
        LifecycleRecorder recorder = new();
        World world = new();
        Entity enemy = new();
        RecordingBehaviorComponent enemyBrain = new("EnemyBrain", recorder);
        RecordingRenderSystem hitFlash = new("HitFlash", recorder);

        enemy.AddComponent(enemyBrain);
        world.AddEntity(enemy);
        world.AddSystem(hitFlash);
        world.Initialize();
        world.Start();
        recorder.Clear();

        world.RemoveEntity(enemy);
        world.RemoveSystem(hitFlash);

        Assert.Empty(recorder.Events);

        world.Render(null);
        world.Update();
        world.Render(null);
        world.Destroy();
        world.Dispose();

        Assert.Equal(
            [
                "HitFlash.Render",
                "EnemyBrain.OnDestroy",
                "EnemyBrain.Dispose",
                "HitFlash.OnDestroy",
                "HitFlash.Dispose"
            ],
            recorder.Events);
    }

    [Fact]
    public void RepeatedUpdateAndRender_DoNotRepeatNonRepeatableLifecycle()
    {
        LifecycleRecorder recorder = new();
        World world = new();
        Entity entity = new();
        RecordingBehaviorComponent behavior = new("Behavior", recorder);
        RecordingUpdateSystem updateSystem = new("UpdateSystem", recorder);
        RecordingRenderSystem renderSystem = new("RenderSystem", recorder);

        entity.AddComponent(behavior);
        world.AddEntity(entity);
        world.AddSystem(updateSystem);
        world.AddSystem(renderSystem);
        world.Initialize();
        world.Start();
        world.Update();
        world.Update();
        world.Render(null);
        world.Render(null);
        world.Destroy();
        world.Dispose();

        Assert.Equal(1, recorder.Count("Behavior.Initialize"));
        Assert.Equal(1, recorder.Count("Behavior.OnStart"));
        Assert.Equal(2, recorder.Count("Behavior.Update"));
        Assert.Equal(1, recorder.Count("Behavior.OnDestroy"));
        Assert.Equal(1, recorder.Count("Behavior.Dispose"));
        Assert.Equal(1, recorder.Count("UpdateSystem.Initialize"));
        Assert.Equal(1, recorder.Count("UpdateSystem.OnStart"));
        Assert.Equal(2, recorder.Count("UpdateSystem.Update"));
        Assert.Equal(1, recorder.Count("UpdateSystem.OnDestroy"));
        Assert.Equal(1, recorder.Count("UpdateSystem.Dispose"));
        Assert.Equal(1, recorder.Count("RenderSystem.Initialize"));
        Assert.Equal(1, recorder.Count("RenderSystem.OnStart"));
        Assert.Equal(2, recorder.Count("RenderSystem.Render"));
        Assert.Equal(1, recorder.Count("RenderSystem.OnDestroy"));
        Assert.Equal(1, recorder.Count("RenderSystem.Dispose"));
    }

    [Fact]
    public void WorldLifecycleCommands_AreGuardedByTheCurrentPublicContract()
    {
        World createdWorld = new();

        Assert.Throws<InvalidOperationException>(() => createdWorld.Start());
        Assert.Throws<InvalidOperationException>(() => createdWorld.Update());
        Assert.Throws<InvalidOperationException>(() => createdWorld.Render(null));
        Assert.Throws<InvalidOperationException>(() => createdWorld.Destroy());

        World initializedWorld = new();
        initializedWorld.Initialize();

        Assert.Throws<InvalidOperationException>(() => initializedWorld.Initialize());
        Assert.Throws<InvalidOperationException>(() => initializedWorld.Update());
        Assert.Throws<InvalidOperationException>(() => initializedWorld.Render(null));
        Assert.Throws<InvalidOperationException>(() => initializedWorld.Destroy());

        initializedWorld.Dispose();
    }

    [Fact]
    public void DisposeFromStartedWorld_AutoDestroysThenDisposesAndDisposeTwiceThrows()
    {
        LifecycleRecorder recorder = new();
        World world = new();
        Entity entity = new();
        RecordingBehaviorComponent behavior = new("Behavior", recorder);
        RecordingUpdateSystem updateSystem = new("UpdateSystem", recorder);
        RecordingRenderSystem renderSystem = new("RenderSystem", recorder);

        entity.AddComponent(behavior);
        world.AddEntity(entity);
        world.AddSystem(updateSystem);
        world.AddSystem(renderSystem);
        world.Initialize();
        world.Start();
        recorder.Clear();

        world.Dispose();

        Assert.Equal(
            [
                "Behavior.OnDestroy",
                "UpdateSystem.OnDestroy",
                "RenderSystem.OnDestroy",
                "Behavior.Dispose",
                "UpdateSystem.Dispose",
                "RenderSystem.Dispose"
            ],
            recorder.Events);

        Assert.Throws<InvalidOperationException>(() => world.Dispose());
    }

    private static World CreateStartedWorld()
    {
        World world = new();
        world.Initialize();
        world.Start();
        return world;
    }

    private static void AssertLoggedOnce(LifecycleRecorder recorder, params string[] events)
    {
        foreach (string entry in events)
        {
            Assert.Equal(1, recorder.Count(entry));
        }
    }

    private static void AssertEventOrder(LifecycleRecorder recorder, string earlier, string later)
    {
        int earlierIndex = recorder.IndexOf(earlier);
        int laterIndex = recorder.IndexOf(later);

        Assert.True(earlierIndex >= 0, $"Expected to find '{earlier}' in the log.");
        Assert.True(laterIndex >= 0, $"Expected to find '{later}' in the log.");
        Assert.True(earlierIndex < laterIndex, $"Expected '{earlier}' to appear before '{later}'.");
    }
}
