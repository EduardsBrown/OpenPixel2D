using OpenPixel2D.Engine;

namespace OpenPixel2D.Engine.Tests;

public sealed class PublicApiLifecycleMoveTests
{
    [Fact]
    public void DetachedSubtree_AttachedBeforeStartup_FollowsLifecycleThroughRootAdd()
    {
        LifecycleRecorder recorder = new();
        World world = new();
        Entity root = new();
        Entity child = new();
        RecordingComponent rootPassive = new("RootPassive", recorder);
        RecordingBehaviorComponent childBehavior = new("ChildBehavior", recorder);

        root.AddComponent(rootPassive);
        child.AddComponent(childBehavior);
        root.AddEntity(child);

        Assert.Empty(recorder.Events);

        world.AddEntity(root);

        Assert.Single(world.Entities);
        Assert.Same(root, world.Entities[0]);
        Assert.Same(world, root.World);
        Assert.Same(world, child.World);
        Assert.Same(root, child.Parent);
        Assert.Same(root, rootPassive.Parent);
        Assert.Same(child, childBehavior.Parent);
        Assert.Empty(recorder.Events);

        world.Initialize();
        world.Start();
        world.Update();
        world.Destroy();
        world.Dispose();

        Assert.Equal(
            [
                "RootPassive.Initialize",
                "ChildBehavior.Initialize",
                "ChildBehavior.OnStart",
                "ChildBehavior.Update",
                "ChildBehavior.OnDestroy",
                "RootPassive.Dispose",
                "ChildBehavior.Dispose"
            ],
            recorder.Events);
    }

    [Fact]
    public void MoveEntityFromRootToChildInSameWorld_DoesNotDuplicateLifecycle()
    {
        LifecycleRecorder recorder = new();
        World world = new();
        Entity player = new();
        Entity backpack = new();
        Entity sword = new();
        RecordingBehaviorComponent swordBehavior = new("SwordBehavior", recorder);

        sword.AddComponent(swordBehavior);
        world.AddEntity(player);
        world.AddEntity(backpack);
        world.AddEntity(sword);
        world.Initialize();
        world.Start();
        recorder.Clear();

        backpack.AddEntity(sword);

        Assert.Equal(2, world.Entities.Count);
        Assert.Same(backpack, sword.Parent);
        Assert.Same(world, sword.World);
        Assert.Empty(recorder.Events);

        world.Update();
        world.Destroy();
        world.Dispose();

        Assert.Equal(
            [
                "SwordBehavior.Update",
                "SwordBehavior.OnDestroy",
                "SwordBehavior.Dispose"
            ],
            recorder.Events);
        Assert.Equal(0, recorder.Count("SwordBehavior.Initialize"));
        Assert.Equal(0, recorder.Count("SwordBehavior.OnStart"));
    }

    [Fact]
    public void MoveEntityFromChildToRootInSameWorld_DoesNotDuplicateLifecycle()
    {
        LifecycleRecorder recorder = new();
        World world = new();
        Entity player = new();
        Entity pet = new();
        RecordingBehaviorComponent petBehavior = new("PetBehavior", recorder);

        pet.AddComponent(petBehavior);
        player.AddEntity(pet);
        world.AddEntity(player);
        world.Initialize();
        world.Start();
        recorder.Clear();

        world.AddEntity(pet);

        Assert.Equal(2, world.Entities.Count);
        Assert.Null(pet.Parent);
        Assert.Same(world, pet.World);
        Assert.Empty(recorder.Events);

        world.Update();
        world.Destroy();
        world.Dispose();

        Assert.Equal(
            [
                "PetBehavior.Update",
                "PetBehavior.OnDestroy",
                "PetBehavior.Dispose"
            ],
            recorder.Events);
        Assert.Equal(0, recorder.Count("PetBehavior.Initialize"));
        Assert.Equal(0, recorder.Count("PetBehavior.OnStart"));
    }

    [Fact]
    public void PendingSubtreeMovedAcrossWorlds_ActivatesOnlyInFinalWorld()
    {
        LifecycleRecorder recorder = new();
        World firstWorld = new();
        World secondWorld = new();
        Entity root = new();
        Entity child = new();
        RecordingBehaviorComponent traveler = new("Traveler", recorder);

        child.AddComponent(traveler);
        root.AddEntity(child);

        firstWorld.AddEntity(root);
        secondWorld.AddEntity(root);

        Assert.Empty(firstWorld.Entities);
        Assert.Single(secondWorld.Entities);
        Assert.Same(secondWorld, root.World);
        Assert.Same(secondWorld, child.World);
        Assert.Empty(recorder.Events);

        firstWorld.Initialize();
        firstWorld.Start();
        firstWorld.Update();
        firstWorld.Destroy();
        firstWorld.Dispose();

        Assert.Empty(recorder.Events);

        secondWorld.Initialize();
        secondWorld.Start();
        secondWorld.Update();
        secondWorld.Destroy();
        secondWorld.Dispose();

        Assert.Equal(
            [
                "Traveler.Initialize",
                "Traveler.OnStart",
                "Traveler.Update",
                "Traveler.OnDestroy",
                "Traveler.Dispose"
            ],
            recorder.Events);
    }

    [Fact]
    public void StartedSubtreeMovedToStartedWorld_TearsDownInOldWorldAndDoesNotReactivate()
    {
        LifecycleRecorder recorder = new();
        World oldWorld = new();
        World newWorld = CreateStartedWorld();
        Entity enemy = new();
        RecordingBehaviorComponent enemyBehavior = new("EnemyBehavior", recorder);

        enemy.AddComponent(enemyBehavior);
        oldWorld.AddEntity(enemy);
        oldWorld.Initialize();
        oldWorld.Start();
        recorder.Clear();

        newWorld.AddEntity(enemy);

        Assert.Empty(oldWorld.Entities);
        Assert.Single(newWorld.Entities);
        Assert.Same(newWorld, enemy.World);
        Assert.Empty(recorder.Events);

        newWorld.Update();
        Assert.Empty(recorder.Events);

        oldWorld.Update();
        Assert.Equal(
            [
                "EnemyBehavior.OnDestroy",
                "EnemyBehavior.Dispose"
            ],
            recorder.Events);

        newWorld.Update();
        newWorld.Destroy();
        newWorld.Dispose();
        oldWorld.Destroy();
        oldWorld.Dispose();

        Assert.Equal(
            [
                "EnemyBehavior.OnDestroy",
                "EnemyBehavior.Dispose"
            ],
            recorder.Events);
    }

    [Fact]
    public void StartedSubtreeMovedToInitializedWorld_DoesNotStartInDestinationAndStillTearsDownInSource()
    {
        LifecycleRecorder recorder = new();
        World sourceWorld = new();
        World destinationWorld = new();
        Entity enemy = new();
        RecordingBehaviorComponent enemyBehavior = new("EnemyBehavior", recorder);

        enemy.AddComponent(enemyBehavior);
        sourceWorld.AddEntity(enemy);
        sourceWorld.Initialize();
        sourceWorld.Start();
        destinationWorld.Initialize();
        recorder.Clear();

        destinationWorld.AddEntity(enemy);

        Assert.Empty(sourceWorld.Entities);
        Assert.Single(destinationWorld.Entities);
        Assert.Same(destinationWorld, enemy.World);
        Assert.Empty(recorder.Events);

        destinationWorld.Start();
        destinationWorld.Update();
        Assert.Empty(recorder.Events);

        sourceWorld.Update();
        Assert.Equal(
            [
                "EnemyBehavior.OnDestroy",
                "EnemyBehavior.Dispose"
            ],
            recorder.Events);

        destinationWorld.Update();
        destinationWorld.Destroy();
        destinationWorld.Dispose();
        sourceWorld.Destroy();
        sourceWorld.Dispose();

        Assert.Equal(
            [
                "EnemyBehavior.OnDestroy",
                "EnemyBehavior.Dispose"
            ],
            recorder.Events);
    }

    [Fact]
    public void OfflineSubtreeAttachedToStartedWorld_ActivatesOnNextUpdateWithoutImmediateLifecycle()
    {
        LifecycleRecorder recorder = new();
        World world = CreateStartedWorld();
        Entity root = new();
        Entity child = new();
        RecordingComponent rootPassive = new("RootPassive", recorder);
        RecordingBehaviorComponent childBehavior = new("ChildBehavior", recorder);

        root.AddComponent(rootPassive);
        child.AddComponent(childBehavior);
        root.AddEntity(child);

        world.AddEntity(root);

        Assert.Single(world.Entities);
        Assert.Same(root, world.Entities[0]);
        Assert.Same(world, root.World);
        Assert.Same(world, child.World);
        Assert.Same(root, child.Parent);
        Assert.Empty(recorder.Events);

        world.Render();
        Assert.Empty(recorder.Events);

        world.Update();
        Assert.Equal(
            [
                "RootPassive.Initialize",
                "ChildBehavior.Initialize",
                "ChildBehavior.OnStart",
                "ChildBehavior.Update"
            ],
            recorder.Events);

        world.Update();
        world.Destroy();
        world.Dispose();

        Assert.Equal(
            [
                "RootPassive.Initialize",
                "ChildBehavior.Initialize",
                "ChildBehavior.OnStart",
                "ChildBehavior.Update",
                "ChildBehavior.Update",
                "ChildBehavior.OnDestroy",
                "RootPassive.Dispose",
                "ChildBehavior.Dispose"
            ],
            recorder.Events);
    }

    private static World CreateStartedWorld()
    {
        World world = new();
        world.Initialize();
        world.Start();
        return world;
    }
}
